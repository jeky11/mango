using AutoMapper;
using Mango.Services.ProductAPI.Data;
using Mango.Services.ProductAPI.Models;
using Mango.Services.ProductAPI.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ProductAPI.Controllers;

[Route("api/product")]
[ApiController]
[Authorize]
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
	public ResponseDto Post([FromBody] ProductDto productDto)
	{
		try
		{
			var product = _mapper.Map<Product>(productDto);
			_db.Products.Add(product);
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

	[HttpPut]
	[Authorize(Roles = nameof(Role.ADMIN))]
	public ResponseDto Put([FromBody] ProductDto productDto)
	{
		try
		{
			var product = _mapper.Map<Product>(productDto);
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
}
