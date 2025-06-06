using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Utility;

public class MaxFileSizeAttribute(int maxFileSizeInMb) : ValidationAttribute
{
	private readonly int _maxFileSizeInMb = maxFileSizeInMb;

	protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
	{
		if (value is IFormFile file)
		{
			if (file.Length > _maxFileSizeInMb * 1024 * 1024)
			{
				return new ValidationResult($"Max allowed file size is {_maxFileSizeInMb} MB.");
			}
		}

		return ValidationResult.Success;
	}
}
