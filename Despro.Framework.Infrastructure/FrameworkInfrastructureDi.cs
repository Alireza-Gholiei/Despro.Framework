using Despro.Framework.Base.BaseModels.DbModels;
using Despro.Framework.Base.IBaseServices;
using Despro.Framework.Base.IMediator;
using Despro.Framework.Infrastructure.BaseServices;
using Despro.Framework.Infrastructure.BaseServices.DIContainer;
using Despro.Framework.Infrastructure.BaseServices.IDIContainer;
using Despro.Framework.Infrastructure.InfrastructureIServices;
using Despro.Framework.Infrastructure.InfrastructureServices;
using Despro.Framework.Infrastructure.Mediator;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Reflection;

namespace Despro.Framework.Infrastructure;

public static class FrameworkInfrastructureDi
{
    /// <param name="services">IServiceCollection</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Add Infrastructure Dependency Injection
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="useCaseAssembly">Assembly where UseCases is located</param>
        /// <param name="queryAssembly">Assembly where Queries is located</param>
        /// <returns>ServiceCollection</returns>
        public IServiceCollection AddFrameworkInfrastructure(IConfiguration configuration,
            Assembly useCaseAssembly,
            Assembly queryAssembly,
            bool MongoDbLog = false)
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IErrorLogger, ErrorLogger>();
            //services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork<>));
            services.AddScoped<ICustomPublisher, CustomPublisher>();

            if (MongoDbLog)
            {
                var conn = configuration.GetSection("DesproConfig:MongoDbConfig:ConnectionString").Value
                           ?? throw new Exception("MongoDb not configured! Config Is: 'DesproConfig:MongoDbConfig:ConnectionString'");

                services.Configure<MongoDbConfig>(configuration.GetSection("DesproConfig:MongoDbConfig"));

                services.AddSingleton<IMongoClient>(new MongoClient(conn));
                services.AddScoped(sp => sp.GetRequiredService<IMongoClient>()
                    .GetDatabase(sp.GetRequiredService<IOptions<MongoDbConfig>>().Value.DatabaseName));

                services.AddScoped<ILogService, MongoLogService>();
                services.AddScoped<ILoggingContext, LoggingContext>();
            }
            else
            {
                services.AddScoped<ILogService, NullLogService>();
                services.AddScoped<ILoggingContext, NullLoggingContext>();
            }

            //services.AddScoped(typeof(IDapperRepository<>), typeof(DapperRepository<>));

            #region MediatR
            services.AddDesproMediator(typeof(CustomPublisher).Assembly, useCaseAssembly, queryAssembly);

            services.AddValidatorsFromAssembly(typeof(CustomPublisher).Assembly);
            services.AddValidatorsFromAssembly(useCaseAssembly);
            services.AddValidatorsFromAssembly(queryAssembly);
            #endregion

            services.AddScoped(typeof(IBaseRepository<,>), typeof(Repository<,>));

            services.AddScoped<IRepositoryServices, RepositoryServices>();

            return services;
        }

        private IServiceCollection AddDesproMediator(params Assembly[] assemblies)
        {
            services.AddScoped<ISender, Sender>();

            var requestHandlerDef = typeof(IRequestHandler<,>);
            var notificationHandlerDef = typeof(INotificationHandler<>);

            foreach (var type in assemblies.Distinct().SelectMany(a => a.GetTypes()))
            {
                if (type is not { IsClass: true, IsAbstract: false, IsGenericTypeDefinition: false })
                    continue;

                var genericInterfaces = type.GetInterfaces().Where(i => i.IsGenericType).ToList();

                foreach (var i in genericInterfaces.Where(i => i.GetGenericTypeDefinition() == requestHandlerDef))
                    services.AddScoped(i, type);

                var notificationInterfaces = genericInterfaces
                    .Where(i => i.GetGenericTypeDefinition() == notificationHandlerDef)
                    .ToList();

                if (notificationInterfaces.Count == 0)
                    continue;

                services.AddScoped(type);
                foreach (var i in notificationInterfaces)
                    services.AddScoped(i, sp => sp.GetRequiredService(type));
            }

            return services;
        }
    }
}
