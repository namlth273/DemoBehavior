using Autofac;
using MediatR;
using MediatR.Extensions.Autofac.DependencyInjection;

namespace DemoBehavior
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.AddMediatR(ThisAssembly);
            builder.RegisterGeneric(typeof(ExceptionHandlingBehavior<,>)).As(typeof(IPipelineBehavior<,>));
        }
    }
}