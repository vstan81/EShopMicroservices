using BuildingBlocks.CQRS;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using System.Net.Http;



namespace BuildingBlocks.Behaviours
{    //pipeline behaviour with MediatR and FluentValidations
    public class ValidationBehaviours<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse> where TRequest: ICommand<TResponse>
    {
        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var context = new ValidationContext<TRequest>(request);

            var validationResults = await Task.WhenAll(validators.Select(x => x.ValidateAsync(context, CancellationToken.None)));

            var failures = validationResults.Where(r => r.Errors.Any()).SelectMany(r => r.Errors).ToList();
            if (failures.Count != 0)
                throw new ValidationException(failures);

            return await next();
        }
    }



    //custom pipeline behaviour with  FluentValidations -
    public class ValidationCustomMiddleware
    {
        private readonly RequestDelegate _next;

        public ValidationCustomMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IEnumerable<IValidator<ICommand>> validators)
        {
            var selectJustCommands = validators.Where(x => x is IValidator<ICommand>).ToList();

            var validationResults = await Task.WhenAll(selectJustCommands.Select(x => x.ValidateAsync((IValidationContext)context, CancellationToken.None)));

            var failures = validationResults.Where(r => r.Errors.Any()).SelectMany(r => r.Errors).ToList();

            if (failures.Count != 0)
                throw new ValidationException(failures);

            // Call the next delegate/middleware in the pipeline.
            await _next(context);
        }


    }

}
