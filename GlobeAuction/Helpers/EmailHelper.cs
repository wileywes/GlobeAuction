using GlobeAuction.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace GlobeAuction.Helpers
{
    public class EmailHelper
    {
        private static readonly string _gmailUsername = ConfigurationManager.AppSettings["GmailAccountUsername"];
        private static readonly string _gmailPassword = ConfigurationManager.AppSettings["GmailAccountPassword"];
        private static readonly string _siteName = ConfigurationManager.AppSettings["SiteName"];
        private static readonly string _siteUrl = ConfigurationManager.AppSettings["SiteUrl"];
        private static readonly string _allEmailBcc = ConfigurationManager.AppSettings["AllEmailBcc"];
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
                    .Select(s => new Tuple<string, decimal>(s.StoreItem.Title, s.PricePaid.Value)));
            }

            var body = GetInvoiceEmail(trans.PaymentGross,
                bidder.FirstName + " " + bidder.LastName,
                "Transaction ID " + trans.TxnId,
                DateTime.Now.ToString("g"),
                lines);

            SendEmail(bidder.Email, "Ticket Confirmation", body);
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
            var msg = new MailMessage(new MailAddress(_gmailUsername, _siteName), new MailAddress(to))
            {
                Body = body,
                IsBodyHtml = true,
                Subject = subject
            };

            if (!string.IsNullOrEmpty(_allEmailBcc))
            {
                msg.Bcc.Add(_allEmailBcc);
            }

            var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(_gmailUsername, _gmailPassword),
                EnableSsl = true
            };
            smtp.Send(msg);
        }
    }
}