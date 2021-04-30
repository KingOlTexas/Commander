using Autofac;

namespace Commander.Lib.Views.Bindings
{
    public class ViewsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<MainView>().SingleInstance();
        }
    }
}
