using Despro.Framework.Application.ApplicationExceptions;
using Despro.Framework.Base.IMediator;
using FluentValidation;
using FluentValidation.Results;
using System.Text;

namespace Despro.Framework.Application.QueryCommandTools;

public class CommandValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var validationResults = new List<ValidationResult>();
        foreach (var validator in validators)
        {
            var result = await validator.ValidateAsync(context, cancellationToken);
            validationResults.Add(result);
        }

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (!failures.Any())
            return await next(cancellationToken);

        var sb = new StringBuilder();
        foreach (var error in failures)
            sb.AppendLine(error.ErrorMessage);

        throw new InvalidCommandException(sb.ToString());
    }
}