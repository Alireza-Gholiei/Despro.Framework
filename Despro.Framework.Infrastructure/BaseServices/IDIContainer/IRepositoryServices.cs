using Despro.Framework.Base.IBaseServices;
using Despro.Framework.Infrastructure.InfrastructureIServices;

namespace Despro.Framework.Infrastructure.BaseServices.IDIContainer;

public interface IRepositoryServices
{
    IAuthService AuthService { get; }
    ILoggingContext LoggingContext { get; }
}