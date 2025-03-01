namespace GlobeAuction.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DigitalCertificateUrl : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.DonationItems", "DigitalCertificateUrl", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.DonationItems", "DigitalCertificateUrl");
        }
    }
}
