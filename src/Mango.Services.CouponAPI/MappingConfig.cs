using AutoMapper;
using Mango.Services.CouponAPI.Models;
using Mango.Services.CouponAPI.Models.Dto;

namespace Mango.Services.CouponAPI;

public class MappingConfig
{
	public static MapperConfiguration RegisterMaps()
	{
		var mappingConfig = new MapperConfiguration(
			config =>
			{
				config.CreateMap<Coupon, CouponDto>().ReverseMap();
				config.CreateMap<CouponDto, Coupon>().ReverseMap();
			});

		return mappingConfig;
	}
}
