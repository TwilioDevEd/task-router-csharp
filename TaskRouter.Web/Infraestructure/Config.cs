using System.Web.Configuration;

namespace TaskRouter.Web.Infraestructure
{
    public class Config
    {
        public static string AccountSID
        {
            get { return WebConfigurationManager.AppSettings["AccountSID"]; }
        }

        public static string AuthToken
        {
            get { return WebConfigurationManager.AppSettings["AuthToken"]; }
        }

        public static string TwilioNumber
        {
            get { return WebConfigurationManager.AppSettings["TwilioNumber"]; }
        }
    }
}