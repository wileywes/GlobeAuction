﻿using GlobeAuction.Models;

namespace GlobeAuction
{
    public static class AutoMapperConfig
    {
        public static void RegisterMappings()
        {
            AutoMapper.Mapper.Initialize(c =>
            {
                c.CreateMap<Bidder, BidderViewModel>();
                c.CreateMap<BidderViewModel, Bidder>();

                c.CreateMap<AuctionGuest, AuctionGuestViewModel>();
                c.CreateMap<AuctionGuestViewModel, AuctionGuest>();

                c.CreateMap<Student, StudentViewModel>();
                c.CreateMap<StudentViewModel, Student>();

                c.CreateMap<StoreItemPurchase, StoreItemPurchaseViewModel>()
                    .ForMember(dest => dest.StoreItemId,
                               opts => opts.MapFrom(src => src.StoreItem.StoreItemId));
                c.CreateMap<StoreItemPurchaseViewModel, StoreItemPurchase>();

                c.CreateMap<StoreItem, StoreItemViewModel>();
                c.CreateMap<StoreItemViewModel, StoreItem>();
            });
        }
    }
}