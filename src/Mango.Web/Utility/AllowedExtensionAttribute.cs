using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Utility;

public class AllowedExtensionAttribute(params string[] extensions) : ValidationAttribute
{
	private readonly string[] _extensions = extensions;

	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		if (value is IFormFile file)
		{
			var extension = Path.GetExtension(file.FileName);
			if (!_extensions.Contains(extension.ToLower()))
			{
				return new ValidationResult("This photo extension is not allowed.");
			}
		}

		return ValidationResult.Success;
	}
}
