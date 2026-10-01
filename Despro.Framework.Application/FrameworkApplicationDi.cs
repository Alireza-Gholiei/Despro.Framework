using Despro.Framework.Application.QueryCommandTools;
using Despro.Framework.Base.IMediator;
using Despro.Framework.Base.Validator;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Despro.Framework.Application;

public static class FrameworkApplicationDi
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFrameworkApplication()
        {
            services.AddPipelineBehavior(typeof(CommandValidationBehavior<,>));

            services.AddValidatorsFromAssembly(typeof(CommandValidationBehavior<,>).Assembly);
            services.AddValidatorsFromAssembly(typeof(FileValidator).Assembly);

            return services;
        }

        private IServiceCollection AddPipelineBehavior(Type behaviorType)
            => services.AddScoped(typeof(IPipelineBehavior<,>), behaviorType);
    }
}