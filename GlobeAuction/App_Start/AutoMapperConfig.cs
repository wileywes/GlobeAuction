using GlobeAuction.Models;

namespace GlobeAuction
{
    public static class AutoMapperConfig
    {
        public static void RegisterMappings()
        {
            AutoMapper.Mapper.Initialize(c =>
            {
                c.CreateMap<AuctionItem, AuctionItemViewModel>();
                c.CreateMap<AuctionItemViewModel, AuctionItem>();

                c.CreateMap<Bidder, BidderViewModel>();
                c.CreateMap<BidderViewModel, Bidder>();
                c.CreateMap<BidderRegistrationViewModel, Bidder>();

                c.CreateMap<AuctionGuest, AuctionGuestViewModel>();
                c.CreateMap<AuctionGuestViewModel, AuctionGuest>();

                c.CreateMap<Student, StudentViewModel>();
                c.CreateMap<StudentViewModel, Student>();

                c.CreateMap<StoreItemPurchase, StoreItemPurchaseViewModel>();
                c.CreateMap<StoreItemPurchaseViewModel, StoreItemPurchase>();

                c.CreateMap<StoreItem, StoreItemViewModel>();
                c.CreateMap<StoreItem, StoreItemsListViewModel>();

                c.CreateMap<StoreItemViewModel, StoreItem>();
            });
        }
    }
}