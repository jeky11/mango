using AutoMapper;
using Mango.MessageBus;
using Mango.Services.Infrastructure.Models.Dto;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Mango.Services.ShoppingCartAPI.Controllers;

[Route("api/cart")]
[ApiController]
public class CartApiController : ControllerBase
{
	private readonly IMapper _mapper;
	private readonly AppDbContext _db;
	private readonly IProductService _productService;
	private readonly ICouponService _couponService;
	private readonly IMessageBus _messageBus;
	private readonly TopicAndQueueNames _topicAndQueueNames;

	public CartApiController(
		IMapper mapper,
		AppDbContext db,
		IProductService productService,
		ICouponService couponService,
		IMessageBus messageBus,
		IOptions<TopicAndQueueNames> topicAndQueueNames)
	{
		_mapper = mapper;
		_db = db;
		_productService = productService;
		_couponService = couponService;
		_messageBus = messageBus;
		_topicAndQueueNames = topicAndQueueNames.Value;
	}

	[HttpGet("get/{userId}")]
	public async Task<ResponseDto> GetCart(string userId)
	{
		try
		{
			var cartHeader = _mapper.Map<CartHeaderDto>(await _db.CartHeaders.FirstAsync(u => u.UserId == userId));
			var cartDetails = _mapper.Map<IEnumerable<CartDetailsDto>>(_db.CartDetails.Where(u => u.CartHeaderId == cartHeader.CartHeaderId))
				.ToList();

			var productDtos = (await _productService.GetProducts()).ToList();

			foreach (var item in cartDetails)
			{
				item.Product = productDtos.First(x => x.ProductId == item.ProductId);
				cartHeader.CartTotal += item.Count * (decimal)item.Product.Price;
			}

			if (!string.IsNullOrEmpty(cartHeader.CouponCode))
			{
				var coupon = await _couponService.GetCoupon(cartHeader.CouponCode);
				if (coupon != null && cartHeader.CartTotal > coupon.MinAmount)
				{
					cartHeader.CartTotal -= coupon.DiscountAmount;
					cartHeader.Discount = coupon.DiscountAmount;
				}
			}

			var cart = new CartDto
			{
				CartHeader = cartHeader,
				CartDetails = cartDetails
			};

			return new ResponseDto {Result = cart};
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

	[HttpPost("applyCoupon")]
	public async Task<ResponseDto> ApplyCoupon([FromBody] CartDto cartDto)
	{
		try
		{
			if (cartDto.CartHeader == null)
			{
				return new ResponseDto
				{
					Message = "Invalid cart",
					IsSuccess = false,
				};
			}

			var cartFromDb = await _db.CartHeaders.FirstAsync(x => x.UserId == cartDto.CartHeader.UserId);
			cartFromDb.CouponCode = cartDto.CartHeader.CouponCode;
			_db.CartHeaders.Update(cartFromDb);
			await _db.SaveChangesAsync();

			return new ResponseDto {Result = true};
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

	[HttpPost("upsert")]
	public async Task<ResponseDto> Upsert(CartDto cartDto)
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

			var cartDetailsDto = cartDto.CartDetails.First();

			var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking().FirstOrDefaultAsync(u => u.UserId == cartDto.CartHeader.UserId);
			if (cartHeaderFromDb == null)
			{
				// create header and details
				var cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
				_db.CartHeaders.Add(cartHeader);
				await _db.SaveChangesAsync();

				cartDetailsDto.CartHeaderId = cartHeader.CartHeaderId;
				var cartDetails = _mapper.Map<CartDetails>(cartDetailsDto);
				cartDetails.CartHeader = null;
				_db.CartDetails.Add(cartDetails);
				await _db.SaveChangesAsync();
			}
			else
			{
				var cartDetailsFromDb = await _db.CartDetails.AsNoTracking()
					.FirstOrDefaultAsync(u =>
						u.ProductId == cartDetailsDto.ProductId &&
						u.CartHeaderId == cartHeaderFromDb.CartHeaderId);
				if (cartDetailsFromDb == null)
				{
					// create cart details
					cartDetailsDto.CartHeaderId = cartHeaderFromDb.CartHeaderId;
					var cartDetails = _mapper.Map<CartDetails>(cartDetailsDto);
					cartDetails.CartHeader = null;
					_db.CartDetails.Add(cartDetails);
					await _db.SaveChangesAsync();
				}
				else
				{
					// update count in cart details
					cartDetailsDto.Count += cartDetailsFromDb.Count;
					cartDetailsDto.CartHeaderId = cartDetailsFromDb.CartHeaderId;
					cartDetailsDto.CartDetailsId = cartDetailsFromDb.CartDetailsId;
					var cartDetails = _mapper.Map<CartDetails>(cartDetailsDto);
					cartDetails.CartHeader = null;
					_db.CartDetails.Update(cartDetails);
					await _db.SaveChangesAsync();
				}
			}

			return new ResponseDto {Result = cartDto};
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

	[HttpPost("remove")]
	public async Task<ResponseDto> Remove([FromBody] int cartDetailsId)
	{
		try
		{
			var cartDetails = await _db.CartDetails.FirstAsync(u => u.CartDetailsId == cartDetailsId);
			_db.CartDetails.Remove(cartDetails);

			var totalCountOfCartItem = _db.CartDetails.Count(u => u.CartHeaderId == cartDetails.CartHeaderId);
			if (totalCountOfCartItem == 1)
			{
				var cartHeaderToRemove = await _db.CartHeaders.FirstAsync(u => u.CartHeaderId == cartDetails.CartHeaderId);
				_db.CartHeaders.Remove(cartHeaderToRemove);
			}

			await _db.SaveChangesAsync();

			return new ResponseDto {Result = true};
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

	[HttpPost("emailCartRequest")]
	public async Task<ResponseDto> EmailCartRequest([FromBody] CartDto cartDto)
	{
		try
		{
			await _messageBus.PublishMessageAsync(cartDto, _topicAndQueueNames.EmailShoppingCartQueue);

			return new ResponseDto {Result = true};
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
