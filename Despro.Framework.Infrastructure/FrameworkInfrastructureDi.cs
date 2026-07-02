using Despro.Framework.Base.BaseModels.DbModels;
using Despro.Framework.Base.IBaseServices;
using Despro.Framework.Infrastructure.BaseServices;
using Despro.Framework.Infrastructure.BaseServices.DIContainer;
using Despro.Framework.Infrastructure.BaseServices.IDIContainer;
using Despro.Framework.Infrastructure.InfrastructureIServices;
using Despro.Framework.Infrastructure.InfrastructureServices;
using Despro.Framework.Infrastructure.MediatR;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using System.Reflection;

namespace Despro.Framework.Infrastructure;

public static class FrameworkInfrastructureDi
{
    /// <summary>
    /// Add Infrastructure Dependency Injection
    /// </summary>
    /// <param name="services">IServiceCollection</param>
    /// <param name="configuration"></param>
    /// <param name="useCaseAssembly">Assembly where UseCases is located</param>
    /// <param name="queryAssembly">Assembly where Queries is located</param>
    /// <returns>ServiceCollection</returns>
    public static IServiceCollection AddFrameworkInfrastructure(this IServiceCollection services,
        IConfiguration configuration,
        Assembly useCaseAssembly,
        Assembly queryAssembly,
        bool MongoDbLog = false)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IErrorLogger, ErrorLogger>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ICustomPublisher, CustomPublisher>();

        if (MongoDbLog)
        {
            services.Configure<MongoDbConfig>(configuration.GetSection("MongoDbConfig"));

            var conn = configuration.GetSection("MongoDbConfig:ConnectionString").Value
                       ?? throw new Exception("MongoDb ConnectionString not configured!");

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
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(CustomPublisher).Assembly);
            cfg.RegisterServicesFromAssembly(useCaseAssembly);
            cfg.RegisterServicesFromAssembly(queryAssembly);
        });

        services.AddValidatorsFromAssembly(typeof(CustomPublisher).Assembly);
        services.AddValidatorsFromAssembly(useCaseAssembly);
        services.AddValidatorsFromAssembly(queryAssembly);
        #endregion

        services.AddScoped(typeof(IBaseRepository<>), typeof(Repository<>));

        services.AddScoped<IRepositoryServices, RepositoryServices>();

        return services;
    }
}
