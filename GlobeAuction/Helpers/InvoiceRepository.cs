using GlobeAuction.Models;
using System;
using System.Collections.Generic;

namespace GlobeAuction.Helpers
{
    public class InvoiceRepository
    {
        private ApplicationDbContext db;

        public InvoiceRepository(ApplicationDbContext context)
        {
            db = context;
        }

        public Invoice CreateInvoice(Bidder bidder, List<AuctionItem> auctionItems)
        {
            var newInvoice = new Invoice
            {
                AuctionItems = auctionItems,
                Bidder = bidder,
                CreateDate = DateTime.Now,
                IsPaid = false,
                UpdateBy = bidder.FirstName + "  " + bidder.LastName,
                UpdateDate = DateTime.Now
            };

            db.Invoices.Add(newInvoice);
            db.SaveChanges();

            return newInvoice;
        }
    }
}