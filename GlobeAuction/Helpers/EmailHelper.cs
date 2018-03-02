using GlobeAuction.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Web;

namespace GlobeAuction.Helpers
{
    /// <summary>
    /// NOTE: GMail has a limit of 100 emails per day through SMTP
    /// </summary>
    public class EmailHelper
    {
        private static readonly string _gmailUsername = ConfigurationManager.AppSettings["GmailAccountUsername"];
        private static readonly string _gmailPassword = ConfigurationManager.AppSettings["GmailAccountPassword"];
        private static readonly string _siteName = ConfigurationManager.AppSettings["SiteName"];
        private static readonly string _siteUrl = ConfigurationManager.AppSettings["SiteUrl"];
        private static readonly string _allEmailBcc = ConfigurationManager.AppSettings["AllEmailBcc"];
        private static readonly string _donorReceiptEmailBcc = ConfigurationManager.AppSettings["DonorReceiptEmailBcc"];
        private readonly string _baseFilePath;


        public EmailHelper()
            : this(HttpContext.Current.Server.MapPath("/"))
        {
        }

        public EmailHelper(string baseFilePath)
        {
            _baseFilePath = baseFilePath;
        }

        private string GetEmailBody(string inlinedTemplateName)
        {
            var path = GetTemplateFilePath(inlinedTemplateName);
            var content = File.ReadAllText(path);

            return content;
        }

        private Tuple<string, decimal> GetStoreItemPurchaseLineString(StoreItemPurchase sip)
        {
            if (sip.StoreItem.IsRaffleTicket)
            {
                return new Tuple<string, decimal>(sip.StoreItem.Title + " -  Ticket #" + sip.StoreItemPurchaseId.ToString("D8"), sip.PricePaid.Value);
            }

            return new Tuple<string, decimal>(sip.StoreItem.Title + (sip.Quantity > 1 ? " x " + sip.Quantity : ""), sip.PricePaid.Value);
        }

        public void SendBidderPaymentConfirmation(Bidder bidder, PayPalTransaction trans)
        {
            var lines = bidder.AuctionGuests
                .Where(g => g.TicketPricePaid.HasValue)
                .Select(g => new Tuple<string, decimal>(g.TicketType, g.TicketPricePaid.Value))
                .ToList();

            if (bidder.StoreItemPurchases.Any())
            {
                lines.AddRange(bidder.StoreItemPurchases
                    .Where(s => s.IsPaid)
                    .Select(GetStoreItemPurchaseLineString));
            }

            var body = GetInvoiceEmail(trans.PaymentGross,
                bidder.FirstName + " " + bidder.LastName,
                "Transaction ID " + trans.TxnId,
                DateTime.Now.ToString("g"),
                lines);

            SendEmail(bidder.Email, "Ticket Confirmation", body);
        }

        public void SendInvoicePaymentConfirmation(Invoice invoice, bool paidManually)
        {
            var lines = invoice.AuctionItems
                .Where(g => g.WinningBid.HasValue)
                .Select(g => new Tuple<string, decimal>(g.Title, g.WinningBid.Value))
                .ToList();

            if (invoice.StoreItemPurchases.Any())
            {
                lines.AddRange(invoice.StoreItemPurchases
                    .Where(s => s.IsPaid)
                    .Select(GetStoreItemPurchaseLineString));
            }

            var body = GetInvoiceEmail(
                paidManually ? invoice.Total : invoice.PaymentTransaction.PaymentGross,
                invoice.FirstName + " " + invoice.LastName,
                "Invoice ID " + invoice.InvoiceId,
                paidManually ? "Paid in Person" : "PayPal Transaction ID " + invoice.PaymentTransaction.TxnId,
                lines);

            SendEmail(invoice.Email, "Order Confirmation", body);
        }

        public void SendDonationItemCertificate(Bidder bidder, DonationItem item)
        {
            var body = GetDonationItemCertificateEmail(
                bidder.FirstName + " " + bidder.LastName,
                item.Title,
                item.Description);

            SendEmail(bidder.Email, item.Donor.Email, "Certificate of Auction Won", body);
        }

