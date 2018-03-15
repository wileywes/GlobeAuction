﻿using GlobeAuction.Models;
using System;
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

        public Invoice CreateInvoiceForAuctionItems(Bidder bidder, List<AuctionItem> winnings, List<StoreItemPurchase> storeItemPurchases, PaymentMethod? manualPayMethod, string updatedBy)
        {
            var invoice = new Invoice
            {
                AuctionItems = winnings,
                StoreItemPurchases = storeItemPurchases,
                Bidder = bidder,
                CreateDate = DateTime.Now,
                IsPaid = false,
                UpdateBy = updatedBy,
                UpdateDate = DateTime.Now,
                Email = bidder.Email,
                FirstName = bidder.FirstName,
                LastName = bidder.LastName,
                Phone = bidder.Phone,
                ZipCode = bidder.ZipCode
            };

            if (manualPayMethod.HasValue)
            {
                invoice.IsPaid = true;
                invoice.WasMarkedPaidManually = true;
                invoice.PaymentMethod = manualPayMethod.Value;
                invoice.UpdateBy = updatedBy;

                foreach (var storeItem in invoice.StoreItemPurchases)
                {
                    storeItem.PricePaid = storeItem.Price * storeItem.Quantity;
                }

                RevenueHelper.IncrementTotalRevenue(invoice.TotalPaid);
            }

            db.Invoices.Add(invoice);
            db.SaveChanges();

            if (manualPayMethod.HasValue)
            {
                new EmailHelper().SendInvoicePaymentConfirmation(invoice, true);
            }

            return invoice;
        }

        public Invoice CreateInvoiceForStoreItems(BuyViewModel buyModel, PaymentMethod? manualPayMethod, string updatedBy)
        {
            var allPurchases = new List<BuyItemViewModel>();
            if (buyModel.RaffleItems != null) allPurchases.AddRange(buyModel.RaffleItems);
            if (buyModel.StoreItems != null) allPurchases.AddRange(buyModel.StoreItems);

            var storeItemPurchases = new ItemsRepository(db).GetStorePurchasesWithIndividualizedRaffleTickets(allPurchases);

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

            if (manualPayMethod.HasValue)
            {
                invoice.IsPaid = true;
                invoice.WasMarkedPaidManually = true;
                invoice.PaymentMethod = manualPayMethod.Value;
                invoice.UpdateBy = updatedBy;

                foreach (var sip in invoice.StoreItemPurchases)
                {
                    sip.PricePaid = sip.Price * sip.Quantity;
                }

                RevenueHelper.IncrementTotalRevenue(invoice.TotalPaid);
            }

            if (buyModel.BidderId.GetValueOrDefault(-1) >= 0)
            {
                invoice.Bidder = db.Bidders.Find(buyModel.BidderId.Value);
            }
            else
            {
                //look for exact match by last name and email address automatically
                invoice.Bidder = db.Bidders.FirstOrDefault(b => b.LastName.Equals(buyModel.LastName, StringComparison.OrdinalIgnoreCase) &&
                            b.Email.Equals(buyModel.Email, StringComparison.OrdinalIgnoreCase) &&
                            b.IsDeleted == false);
            }

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
                invoice.PaymentMethod = PaymentMethod.PayPal;
                invoice.UpdateDate = DateTime.Now;
                invoice.UpdateBy = ppTrans.PayerEmail;

                var paymentLeft = ppTrans.PaymentGross;

                foreach (var storeItem in invoice.StoreItemPurchases)
                {
                    var lineExtendedPrice = storeItem.Price * storeItem.Quantity;
                    var priceToUseUp = Math.Min(lineExtendedPrice, paymentLeft);
                    storeItem.PricePaid = priceToUseUp;
                    storeItem.PurchaseTransaction = ppTrans;
                    paymentLeft -= priceToUseUp;
                }

                db.SaveChanges();

                RevenueHelper.IncrementTotalRevenue(invoice.TotalPaid);
                new EmailHelper().SendInvoicePaymentConfirmation(invoice, false);
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