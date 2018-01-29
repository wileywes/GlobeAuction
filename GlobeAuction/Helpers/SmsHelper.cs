using System.Configuration;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace GlobeAuction.Helpers
{
    public class SmsHelper
    {
        private static readonly string _twilioAccountSid = ConfigurationManager.AppSettings["TwilioAccountSid"];
        private static readonly string _twilioAuthToken = ConfigurationManager.AppSettings["TwilioAuthToken"];
        //private static readonly string _twilioPhoneFrom = ConfigurationManager.AppSettings["TwilioPhoneFrom"];
        private static readonly string _copilotSid = ConfigurationManager.AppSettings["TwilioCopilotServiceSid"];

        public void SendSms(string toPhone, string body)
        {
            TwilioClient.Init(_twilioAccountSid, _twilioAuthToken);

            var message = MessageResource.Create(
                to: new PhoneNumber(toPhone),
                messagingServiceSid: _copilotSid,
                body: body);            
        }
    }
}