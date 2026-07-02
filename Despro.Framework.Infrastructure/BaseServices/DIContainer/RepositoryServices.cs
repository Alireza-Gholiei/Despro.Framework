using Despro.Framework.Base.IBaseServices;
using Despro.Framework.Infrastructure.BaseServices.IDIContainer;
using Despro.Framework.Infrastructure.InfrastructureIServices;

namespace Despro.Framework.Infrastructure.BaseServices.DIContainer;

public class RepositoryServices(
    IAuthService authService,
    ILoggingContext loggingContext,
    IServiceProvider serviceProvider) : IRepositoryServices
{
    public IAuthService AuthService { get; } = authService;
    public ILoggingContext LoggingContext { get; } = loggingContext;
    public IServiceProvider ServiceProvider { get; } = serviceProvider;
}