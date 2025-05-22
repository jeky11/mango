using AutoMapper;
using Mango.Services.Infrastructure.Models.Dto;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.OrderAPI.Controllers;

[Route("api/order")]
[ApiController]
public class OrderApiController(
	IMapper mapper,
	AppDbContext db,
	IProductService productService) : ControllerBase
{
	private readonly IMapper _mapper = mapper;
	private readonly AppDbContext _db = db;
	private readonly IProductService _productService = productService;

	[HttpPost("create")]
	public async Task<ResponseDto> Create([FromBody] CartDto cartDto)
	{
		try
		{
			if (cartDto.CartHeader == null || cartDto.CartDetails == null || !cartDto.CartDetails.Any())
			{
				return new ResponseDto
				{
					Message = "Invalid cart",
					IsSuccess = false,
				};
			}

			var orderHeaderDto = _mapper.Map<OrderHeaderDto>(cartDto.CartHeader);
			orderHeaderDto.OrderTime = DateTime.UtcNow;
			orderHeaderDto.Status = Status.Pending;
			orderHeaderDto.OrderDetails = _mapper.Map<List<OrderDetailsDto>>(cartDto.CartDetails);

			var orderCreated = await _db.OrderHeaders.AddAsync(_mapper.Map<OrderHeader>(orderHeaderDto));
			await _db.SaveChangesAsync();

			orderHeaderDto.OrderHeaderId = orderCreated.Entity.OrderHeaderId;

			return new ResponseDto {Result = orderHeaderDto};
		}
		catch (Exception e)
		{
			return new ResponseDto
			{
				Message = e.Message,
				IsSuccess = false,
			};
		}
	}
}
