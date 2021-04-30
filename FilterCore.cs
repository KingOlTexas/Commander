using System;
using System.Linq;
using System.Reflection;
using Autofac;
using Commander.Lib.Controllers;
using Commander.Lib.Controllers.Bindings;
using Commander.Lib.Models.Bindings;
using Commander.Lib.Services;
using Commander.Lib.Services.Bindings;
using Commander.Lib.Views.Bindings;
using Decal.Adapter;
using Decal.Adapter.Wrappers;

namespace Commander
{
    [FriendlyName("Commander")]
    public class FilterCore : FilterBase
    {
        private IContainer _container;
        private Logger _logger;
        private Debugger _debugger;
        private static Assembly ExecutingAssembly = Assembly.GetExecutingAssembly();
        private static string[] EmbeddedLibraries =
           ExecutingAssembly.GetManifestResourceNames().Where(x => x.EndsWith(".dll")).ToArray();

        public FilterCore()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyName = new AssemblyName(args.Name).Name + ".dll";
 
            var resourceName = EmbeddedLibraries.FirstOrDefault(x => x.EndsWith(assemblyName));
            if (resourceName == null)
            {
                return null;
            }
 
            using (var stream = ExecutingAssembly.GetManifestResourceStream(resourceName))
            {
                var bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                return Assembly.Load(bytes);
            }
        }

        private void ConfigureServices(NetServiceHost Host, CoreManager Core)
        {
            ContainerBuilder builder = new ContainerBuilder();
            builder.RegisterInstance(Host).As<NetServiceHost>().SingleInstance();
            builder.RegisterInstance(Core).As<CoreManager>().SingleInstance();
            builder.RegisterModule(new ControllersModule());
            builder.RegisterModule(new ServicesModule());
            builder.RegisterModule(new ModelsModule());
            builder.RegisterModule(new ViewsModule());
            _container = builder.Build();

            _logger = _container.Resolve<Logger>().Scope("App");
            _debugger = _container.Resolve<Debugger>();
            _debugger.Start();
        }

        protected override void Startup()
        {
            try
            {
                ConfigureServices(Host, Core);
                _logger.Info("Startup()");
                Core.PluginTermComplete += _container.Resolve<PluginTermCompleteController>().Init;
                Core.FilterInitComplete += FilterInitComplete;
            } catch (Exception ex) { _logger.Error(ex); }
        }

        protected override void Shutdown()
        {
            try
            {
                _logger.Info("ShutDown()");
                Core.CharacterFilter.Login -= _container.Resolve<LoginController>().Init;
                Core.CharacterFilter.LoginComplete -= _container.Resolve<LoginCompleteController>().Init;
                Core.CharacterFilter.Death -= _container.Resolve<DeathController>().Init;
                Core.WorldFilter.CreateObject -= _container.Resolve<CreateObjectController>().Init;
                Core.WorldFilter.MoveObject -= _container.Resolve<MoveObjectController>().Init;
                Core.WorldFilter.ReleaseObject -= _container.Resolve<ReleaseObjectController>().Init;
                Core.PluginTermComplete -= _container.Resolve<PluginTermCompleteController>().Init;
                Core.FilterInitComplete -= FilterInitComplete;
            } catch (Exception ex) { _logger.Error(ex); }
        }

        private void FilterInitComplete(object sender, EventArgs e)
        {
            try
            {
                _logger.Info("FilterInitComplete()");
                Core.CharacterFilter.Login += _container.Resolve<LoginController>().Init;
                Core.CharacterFilter.LoginComplete += _container.Resolve<LoginCompleteController>().Init;
                Core.CharacterFilter.Death += _container.Resolve<DeathController>().Init;
                Core.WorldFilter.CreateObject += _container.Resolve<CreateObjectController>().Init;
                Core.WorldFilter.MoveObject += _container.Resolve<MoveObjectController>().Init;
                Core.WorldFilter.ReleaseObject += _container.Resolve<ReleaseObjectController>().Init;
            } catch (Exception ex) { _logger.Error(ex); }
        }
    }
}
