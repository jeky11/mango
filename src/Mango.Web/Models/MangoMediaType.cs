using System.Net.Mime;

namespace Mango.Web.Models;

public record MangoMediaType
{
	public string Value { get; }

	private MangoMediaType(string mediaType)
	{
		Value = mediaType;
	}

	public static readonly MangoMediaType ApplicationJson = new(MediaTypeNames.Application.Json);
	public static readonly MangoMediaType MultipartFormData = new(MediaTypeNames.Multipart.FormData);
}
