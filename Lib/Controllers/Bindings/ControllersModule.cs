using Autofac;

namespace Commander.Lib.Controllers.Bindings
{
    public class ControllersModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LoginControllerImpl>().As<LoginController>();
            builder.RegisterType<LoginCompleteControllerImpl>().As<LoginCompleteController>();
            builder.RegisterType<PluginTermCompleteControllerImpl>().As<PluginTermCompleteController>();
            builder.RegisterType<MoveObjectControllerImpl>().As<MoveObjectController>();
            builder.RegisterType<CreateObjectControllerImpl>().As<CreateObjectController>();
            builder.RegisterType<ReleaseObjectControllerImpl>().As<ReleaseObjectController>();
            builder.RegisterType<ServerDispatchControllerImpl>().As<ServerDispatchController>();
            builder.RegisterType<DeathControllerImpl>().As<DeathController>();
        }
    }
}
