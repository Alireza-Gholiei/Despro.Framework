using Despro.Framework.Base.BaseExceptions;

namespace Despro.Framework.Domain.DomainExceptions;

public abstract class BaseDomainException(string message) : BaseException(message);