        public void SendAuctionWinningsPaymentNudge(Bidder bidder, List<AuctionItem> winnings, string payLink, bool isAfterEvent)
        {
            var lines = winnings
                .Select(g => new Tuple<string, decimal>(g.Title, g.WinningBid.Value))
                .ToList();

            var totalOwed = winnings.Sum(i => i.WinningBid.Value);
            var body = GetAuctionWinningsNudgeEmailEmail(winnings.Count,
                totalOwed, payLink,
                bidder.FirstName + " " + bidder.LastName,
                "Bidder # " + bidder.BidderId,
                lines,
                isAfterEvent);

            SendEmail(bidder.Email, "Auction Checkout", body);
        }

        public void SendDonorTaxReceipt(Donor donor, List<DonationItem> itemsToInclude)
        {
            var headerPath = Path.Combine(_baseFilePath, @"Content\EmailTemplates\inlined\thankyouheader.jpg");
            var body = GetDonorTaxReceiptEmail(itemsToInclude.Sum(i => i.DollarValue.Value),
                donor.BusinessName,
                itemsToInclude.Select(d => new Tuple<string, decimal>(d.Title, d.DollarValue.Value)).ToList());

            var imagesToEmbed = new Dictionary<string, string> { { "<!--%HeaderImage-->", headerPath } };

            SendEmail(donor.Email, null, "Thanks for your contribution to \"An Evening Around the GLOBE\"", body,
                false, _donorReceiptEmailBcc, imagesToEmbed);
        }

        private string GetDonorTaxReceiptEmail(decimal totalDonated, string donorName, List<Tuple<string, decimal>> donations)
        {
            var body = GetEmailBody("donorReceipt");

            var lineTemplate = GetEmailBody("donorReceiptLine");

            body = ReplaceToken("DonationTotal", totalDonated.ToString("C"), body);
            body = ReplaceToken("DonorName", donorName, body);
            body = ReplaceToken("CurrentDate", DateTime.Now.Date.ToString("D"), body);

            var linesHtml = string.Empty;
            foreach (var line in donations)
            {
                linesHtml += ReplaceToken("LineName", line.Item1, ReplaceToken("LinePrice", line.Item2.ToString("C"), lineTemplate));
            }

            body = ReplaceToken("DonationLines", linesHtml, body);

            return body;
        }

        private string GetAuctionWinningsNudgeEmailEmail(int itemCount, decimal totalOwed, string payLink, string address1, string address2, List<Tuple<string, decimal>> lines, bool isAfterEvent)
        {
            var template = isAfterEvent ? "auctionWinningsNudgeAfterEvent" : "auctionWinningsNudge";
            var body = GetEmailBody(template);

            var lineTemplate = GetEmailBody("invoiceLine");

            body = ReplaceToken("SiteName", _siteName, body);
            body = ReplaceToken("ItemCount", itemCount.ToString(), body);
            body = ReplaceToken("TotalOwed", totalOwed.ToString("C"), body);
            body = ReplaceToken("PayLink", payLink, body);
            body = ReplaceToken("Address1", address1, body);
            body = ReplaceToken("Address2", address2, body);

            var linesHtml = string.Empty;
            foreach (var line in lines)
            {
                linesHtml += ReplaceToken("LineName", line.Item1, ReplaceToken("LinePrice", line.Item2.ToString("C"), lineTemplate));
            }

            body = ReplaceToken("InvoiceLines", linesHtml, body);
            body = ReplaceToken("SiteUrl", _siteUrl, body);
            body = ReplaceToken("SiteEmail", _gmailUsername, body);

            return body;
        }

        private string GetInvoiceEmail(decimal totalPaid, string address1, string address2, string address3, List<Tuple<string, decimal>> lines)
        {
            var body = GetEmailBody("invoicePaid");
            var lineTemplate = GetEmailBody("invoiceLine");

            body = ReplaceToken("SiteName", _siteName, body);
            body = ReplaceToken("TotalPaid", totalPaid.ToString("C"), body);
            body = ReplaceToken("Address1", address1, body);
            body = ReplaceToken("Address2", address2, body);
            body = ReplaceToken("Address3", address3, body);

            var linesHtml = string.Empty;
            foreach (var line in lines)
            {
                linesHtml += ReplaceToken("LineName", line.Item1, ReplaceToken("LinePrice", line.Item2.ToString("C"), lineTemplate));
            }
            body = ReplaceToken("InvoiceLines", linesHtml, body);
            body = ReplaceToken("SiteUrl", _siteUrl, body);
            body = ReplaceToken("SiteEmail", _gmailUsername, body);

            return body;
        }

