using System.Linq;
using FluentValidation;
using Service.Verification.Api.Controllers.Contracts;

namespace Service.Verification.Api.Validators
{
    public class PhoneRequestValidator  : AbstractValidator<SendPhoneSetupRequest>
    {
        public PhoneRequestValidator()
        {
            RuleFor(x => x.PhoneNumber).Length(8, 20);
            RuleFor(x => x.PhoneNumber).Must(t => t.StartsWith('+')).WithMessage("Phone number should starts with +").Must(t => !t.Any(char.IsWhiteSpace));
            RuleFor(x => x.PhoneNumber).Matches("^[+]{0,1}[0-9]");
        }
    }
    
    public class PhoneVerifyValidator  : AbstractValidator<VerifyPhoneSetupRequest>
    {
        public PhoneVerifyValidator()
        {
            RuleFor(x => x.PhoneNumber).Length(8, 20);
            RuleFor(x => x.PhoneNumber).Must(t => t.StartsWith('+')).WithMessage("Phone number should starts with +").Must(t => !t.Any(char.IsWhiteSpace));
            RuleFor(x => x.PhoneNumber).Matches("^[+]{0,1}[0-9]");
        }
    }
}