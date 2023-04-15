namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class EmailTemplateInDb : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.EmailTemplates",
                c => new
                    {
                        EmailTemplateId = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Description = c.String(nullable: false),
                        Subject = c.String(nullable: false),
                        SupportedTokensCsvList = c.String(),
                        HtmlBody = c.String(),
                        CreateDate = c.DateTime(nullable: false),
                        UpdateDate = c.DateTime(nullable: false),
                        UpdateBy = c.String(),
                    })
                .PrimaryKey(t => t.EmailTemplateId);

            Sql(@"INSERT INTO [dbo].[EmailTemplates] (Name, Description, Subject, SupportedTokensCsvList, HtmlBody, CreateDate, UpdateDate, UpdateBy) VALUES 
                ('auctionWinningsNudge', 'Sent after auction closes for bidder to pay for their winnings.', 'Auction Checkout', 'SiteName,ItemCount,TotalOwed,PayLink,Address1,Address2,CheckoutNotes,FooterNotes,InvoiceLines,SiteUrl,SiteEmail', 'HtmlBody', '2023-04-14', '2023-04-14', 'wes')");

            Sql(@"INSERT INTO [dbo].[EmailTemplates] (Name, Description, Subject, SupportedTokensCsvList, HtmlBody, CreateDate, UpdateDate, UpdateBy) VALUES 
                ('auctionWinningsNudgeAfterEvent', 'Sent a day or two after auction has closed for any bidder that still needs to pay for winnings', 'Auction Checkout', 'SiteName,ItemCount,PaidItemCount,UnpaidItemCount,TotalUnpaid,TotalPaid,PayLink,Address1,Address2,PaidFooterNotes,UnpaidFooterNotes,UnpaidInvoiceLines,PaidInvoiceLines,SiteUrl,SiteEmail', 'HtmlBody', '2023-04-14', '2023-04-14', 'wes')");

            Sql(@"INSERT INTO [dbo].[EmailTemplates] (Name, Description, Subject, SupportedTokensCsvList, HtmlBody, CreateDate, UpdateDate, UpdateBy) VALUES 
                ('bidderNotPaid', 'Sent to a bidder if they register but do not pay within 24 hours.', 'Ticket Payment Reminder', 'SiteName,SiteUrl,SiteEmail,CompletePaymentUrl', 'HtmlBody', '2023-04-14', '2023-04-14', 'wes')");

            Sql(@"INSERT INTO [dbo].[EmailTemplates] (Name, Description, Subject, SupportedTokensCsvList, HtmlBody, CreateDate, UpdateDate, UpdateBy) VALUES 
                ('bidderRegistrationConfirmation', 'Sent when registration is completed and paid manually or when bidder requests their bidder number.', 'Your Bid Number for The GLOBE Auction', 'SiteName,SiteUrl,SiteEmail,BidderNumber,CompletePaymentUrl', 'HtmlBody', '2023-04-14', '2023-04-14', 'wes')");

            Sql(@"INSERT INTO [dbo].[EmailTemplates] (Name, Description, Subject, SupportedTokensCsvList, HtmlBody, CreateDate, UpdateDate, UpdateBy) VALUES 
                ('bidderRegistrationNudgeForPaidBidder', 'Sends bidder nudges for auction launch with paddle number and mobile bidding instructions.', 'Your Bid Number for The GLOBE Auction', 'SiteName,SiteUrl,SiteEmail,BidderNumber,CompletePaymentUrl', 'HtmlBody', '2023-04-14', '2023-04-14', 'wes')");

            Sql(@"INSERT INTO [dbo].[EmailTemplates] (Name, Description, Subject, SupportedTokensCsvList, HtmlBody, CreateDate, UpdateDate, UpdateBy) VALUES 
                ('bidderRegistrationNudgeForUnpaidBidder', 'Sends bidder nudges for auction launch with paddle number and mobile bidding instructions.', 'Your Bid Number for The GLOBE Auction', 'SiteName,SiteUrl,SiteEmail,BidderNumber,CompletePaymentUrl', 'HtmlBody', '2023-04-14', '2023-04-14', 'wes')");

            Sql(@"INSERT INTO [dbo].[EmailTemplates] (Name, Description, Subject, SupportedTokensCsvList, HtmlBody, CreateDate, UpdateDate, UpdateBy) VALUES 
                ('donationItemCertificate', 'Sends certificates for digital items when they are paid for via auction checkout or a store purchase.', 'Certificate of Auction Won', 'WinnerName,Title,Description,DonorBusiness,DonorName,DonorEmail,DonorPhone,ItemDetails1,ItemDetails2,ReceiptId,SiteName,SiteUrl,SiteEmail', 'HtmlBody', '2023-04-14', '2023-04-14', 'wes')");

            Sql(@"INSERT INTO [dbo].[EmailTemplates] (Name, Description, Subject, SupportedTokensCsvList, HtmlBody, CreateDate, UpdateDate, UpdateBy) VALUES 
                ('donorReceipt', 'Sends donor receipts for all donation items with a dollar value.', 'Thanks for your contribution to ""An Evening Around the GLOBE""', 'DonationTotal,DonorName,CurrentDate,DonationLines', 'HtmlBody', '2023-04-14', '2023-04-14', 'wes')");

            Sql(@"INSERT INTO [dbo].[EmailTemplates] (Name, Description, Subject, SupportedTokensCsvList, HtmlBody, CreateDate, UpdateDate, UpdateBy) VALUES 
                ('donorReceiptLine', 'Template inside of donor receipts for each line item', 'subject not used', 'LineName,LinePrice', 'HtmlBody', '2023-04-14', '2023-04-14', 'wes')");

            Sql(@"INSERT INTO [dbo].[EmailTemplates] (Name, Description, Subject, SupportedTokensCsvList, HtmlBody, CreateDate, UpdateDate, UpdateBy) VALUES 
                ('invoiceLine', 'Template inside of emails with lists of items', 'subject not used', 'LineName,LinePrice', 'HtmlBody', '2023-04-14', '2023-04-14', 'wes')");

            Sql(@"INSERT INTO [dbo].[EmailTemplates] (Name, Description, Subject, SupportedTokensCsvList, HtmlBody, CreateDate, UpdateDate, UpdateBy) VALUES 
                ('invoiceNotPaid', 'Sends payment nudge for standalone store purchases that are not paid after 24 hours.', 'GLOBE Auction Payment Reminder', 'SiteName,SiteUrl,SiteEmail,CompletePaymentUrl', 'HtmlBody', '2023-04-14', '2023-04-14', 'wes')");

            Sql(@"INSERT INTO [dbo].[EmailTemplates] (Name, Description, Subject, SupportedTokensCsvList, HtmlBody, CreateDate, UpdateDate, UpdateBy) VALUES 
                ('invoicePaid', 'description', 'Ticket Confirmation', 'SiteName,TotalPaid,Address1,Address2,Address3,FooterNotes,InvoiceLines,SiteUrl,SiteEmail', 'HtmlBody', '2023-04-14', '2023-04-14', 'wes')");
        }

        public override void Down()
        {
            DropTable("dbo.EmailTemplates");
        }
    }
}
