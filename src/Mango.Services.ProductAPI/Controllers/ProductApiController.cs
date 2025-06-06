using AutoMapper;
using Mango.Services.Infrastructure.Models.Dto;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ProductAPI.Controllers;

[Route("api/product")]
[ApiController]
public class ProductApiController : ControllerBase
{
	private readonly AppDbContext _db;
	private readonly ResponseDto _responseDto;
	private readonly IMapper _mapper;

	public ProductApiController(AppDbContext db, IMapper mapper)
	{
		_db = db;
		_mapper = mapper;
		_responseDto = new ResponseDto();
	}

	[HttpGet]
	public ResponseDto Get()
	{
		try
		{
			var products = _db.Products.ToList();
			_responseDto.Result = _mapper.Map<IEnumerable<ProductDto>>(products);
		}
		catch (Exception e)
		{
			_responseDto.IsSuccess = false;
			_responseDto.Message = e.Message;
		}

		return _responseDto;
	}

	[HttpGet]
	[Route("{id:int}")]
	public ResponseDto Get(int id)
	{
		try
		{
			var product = _db.Products.First(x => x.ProductId == id);
			_responseDto.Result = _mapper.Map<ProductDto>(product);
		}
		catch (Exception e)
		{
			_responseDto.IsSuccess = false;
			_responseDto.Message = e.Message;
		}

		return _responseDto;
	}

	[HttpPost]
	[Authorize(Roles = nameof(Role.ADMIN))]
	public ResponseDto Post(ProductDto productDto)
	{
		try
		{
			var product = _mapper.Map<Product>(productDto);
			product.ImageUrl = "https://placehold.co/600x400";
			_db.Products.Add(product);
			_db.SaveChanges();

			if (productDto.Image != null)
			{
				var (imageUrl, filePath) = SaveImage(product.ProductId, productDto.Image);

				product.ImageUrl = imageUrl;
				product.ImageLocalPath = filePath;
				_db.Products.Update(product);
				_db.SaveChanges();
			}

			_responseDto.Result = _mapper.Map<ProductDto>(product);
		}
		catch (Exception e)
		{
			_responseDto.IsSuccess = false;
			_responseDto.Message = e.Message;
		}

		return _responseDto;
	}

	[HttpPut]
	[Authorize(Roles = nameof(Role.ADMIN))]
	public ResponseDto Put(ProductDto productDto)
	{
		try
		{
			var product = _mapper.Map<Product>(productDto);

			if (productDto.Image != null)
			{
				DeleteImage(product);
				var (imageUrl, filePath) = SaveImage(productDto.ProductId, productDto.Image);

				product.ImageUrl = imageUrl;
				product.ImageLocalPath = filePath;
			}

			_db.Products.Update(product);
			_db.SaveChanges();

			_responseDto.Result = _mapper.Map<ProductDto>(product);
		}
		catch (Exception e)
		{
			_responseDto.IsSuccess = false;
			_responseDto.Message = e.Message;
		}

		return _responseDto;
	}

	[HttpDelete]
	[Route("{id:int}")]
	[Authorize(Roles = nameof(Role.ADMIN))]
	public ResponseDto Delete(int id)
	{
		try
		{
			var product = _db.Products.First(x => x.ProductId == id);

			DeleteImage(product);

			_db.Products.Remove(product);
			_db.SaveChanges();
		}
		catch (Exception e)
		{
			_responseDto.IsSuccess = false;
			_responseDto.Message = e.Message;
		}

		return _responseDto;
	}

	private (string imageUrl, string filePath) SaveImage(int productId, IFormFile image)
	{
		var fileName = productId + Path.GetExtension(image.FileName);
		var filePath = @"wwwroot\ProductImages\" + fileName;
		var filePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), filePath);
		using (var fileStream = new FileStream(filePathDirectory, FileMode.Create))
		{
			image.CopyTo(fileStream);
		}

		var baseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host.Value}{HttpContext.Request.PathBase.Value}";
		var imageUrl = baseUrl + "/ProductImages/" + fileName;
		return (imageUrl, filePath);
	}

	private static void DeleteImage(Product product)
	{
		if (string.IsNullOrEmpty(product.ImageLocalPath))
		{
			return;
		}

		var oldFilePathDirectory = Path.Combine(Directory.GetCurrentDirectory(), product.ImageLocalPath);
		var file = new FileInfo(oldFilePathDirectory);
		if (file.Exists)
		{
			file.Delete();
		}
	}
}
