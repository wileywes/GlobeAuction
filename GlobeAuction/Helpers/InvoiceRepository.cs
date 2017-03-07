using AutoMapper;
using GlobeAuction.Models;
using System;
using System.Data.Entity;
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
                UpdateDate = DateTime.Now,
                Email = bidder.Email,
                FirstName = bidder.FirstName,
                LastName = bidder.LastName,
                Phone = bidder.Phone,
                ZipCode = bidder.ZipCode
            };

            db.Invoices.Add(invoice);
            db.SaveChanges();

            return InvoiceCreateResultType.Success;
        }

        public Invoice CreateInvoiceForStoreItems(BuyViewModel buyModel)
        {
            var storeItemPurchases = buyModel.StoreItemPurchases
                .Where(s => s.Quantity > 0)
                .Select(s => Mapper.Map<StoreItemPurchase>(s))
                .ToList();

            foreach (var sip in storeItemPurchases)
            {
                db.Entry(sip.StoreItem).State = EntityState.Unchanged;
            }

            var invoice = new Invoice
            {
                StoreItemPurchases = storeItemPurchases,
                CreateDate = DateTime.Now,
                IsPaid = false,
                UpdateBy = buyModel.FirstName + "  " + buyModel.LastName,
                UpdateDate = DateTime.Now,
                Email = buyModel.Email,
                FirstName = buyModel.FirstName,
                LastName = buyModel.LastName,
                Phone = buyModel.Phone,
                ZipCode = buyModel.ZipCode                
            };

            db.Invoices.Add(invoice);
            db.SaveChanges();

            return invoice;
        }
        
        public void ApplyPaymentToInvoice(PayPalTransaction ppTrans, Invoice invoice)
        {
            //only apply the ticket payment if there are un-paid tickets and the PP trans was successful
            if (ppTrans.WasPaymentSuccessful && invoice.IsPaid == false)
            {
                invoice.PaymentTransaction = ppTrans;
                invoice.IsPaid = true;

                var paymentLeft = ppTrans.PaymentGross;
                
                foreach (var storeItem in invoice.StoreItemPurchases)
                {
                    var lineExtendedPrice = storeItem.StoreItem.Price * storeItem.Quantity;
                    var priceToUseUp = Math.Min(lineExtendedPrice, paymentLeft);
                    storeItem.PricePaid = priceToUseUp;
                    storeItem.PurchaseTransaction = ppTrans;
                    paymentLeft -= priceToUseUp;
                }

                db.SaveChanges();

                new EmailHelper().SendInvoicePaymentConfirmation(invoice);
            }
        }

        public static int? GetInvoiceIdFromTransaction(PayPalTransaction ppTrans)
        {
            var invoiceIdStr = (ppTrans.Custom ?? string.Empty).Replace("Invoice:", "");
            int invoiceId;
            if (int.TryParse(invoiceIdStr, out invoiceId)) return invoiceId;
            return null;
        }
    }
}