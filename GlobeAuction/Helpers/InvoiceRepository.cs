using GlobeAuction.Models;
using System;
using System.Linq;

namespace GlobeAuction.Helpers
{
    public enum InvoiceCreateResultType
    {
        Success,
        NoRemainingItems
    }

    public class InvoiceRepository
    {
        private ApplicationDbContext db;

        public InvoiceRepository(ApplicationDbContext context)
        {
            db = context;
        }


        public InvoiceCreateResultType TryCreateInvoiceForWonItemsNotAlreadyOnInvoice(Bidder bidder, out Invoice invoice)
        {
            var winnings = db.AuctionItems.Where(ai =>
                    ai.WinningBidderId.HasValue &&
                    ai.WinningBidderId.Value == bidder.BidderId &&
                    ai.Invoice == null
                ).ToList();

            if (!winnings.Any())
            {
                invoice = null;
                return InvoiceCreateResultType.NoRemainingItems;
            }

            invoice = new Invoice
            {
                AuctionItems = winnings,
                Bidder = bidder,
                CreateDate = DateTime.Now,
                IsPaid = false,
                UpdateBy = bidder.FirstName + "  " + bidder.LastName,
                UpdateDate = DateTime.Now
            };

            db.Invoices.Add(invoice);
            db.SaveChanges();

            return InvoiceCreateResultType.Success;
        }
    }
}