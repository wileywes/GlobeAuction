using GlobeAuction.Helpers;
using NUnit.Framework;

namespace GlobeAuction.UnitTests
{
    [TestFixture]
    public class SendGridTests
    {
        [Test, Explicit]
        public void Concrete_SendEmail()
        {
            new EmailHelper(string.Empty).SendEmail("williams.wes@gmail.com", "test", "test");
        }
    }
}
