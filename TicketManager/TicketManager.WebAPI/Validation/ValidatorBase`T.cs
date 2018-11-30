using FluentValidation;

namespace TicketManager.WebAPI.Validation
{
    public class ValidatorBase<T> : AbstractValidator<T>
    {
        protected bool NotBeWhitespaceOnly(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }
    }
}