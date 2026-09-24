using ConferenceHallBooking.Domain.Exceptions;
using FluentValidation;
using MediatR;
using ValidationException = ConferenceHallBooking.Domain.Exceptions.ValidationException;

namespace ConferenceHallBooking.Application.Behaviours;

/// <summary>
/// MediatR pipeline behavior that runs FluentValidation validators before command handlers.
/// Collects all validation failures and throws a single ValidationException if any exist.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var validatorList = validators as IValidator<TRequest>[] ?? validators.ToArray();

        if (validatorList.Length == 0)
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            validatorList.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = failures
                .GroupBy(e => e.PropertyName, e => e.ErrorMessage)
                .ToDictionary(g => g.Key, g => g.ToArray());

            throw new ValidationException(errors);
        }

        return await next();
    }
}
