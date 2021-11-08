using FluentValidation;
using Service.Verification.Api.Controllers.Contracts;

namespace Service.Verification.Api.Validators
{
    public class PhoneValidator  : AbstractValidator<SendPhoneSetupRequest>
    {
        public PhoneValidator()
        {
            RuleFor(x => x.PhoneNumber).Length(8, 20);
            RuleFor(x => x.PhoneNumber).Must(t => t.StartsWith('+')).WithMessage("Phone number should starts with +");
            RuleFor(x => x.PhoneNumber).Matches("^[+]{0,1}[0-9]");
        }
    }
}