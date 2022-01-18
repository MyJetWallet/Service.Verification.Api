using System.Linq;
using FluentValidation;
using Service.Verification.Api.Controllers.Contracts;

namespace Service.Verification.Api.Validators
{
    public class PhoneRequestValidator  : AbstractValidator<SendPhoneSetupRequest>
    {
        public PhoneRequestValidator()
        {
            RuleFor(x => x.PhoneNumber).Matches("^[+]{0,1}[0-9]*$");
            RuleFor(x => x.PhoneNumber).Length(0, 20);
            //RuleFor(x => x.PhoneNumber).Must(t => t.StartsWith('+')).WithMessage("Phone number should starts with +");
            //RuleFor(x => x.PhoneNumber).Must(t => !t.Any(char.IsWhiteSpace));
            
            RuleFor(x => x.PhoneCode).Matches("^[+]{0,1}[0-9]*$");
            RuleFor(x => x.PhoneCode).Length(0, 5);
            //RuleFor(x => x.PhoneCode).Must(t => t.StartsWith('+')).WithMessage("Phone number should starts with +");
            //RuleFor(x => x.PhoneCode).Must(t => !t.Any(char.IsWhiteSpace));
            
            RuleFor(x => x.PhoneBody).Matches("^[0-9]*$");
            RuleFor(x => x.PhoneBody).Length(0, 20); //TODO: return to normal values when FE moved to code+body
            //RuleFor(x => x.PhoneBody).Must(t => !t.Any(char.IsWhiteSpace));
        }
    }
    
    public class PhoneVerifyValidator  : AbstractValidator<VerifyPhoneSetupRequest>
    {
        public PhoneVerifyValidator()
        {
            RuleFor(x => x.PhoneNumber).Matches("^[+]{0,1}[0-9]*$");
            RuleFor(x => x.PhoneNumber).Length(0, 20);
            //RuleFor(x => x.PhoneNumber).Must(t => t.StartsWith('+')).WithMessage("Phone number should starts with +");
            //RuleFor(x => x.PhoneNumber).Must(t => !t.Any(char.IsWhiteSpace));
            
            RuleFor(x => x.PhoneCode).Matches("^[+]{0,1}[0-9]*$");
            RuleFor(x => x.PhoneCode).Length(0, 5);
            //RuleFor(x => x.PhoneCode).Must(t => t.StartsWith('+')).WithMessage("Phone number should starts with +");
            //RuleFor(x => x.PhoneCode).Must(t => !t.Any(char.IsWhiteSpace));
            
            RuleFor(x => x.PhoneBody).Matches("^[0-9]*$");
            RuleFor(x => x.PhoneBody).Length(0, 20); //TODO: return to normal values when FE moved to code+body
            //RuleFor(x => x.PhoneBody).Must(t => !t.Any(char.IsWhiteSpace));
        }
    }
}