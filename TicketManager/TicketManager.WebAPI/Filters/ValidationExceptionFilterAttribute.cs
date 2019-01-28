using System.Linq;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace TicketManager.WebAPI.Filters
{
    public class ValidationExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(ExceptionContext context)
        {
            if (context.Exception is ValidationException fluentValidationException)
            {
                var validationErrorsDictionary = fluentValidationException
                    .Errors
                    .GroupBy(
                        err => err.PropertyName,
                        (propertyName, propertyValidationErrors) => new
                        {
                            PropertyName = propertyName,
                            Errors = propertyValidationErrors.Select(err => err.ErrorMessage).ToArray()
                        })
                    .ToDictionary(grp => grp.PropertyName, grp => grp.Errors);

                var result = new ValidationProblemDetails(validationErrorsDictionary)
                {
                    Title = "One or more validation errors occurred."
                };

                context.Result = new BadRequestObjectResult(result);
                context.ExceptionHandled = true;
            }
        }
    }
}