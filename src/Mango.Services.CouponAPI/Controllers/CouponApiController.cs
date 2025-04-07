using AutoMapper;
using Mango.Services.CouponAPI.Data;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.Dto;
using Mango.Services.Infrastructure.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.CouponAPI.Controllers;

[Route("api/coupon")]
[ApiController]
[Authorize]
public class CouponApiController : ControllerBase
{
	private readonly AppDbContext _db;
	private readonly ResponseDto _responseDto;
	private readonly IMapper _mapper;

	public CouponApiController(AppDbContext db, IMapper mapper)
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
			var coupons = _db.Coupons.ToList();
			_responseDto.Result = _mapper.Map<IEnumerable<CouponDto>>(coupons);
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
			var coupon = _db.Coupons.First(x => x.CouponId == id);
			_responseDto.Result = _mapper.Map<CouponDto>(coupon);
		}
		catch (Exception e)
		{
			_responseDto.IsSuccess = false;
			_responseDto.Message = e.Message;
		}

		return _responseDto;
	}

	[HttpGet]
	[Route("getByCode/{code}")]
	public ResponseDto Get(string code)
	{
		try
		{
			var coupon = _db.Coupons.FirstOrDefault(x => x.CouponCode.ToLower() == code.ToLower());
			if (coupon == null)
			{
				_responseDto.IsSuccess = false;
			}

			_responseDto.Result = _mapper.Map<CouponDto>(coupon);
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
	public ResponseDto Post([FromBody] CouponDto couponDto)
	{
		try
		{
			var coupon = _mapper.Map<Coupon>(couponDto);
			_db.Coupons.Add(coupon);
			_db.SaveChanges();

			_responseDto.Result = _mapper.Map<CouponDto>(coupon);
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
	public ResponseDto Put([FromBody] CouponDto couponDto)
	{
		try
		{
			var coupon = _mapper.Map<Coupon>(couponDto);
			_db.Coupons.Update(coupon);
			_db.SaveChanges();

			_responseDto.Result = _mapper.Map<CouponDto>(coupon);
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
			var coupon = _db.Coupons.First(x => x.CouponId == id);
			_db.Coupons.Remove(coupon);
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
