﻿using GlobeAuction.Models;
using NLog;
using System;
using System.Linq;
using System.Threading;

namespace GlobeAuction.Helpers
{
    public class BackgroundMaintenance
    {
        private Logger _logger = LogManager.GetCurrentClassLogger();
        private ApplicationDbContext db = new ApplicationDbContext();
        private DateTime _lastMaintenance = DateTime.MinValue;
        private static readonly TimeSpan TimeBetweenMaintenance = TimeSpan.FromMinutes(60);
        private readonly string _baseFilePath;

        public BackgroundMaintenance(string baseFilePath)
        {
            _baseFilePath = baseFilePath;
        }

        public void DoMaintenance(CancellationToken cancelToken)
        {
            var emailHelper = new EmailHelper(_baseFilePath);

            while (true)
            {
                if (cancelToken.IsCancellationRequested) return;

                if (DateTime.Now.Subtract(_lastMaintenance) > TimeBetweenMaintenance)
                {
                    _lastMaintenance = DateTime.Now;

                    var oneDayAgo = DateTime.Now.AddDays(-1);

                    var biddersThatHaveNotPaid = db.Bidders
                        .Where(b => b.CreateDate < oneDayAgo 
                                && b.AuctionGuests.Any(g => g.TicketPricePaid.HasValue == false)
                                && b.IsPaymentReminderSent == false)
                        .ToList();

                    foreach(var bidder in biddersThatHaveNotPaid)
                    {
                        emailHelper.SendBidderPaymentReminder(bidder);
                        bidder.IsPaymentReminderSent = true; //just send this once per bidder
                        db.SaveChanges();

                        _logger.Info("Sent payment reminder to {0}, bidder ID {1}", bidder.Email, bidder.BidderId);
                    }
                }

                Thread.Sleep(TimeSpan.FromSeconds(60));
            }
        }        
    }
}