        private string GetDonationItemCertificateEmail(string winnerName, string itemTitle, string itemDescription)
        {
            var body = GetEmailBody("donationItemCertificate");

            body = ReplaceToken("WinnerName", winnerName, body);
            body = ReplaceToken("Title", itemTitle, body);
            body = ReplaceToken("Description", itemDescription, body);

            body = ReplaceToken("SiteUrl", _siteUrl, body);
            body = ReplaceToken("SiteEmail", _gmailUsername, body);
            body = ReplaceToken("SiteName", _siteName, body);

            return body;
        }

        public void SendBidderPaymentReminder(Bidder bidder)
        {
            var url = string.Format("{0}/Bidders/RedirectToPayPal/{1}", _siteUrl, bidder.BidderId);
            var body = GetPaymentReminderEmail(url);

            SendEmail(bidder.Email, "Ticket Payment Reminder", body);
        }

        private string GetPaymentReminderEmail(string paymentUrl)
        {
            var body = GetEmailBody("bidderNotPaid");

            body = ReplaceToken("SiteName", _siteName, body);
            body = ReplaceToken("SiteUrl", _siteUrl, body);
            body = ReplaceToken("SiteEmail", _gmailUsername, body);
            body = ReplaceToken("CompletePaymentUrl", paymentUrl, body);

            return body;
        }

        private static string ReplaceToken(string tokenKey, string value, string body)
        {
            var token = string.Format("<!--%{0}%-->", tokenKey);
            return body.Replace(token, value);
        }

        private string GetTemplateFilePath(string inlinedTemplateName)
        {
            return Path.Combine(_baseFilePath, @"Content\EmailTemplates\inlined\" + inlinedTemplateName + ".html");
        }

        public void SendEmail(string to, string subject, string body)
        {
            SendEmail(to, null, subject, body, true, null, null);
        }

        public void SendEmail(string to, string additionalTo, string subject, string body)
        {
            SendEmail(to, additionalTo, subject, body, true, null, null);
        }

        public void SendEmail(string to, string additionalTo, string subject, string body, bool includeAllEmailBcc, string additionalBccList, Dictionary<string, string> imagesToEmbedByTag)
        {
            var msg = new MailMessage(new MailAddress(_gmailUsername, _siteName), new MailAddress(to))
            {
                Body = body,
                IsBodyHtml = true,
                Subject = subject
            };

            if (!string.IsNullOrEmpty(additionalTo))
            {
                msg.To.Add(additionalTo);
            }

            if (!string.IsNullOrEmpty(_allEmailBcc) && includeAllEmailBcc)
            {
                var addresses = _allEmailBcc.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                addresses.ToList().ForEach(a => msg.Bcc.Add(a));
            }

            if (!string.IsNullOrEmpty(additionalBccList))
            {
                var addresses = additionalBccList.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                addresses.ToList().ForEach(a => msg.Bcc.Add(a));
            }

            if (imagesToEmbedByTag != null && imagesToEmbedByTag.Any())
            {
                foreach (var imgPair in imagesToEmbedByTag)
                {
                    AddEmbeddedImage(msg, imgPair.Key, imgPair.Value);
                }
            }

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(_gmailUsername, _gmailPassword),
                EnableSsl = true
            };

            Utilities.RetryIt(attemptNum => smtp.Send(msg), "SendEmail", 3);
        }

        private void AddEmbeddedImage(MailMessage msg, string imageTagInHtmlBody, string imageFilePath)
        {
            var inline = new LinkedResource(imageFilePath);
            inline.ContentId = Guid.NewGuid().ToString();
            inline.ContentType = new ContentType("image/jpg");
            msg.Body = msg.Body.Replace(imageTagInHtmlBody, @"<img src='cid:" + inline.ContentId + @"'/>");

            AlternateView alternateView = AlternateView.CreateAlternateViewFromString(msg.Body, null, MediaTypeNames.Text.Html);
            alternateView.LinkedResources.Add(inline);

            msg.AlternateViews.Add(alternateView);
        }
    }
}