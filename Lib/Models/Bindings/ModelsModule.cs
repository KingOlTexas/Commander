using Autofac;
using Commander.Models;

namespace Commander.Lib.Models.Bindings
{
    public class ModelsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<LoginSession>();
            builder.RegisterType<Player>();
            builder.RegisterType<Settings>();
            builder.RegisterType<DebuffInformation>();
            builder.RegisterType<DebuffObj>();
            builder.RegisterType<LowHealthObj>();
        }
    }
}
