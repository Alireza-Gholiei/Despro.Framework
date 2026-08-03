using Despro.Framework.Application.ApplicationExceptions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
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

        //var validationResults = await Task.WhenAll(
        //    validators.Select(v => v.ValidateAsync(context, cancellationToken))
        //);
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

//public class CommandValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
//    : IPipelineBehavior<TRequest, TResponse>
//    where TRequest : notnull
//{
//    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
//    {
//        IEnumerable<ValidationFailure>? errors = validators
//            .Select(v => v.Validate(request))
//            .SelectMany(result => result.Errors)
//            .Where(r => r != null);

//        if (errors is not null && errors.Any())
//        {
//            StringBuilder stringBuilder = new();

//            foreach (var error in errors)
//            {
//                stringBuilder.AppendLine(error.ErrorMessage);
//            }

//            throw new InvalidCommandException(stringBuilder.ToString());
//        }

//        var response = await next(cancellationToken);

//        return response;
//    }
//}