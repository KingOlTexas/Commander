using Autofac;

namespace Commander.Lib.Services.Bindings
{
    public class ServicesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<PlayerManagerImpl>().As<PlayerManager>().SingleInstance();
            builder.RegisterType<LoginSessionManagerImpl>().As<LoginSessionManager>().SingleInstance();
            builder.RegisterType<SettingsManagerImpl>().As<SettingsManager>().SingleInstance();
            builder.RegisterType<DebuggerImpl>().As<Debugger>().SingleInstance();
            builder.RegisterType<RelogManagerImpl>().As<RelogManager>().SingleInstance();
            builder.RegisterType<DebuffManagerImpl>().As<DebuffManager>().SingleInstance();
            builder.RegisterType<GlobalProvider>().SingleInstance();
            builder.RegisterType<LoggerImpl>().As<Logger>();
        }
    }
}
