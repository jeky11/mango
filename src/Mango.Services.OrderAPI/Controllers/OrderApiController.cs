using AutoMapper;
using Mango.MessageBus;
using Mango.Services.Infrastructure.Models.Dto;
using Mango.Services.OrderAPI.Data;
using Mango.Services.OrderAPI.Models;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;

namespace Mango.Services.OrderAPI.Controllers;

[Route("api/order")]
[ApiController]
[Authorize]
public class OrderApiController(
	IMapper mapper,
	AppDbContext db,
	IProductService productService,
	IMessageBus messageBus,
	IOptions<TopicAndQueueNames> topicAndQueueNames) : ControllerBase
{
	private readonly IMapper _mapper = mapper;
	private readonly AppDbContext _db = db;
	private readonly IProductService _productService = productService;
	private readonly IMessageBus _messageBus = messageBus;
	private readonly TopicAndQueueNames _topicAndQueueNames = topicAndQueueNames.Value;
	
	[HttpGet("getOrders")]
	public async Task<ResponseDto> GetOrders(string? userId = "")
	{
		try
		{
			var ordersQuery = _db.OrderHeaders.Include(x => x.OrderDetails).AsQueryable();

			if (!User.IsInRole(nameof(Role.ADMIN)))
			{
				ordersQuery = ordersQuery.Where(x => x.UserId == userId);
			}

			ordersQuery = ordersQuery.OrderByDescending(x => x.OrderHeaderId);
			var orders = await ordersQuery.ToListAsync();

			return new ResponseDto {Result = _mapper.Map<List<OrderHeaderDto>>(orders)};
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

	[HttpGet("getOrder/{id:int}")]
	public async Task<ResponseDto> GetOrder(int id)
	{
		try
		{
			var orderHeader = await _db.OrderHeaders.Include(x => x.OrderDetails).FirstAsync(x => x.OrderHeaderId == id);
			return new ResponseDto {Result = _mapper.Map<OrderHeaderDto>(orderHeader)};
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

			var rewardsDto = new RewardsDto
			{
				OrderId = orderHeader.OrderHeaderId,
				RewardsActivity = Convert.ToInt32(orderHeader.OrderTotal),
				UserId = orderHeader.UserId,
			};

			await _messageBus.PublishMessageAsync(rewardsDto, _topicAndQueueNames.OrderCreatedTopic);

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

	[HttpPost("updateOrderStatus/{orderId:int}")]
	public async Task<ResponseDto> UpdateOrderStatus(int orderId, [FromBody] Status newStatus)
	{
		try
		{
			var orderHeader = await _db.OrderHeaders.FirstOrDefaultAsync(x => x.OrderHeaderId == orderId);
			if (orderHeader == null)
			{
				return new ResponseDto
				{
					Message = "Order not found",
					IsSuccess = false,
				};
			}

			if (newStatus == Status.Cancelled)
			{
				var options = new RefundCreateOptions
				{
					Reason = RefundReasons.RequestedByCustomer,
					PaymentIntent = orderHeader.PaymentIntentId
				};

				var refundService = new RefundService();
				await refundService.CreateAsync(options);
			}

			orderHeader.Status = newStatus;
			await _db.SaveChangesAsync();

			return new ResponseDto();
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
