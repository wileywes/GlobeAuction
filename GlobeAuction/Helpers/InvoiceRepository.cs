using GlobeAuction.Models;
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

        public Invoice CreateInvoiceForAuctionItems(Bidder bidder, List<Bid> winnings, List<StoreItemPurchase> storeItemPurchases, PaymentMethod? manualPayMethod, string updatedBy)
        {
            var invoice = CreateInvoiceFromBidder(bidder, winnings, storeItemPurchases, null, InvoiceType.AuctionCheckout, updatedBy);
            ApplyPotentialManualPayment(invoice, manualPayMethod, updatedBy);

            db.Invoices.Add(invoice);
            db.SaveChanges();

            if (manualPayMethod.HasValue)
            {
                new EmailHelper().SendInvoicePaymentConfirmation(invoice, true);
            }

            return invoice;
        }

        public Invoice CreateInvoiceForBidderRegistration(Bidder bidder, BidderRegistrationViewModel regModel, PaymentMethod? manualPayMethod, string updatedBy)
        {
            var storeItemPurchases = new ItemsRepository(db).GetStorePurchasesWithIndividualizedRaffleTickets(regModel.ItemPurchases);
            var ticketPurchases = bidder.AuctionGuests?.
                Select(ag => new TicketPurchase
                {
                    AuctionGuest = ag,
                    TicketPrice = ag.TicketPrice,
                    TicketType = ag.TicketType
                }).ToList();

            var invoice = CreateInvoiceFromBidder(bidder, null, storeItemPurchases, ticketPurchases, InvoiceType.BidderRegistration, updatedBy);

            if (invoice.Total <= 0)
            {
                //force to paid by cash if total is zero
                manualPayMethod = PaymentMethod.Cash;
            }

            ApplyPotentialManualPayment(invoice, manualPayMethod, updatedBy);

            db.Invoices.Add(invoice);
            db.SaveChanges();

            if (manualPayMethod.HasValue)
            {
                new EmailHelper().SendInvoicePaymentConfirmation(invoice, true);
            }

            return invoice;
        }

        private Invoice CreateInvoiceFromBidder(Bidder bidder, List<Bid> winnings, List<StoreItemPurchase> storeItemPurchases, List<TicketPurchase> ticketPurchases,
            InvoiceType type, string updatedBy)
        {
            return new Invoice
            {
                //child items
                Bids = winnings,
                StoreItemPurchases = storeItemPurchases,
                TicketPurchases = ticketPurchases,

                //properties
                Bidder = bidder,
                CreateDate = Utilities.GetEasternTimeNow(),
                Email = bidder.Email,
                FirstName = bidder.FirstName,
                InvoiceType = type,
                IsPaid = false,
                TotalPaid = 0,
                LastName = bidder.LastName,
                Phone = bidder.Phone,
                UpdateBy = updatedBy,
                UpdateDate = Utilities.GetEasternTimeNow(),
                ZipCode = bidder.ZipCode           
            };
        }

        public Invoice CreateInvoiceForStoreItems(BuyViewModel buyModel, PaymentMethod? manualPayMethod, string updatedBy)
        {
            var allPurchases = new List<BuyItemViewModel>();
            if (buyModel.RaffleItems != null) allPurchases.AddRange(buyModel.RaffleItems);
            if (buyModel.FundAProjectItems != null) allPurchases.AddRange(buyModel.FundAProjectItems);
            if (buyModel.GeneralStoreItems != null) allPurchases.AddRange(buyModel.GeneralStoreItems);

            var storeItemPurchases = new ItemsRepository(db).GetStorePurchasesWithIndividualizedRaffleTickets(allPurchases);

            var invoice = new Invoice
            {
                //child items
                StoreItemPurchases = storeItemPurchases,

                //properties
                CreateDate = Utilities.GetEasternTimeNow(),
                Email = buyModel.Email,
                FirstName = buyModel.FirstName,
                InvoiceType = InvoiceType.AuctionCheckout,
                IsPaid = false,
                TotalPaid = 0,
                LastName = buyModel.LastName,
                Phone = buyModel.Phone,
                UpdateBy = buyModel.FirstName + "  " + buyModel.LastName,
                UpdateDate = Utilities.GetEasternTimeNow(),
                ZipCode = buyModel.ZipCode
            };

            ApplyPotentialManualPayment(invoice, manualPayMethod, updatedBy);

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

        public void ApplyPotentialManualPayment(Invoice invoice, PaymentMethod? manualPayMethod, string updatedBy)
        {
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

                if (invoice.TicketPurchases != null)
                {
                    foreach(var ticket in invoice.TicketPurchases)
                    {
                        ticket.TicketPricePaid = ticket.AuctionGuest.TicketPrice;
                    }
                }

                if (invoice.Bids != null)
                {
                    foreach (var bid in invoice.Bids)
                    {
                        bid.AmountPaid = bid.BidAmount;
                    }
                }

                //mark all as paid when it's manual
                invoice.TotalPaid = invoice.Total;

                RevenueHelper.IncrementTotalRevenue(invoice.TotalPaid);
            }
        }

        public void ApplyPaymentToInvoice(PayPalTransaction ppTrans, Invoice invoice)
        {
            if (ppTrans.WasPaymentSuccessful && invoice.IsPaid == false)
            {
                invoice.PaymentTransaction = ppTrans;
                invoice.IsPaid = true;
                invoice.TotalPaid = ppTrans.PaymentGross;
                invoice.PaymentMethod = PaymentMethod.PayPal;
                invoice.UpdateDate = Utilities.GetEasternTimeNow();
                invoice.UpdateBy = ppTrans.PayerEmail;
                
                //for the child items we assume all were paid in full and we will check in the background if we ever have invoices paid partially
                foreach (var storeItem in invoice.StoreItemPurchases)
                {
                    var lineExtendedPrice = storeItem.Price * storeItem.Quantity;
                    storeItem.PricePaid = lineExtendedPrice;
                    storeItem.PurchaseTransaction = ppTrans;
                }

                foreach(var ticket in invoice.TicketPurchases)
                {
                    ticket.TicketPricePaid = ticket.AuctionGuest.TicketPrice;
                }

                foreach(var bid in invoice.Bids)
                {
                    //have to set these for some reason
                    bid.AuctionItem = bid.AuctionItem;
                    bid.Bidder = bid.Bidder;

                    bid.AmountPaid = bid.BidAmount;
                }

                db.SaveChanges();

                RevenueHelper.IncrementTotalRevenue(invoice.TotalPaid);

                if (invoice.InvoiceType == InvoiceType.BidderRegistration)
                {
                    new EmailHelper().SendBidderPaymentConfirmation(invoice);
                }
                else
                {
                    new EmailHelper().SendInvoicePaymentConfirmation(invoice, false);
                }
            }
        }

        public Invoice GetRegistrationInvoiceForBidder(Bidder bidder)
        {
            return db.Invoices.FirstOrDefault(i => i.InvoiceType == InvoiceType.BidderRegistration && i.Bidder.BidderId == bidder.BidderId);
        }

        public void DeleteInvoice(Invoice invoice)
        {
            foreach (var sip in db.StoreItemPurchases.Where(p => p.Invoice.InvoiceId == invoice.InvoiceId).ToList())
            {
                //add the quantity back if it's not a raffle ticket
                if (sip.StoreItem.IsRaffleTicket == false)
                {
                    sip.StoreItem.Quantity += sip.Quantity;
                }

                //remove store item purchase
                db.StoreItemPurchases.Remove(sip);
            }

            foreach (var bid in invoice.Bids) // db.Bids.Where(p => p.Invoice.InvoiceId == invoice.InvoiceId))
            {
                //detach auction items
                bid.Invoice = null;
                bid.AmountPaid = null;

                //force re-load these since they are required?
                bid.Bidder = bid.Bidder;
                bid.AuctionItem = bid.AuctionItem;
            }

            db.Invoices.Remove(invoice);
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