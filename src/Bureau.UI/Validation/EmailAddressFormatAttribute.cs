using System.ComponentModel.DataAnnotations;

namespace Bureau.UI.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
        AllowMultiple = false)]
    public sealed class EmailAddressFormatAttribute : ValidationAttribute
    {
        private EmailAddressAttribute _emailAddressAttribute;
        public EmailAddressFormatAttribute() 
        {
            _emailAddressAttribute = new EmailAddressAttribute();
        }
        public override bool IsValid(object? value)
        {
            if (string.IsNullOrWhiteSpace(value as string))
            {
                return true;
            }
            else 
            {
                return _emailAddressAttribute.IsValid(value);
            }
        }

        public override string FormatErrorMessage(string name)
        {
            return _emailAddressAttribute.FormatErrorMessage(name);
        }
    }
}
