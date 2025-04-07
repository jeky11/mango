using AutoMapper;
using Mango.Services.Infrastructure.Models.Dto;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
	[Route("api/cart")]
	[ApiController]
	public class CartApiController : ControllerBase
	{
		private readonly IMapper _mapper;
		private readonly AppDbContext _db;

		public CartApiController(IMapper mapper, AppDbContext db)
		{
			_mapper = mapper;
			_db = db;
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
					_db.CartDetails.Add(cartDetails);
					await _db.SaveChangesAsync();
				}
				else
				{
					var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(
						u =>
							u.ProductId == cartDetailsDto.ProductId &&
							u.CartHeaderId == cartHeaderFromDb.CartHeaderId);
					if (cartDetailsFromDb == null)
					{
						// create cart details
						cartDetailsDto.CartHeaderId = cartHeaderFromDb.CartHeaderId;
						var cartDetails = _mapper.Map<CartDetails>(cartDetailsDto);
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
	}
}
