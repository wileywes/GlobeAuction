// ------------------------------------------------------------------------------
//  <auto-generated>
//      This code was generated by SpecFlow (http://www.specflow.org/).
//      SpecFlow Version:3.1.0.0
//      SpecFlow Generator Version:3.1.0.0
// 
//      Changes to this file may cause incorrect behavior and will be lost if
//      the code is regenerated.
//  </auto-generated>
// ------------------------------------------------------------------------------
#region Designer generated code
#pragma warning disable
namespace GlobeAuction.Features.Features
{
    using TechTalk.SpecFlow;
    using System;
    using System.Linq;
    
    
    [System.CodeDom.Compiler.GeneratedCodeAttribute("TechTalk.SpecFlow", "3.1.0.0")]
    [System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [NUnit.Framework.TestFixtureAttribute()]
    [NUnit.Framework.DescriptionAttribute("BidCalculation")]
    public partial class BidCalculationFeature
    {
        
        private TechTalk.SpecFlow.ITestRunner testRunner;
        
        private string[] _featureTags = ((string[])(null));
        
#line 1 "BidCalculation.feature"
#line hidden
        
        [NUnit.Framework.OneTimeSetUpAttribute()]
        public virtual void FeatureSetup()
        {
            testRunner = TechTalk.SpecFlow.TestRunnerManager.GetTestRunner();
            TechTalk.SpecFlow.FeatureInfo featureInfo = new TechTalk.SpecFlow.FeatureInfo(new System.Globalization.CultureInfo("en-US"), "BidCalculation", "\tValidate calculations on winners for item when bids are changed", ProgrammingLanguage.CSharp, ((string[])(null)));
            testRunner.OnFeatureStart(featureInfo);
        }
        
        [NUnit.Framework.OneTimeTearDownAttribute()]
        public virtual void FeatureTearDown()
        {
            testRunner.OnFeatureEnd();
            testRunner = null;
        }
        
        [NUnit.Framework.SetUpAttribute()]
        public virtual void TestInitialize()
        {
        }
        
        [NUnit.Framework.TearDownAttribute()]
        public virtual void TestTearDown()
        {
            testRunner.OnScenarioEnd();
        }
        
        public virtual void ScenarioInitialize(TechTalk.SpecFlow.ScenarioInfo scenarioInfo)
        {
            testRunner.OnScenarioInitialize(scenarioInfo);
            testRunner.ScenarioContext.ScenarioContainer.RegisterInstanceAs<NUnit.Framework.TestContext>(NUnit.Framework.TestContext.CurrentContext);
        }
        
        public virtual void ScenarioStart()
        {
            testRunner.OnScenarioStart();
        }
        
        public virtual void ScenarioCleanup()
        {
            testRunner.CollectScenarioErrors();
        }
        
        [NUnit.Framework.TestAttribute()]
        [NUnit.Framework.DescriptionAttribute("Adding a new bid to an item should recalculate the winners")]
        [NUnit.Framework.CategoryAttribute("hook_purgeall")]
        public virtual void AddingANewBidToAnItemShouldRecalculateTheWinners()
        {
            string[] tagsOfScenario = new string[] {
                    "hook_purgeall"};
            TechTalk.SpecFlow.ScenarioInfo scenarioInfo = new TechTalk.SpecFlow.ScenarioInfo("Adding a new bid to an item should recalculate the winners", null, new string[] {
                        "hook_purgeall"});
#line 5
this.ScenarioInitialize(scenarioInfo);
#line hidden
            bool isScenarioIgnored = default(bool);
            bool isFeatureIgnored = default(bool);
            if ((tagsOfScenario != null))
            {
                isScenarioIgnored = tagsOfScenario.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((this._featureTags != null))
            {
                isFeatureIgnored = this._featureTags.Where(__entry => __entry != null).Where(__entry => String.Equals(__entry, "ignore", StringComparison.CurrentCultureIgnoreCase)).Any();
            }
            if ((isScenarioIgnored || isFeatureIgnored))
            {
                testRunner.SkipScenario();
            }
            else
            {
                this.ScenarioStart();
                TechTalk.SpecFlow.Table table1 = new TechTalk.SpecFlow.Table(new string[] {
                            "Name",
                            "ItemNumberStart",
                            "ItemNumberEnd",
                            "BidOpenDateLtz",
                            "BidCloseDateLtz",
                            "IsFundAProject",
                            "IsAvailableForMobileBidding",
                            "IsOnlyAvailableToAuctionItems"});
                table1.AddRow(new string[] {
                            "SpecflowTestCategory",
                            "90000",
                            "99999",
                            "1/1/2000",
                            "1/1/2200",
                            "false",
                            "false",
                            "false"});
#line 6
 testRunner.Given("I create these auction categories", ((string)(null)), table1, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table2 = new TechTalk.SpecFlow.Table(new string[] {
                            "Title",
                            "Description",
                            "Quantity",
                            "DollarValue"});
                table2.AddRow(new string[] {
                            "Test",
                            "Test Description",
                            "1",
                            "100"});
#line 9
 testRunner.Given("I create these donation items in category \'SpecflowTestCategory\'", ((string)(null)), table2, "Given ");
#line hidden
                TechTalk.SpecFlow.Table table3 = new TechTalk.SpecFlow.Table(new string[] {
                            "Title",
                            "Description",
                            "Quantity",
                            "DollarValue"});
                table3.AddRow(new string[] {
                            "Test",
                            "Test Description",
                            "1",
                            "100"});
#line 12
 testRunner.Then("the donation items in the category \'SpecflowTestCategory\' are", ((string)(null)), table3, "Then ");
#line hidden
#line 15
 testRunner.When("I convert the created donation items to single auction items", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
                TechTalk.SpecFlow.Table table4 = new TechTalk.SpecFlow.Table(new string[] {
                            "UniqueItemNumber",
                            "Title",
                            "Description",
                            "StartingBid",
                            "BidIncrement",
                            "Quantity"});
                table4.AddRow(new string[] {
                            "90000",
                            "Test",
                            "Test Description",
                            "40",
                            "5",
                            "1"});
#line 16
 testRunner.Then("the auction items in the category \'SpecflowTestCategory\' are", ((string)(null)), table4, "Then ");
#line hidden
                TechTalk.SpecFlow.Table table5 = new TechTalk.SpecFlow.Table(new string[] {
                            "FirstName",
                            "LastName",
                            "Phone",
                            "Email",
                            "ZipCode"});
                table5.AddRow(new string[] {
                            "John",
                            "Smith",
                            "123-123-1234",
                            "john@gmail.com",
                            "30001"});
                table5.AddRow(new string[] {
                            "Sally",
                            "Fields",
                            "456-456-4567",
                            "sally@gmail.com",
                            "30002"});
#line 19
 testRunner.When("I create the following bidders using the \'Register and Mark Paid (Cash)\' button", ((string)(null)), table5, "When ");
#line hidden
                TechTalk.SpecFlow.Table table6 = new TechTalk.SpecFlow.Table(new string[] {
                            "BidderNumber",
                            "FirstName",
                            "LastName",
                            "Phone",
                            "Email",
                            "ZipCode",
                            "GuestCount",
                            "TicketsPaid",
                            "ItemsCount",
                            "ItemsPaid",
                            "TotalPaid",
                            "PaymentMethod"});
                table6.AddRow(new string[] {
                            "1",
                            "John",
                            "Smith",
                            "123-123-1234",
                            "john@gmail.com",
                            "30001",
                            "1",
                            "1",
                            "0",
                            "0",
                            "1.00",
                            "Cash"});
                table6.AddRow(new string[] {
                            "2",
                            "Sally",
                            "Fields",
                            "456-456-4567",
                            "sally@gmail.com",
                            "30002",
                            "1",
                            "1",
                            "0",
                            "0",
                            "1.00",
                            "Cash"});
#line 23
 testRunner.Then("the bidders in the system are", ((string)(null)), table6, "Then ");
#line hidden
#line 27
 testRunner.When("I log in as bidder number \'1\' with last name \'Smith\' and email \'john@gmail.com\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "When ");
#line hidden
#line 28
 testRunner.Then("I enter a bid of \'1.00\' for item number \'90000\'", ((string)(null)), ((TechTalk.SpecFlow.Table)(null)), "Then ");
#line hidden
            }
            this.ScenarioCleanup();
        }
    }
}
#pragma warning restore
#endregion
