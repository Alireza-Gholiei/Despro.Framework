using Despro.Framework.Base.BaseExceptions;

namespace Despro.Framework.Infrastructure.InfrastructureExceptions;

public abstract class BaseInfrastructureException(string message) : BaseException(message);