using AutoMapper;
using GlobeAuction.Models;
using System;
using System.Data.Entity;
using System.Collections.Generic;
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

        public Invoice CreateInvoiceForAuctionItems(Bidder bidder, List<AuctionItem> winnings, List<StoreItemPurchase> storeItemPurchases)
        {
            var invoice = new Invoice
            {
                AuctionItems = winnings,
                StoreItemPurchases = storeItemPurchases,
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

            return invoice;
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
            if (ppTrans.WasPaymentSuccessful && invoice.IsPaid == false)
            {
                invoice.PaymentTransaction = ppTrans;
                invoice.IsPaid = true;
                invoice.UpdateDate = DateTime.Now;
                invoice.UpdateBy = ppTrans.PayerEmail;

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

                new EmailHelper().SendInvoicePaymentConfirmation(invoice, false);
            }
        }

        public void MarkPaidManually(Invoice invoice, string username)
        {
            if (invoice.IsPaid == false)
            {
                invoice.IsPaid = true;
                invoice.WasMarkedPaidManually = true;
                invoice.UpdateBy = username;
                invoice.UpdateDate = DateTime.Now;

                db.SaveChanges();

                new EmailHelper().SendInvoicePaymentConfirmation(invoice, true);
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