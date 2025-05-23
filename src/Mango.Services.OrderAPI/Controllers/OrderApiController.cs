using AutoMapper;
using Mango.Services.Infrastructure.Models.Dto;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;

namespace Mango.Services.OrderAPI.Controllers;

[Route("api/order")]
[ApiController]
[Authorize]
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

	[HttpPost("validateStripeSession")]
	public async Task<ResponseDto> ValidateStripeSession([FromBody] int orderHeaderId)
	{
		try
		{
			var orderHeader = await _db.OrderHeaders.FirstAsync(x => x.OrderHeaderId == orderHeaderId);

			var sessionService = new SessionService();
			var session = await sessionService.GetAsync(orderHeader.StripeSessionId);

			var paymentIntentService = new PaymentIntentService();
			var paymentIntent = await paymentIntentService.GetAsync(session.PaymentIntentId);

			if (paymentIntent.Status != "succeeded")
			{
				return new ResponseDto();
			}

			orderHeader.PaymentIntentId = paymentIntent.Id;
			orderHeader.Status = Status.Approved;
			await _db.SaveChangesAsync();

			var result = _mapper.Map<OrderHeaderDto>(orderHeader);
			return new ResponseDto {Result = result};
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

	[HttpPost("createStripeSession")]
	public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
	{
		try
		{
			if (stripeRequestDto.OrderHeader?.OrderDetails == null)
			{
				return new ResponseDto
				{
					Message = "Invalid cart",
					IsSuccess = false,
				};
			}

			var sessionCreateOptions = new SessionCreateOptions
			{
				SuccessUrl = stripeRequestDto.ApprovedUrl,
				CancelUrl = stripeRequestDto.CancelUrl,
				LineItems = [],
				Mode = "payment",
			};

			foreach (var item in stripeRequestDto.OrderHeader.OrderDetails)
			{
				var sessionLineItem = new SessionLineItemOptions
				{
					PriceData = new SessionLineItemPriceDataOptions
					{
						UnitAmount = (long)(item.Price * 100), //20.99 -> 2099
						Currency = "usd",
						ProductData = new SessionLineItemPriceDataProductDataOptions {Name = item.Product!.Name}
					},
					Quantity = item.Count
				};

				sessionCreateOptions.LineItems.Add(sessionLineItem);
			}

			if (stripeRequestDto.OrderHeader.Discount > 0)
			{
				var discountObj = new List<SessionDiscountOptions> {new() {Coupon = stripeRequestDto.OrderHeader.CouponCode}};
				sessionCreateOptions.Discounts = discountObj;
			}

			var service = new SessionService();
			var session = await service.CreateAsync(sessionCreateOptions);
			stripeRequestDto.StripeSessionUrl = session.Url;

			var orderHeader = await _db.OrderHeaders.FirstAsync(x => x.OrderHeaderId == stripeRequestDto.OrderHeader.OrderHeaderId);
			orderHeader.StripeSessionId = session.Id;
			await _db.SaveChangesAsync();

			return new ResponseDto {Result = stripeRequestDto};
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
