using System.Web.Configuration;

namespace TaskRouter.Web.Infrastructure
{
    public class Config
    {
        public static string ENV 
        {
            get { return WebConfigurationManager.AppSettings["ENV"]; }
        }

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

        public static string HostUrl
        {
            get { return WebConfigurationManager.AppSettings["HostUrl"]; }
        }

        public static string VoiceMail
        {
            get { return WebConfigurationManager.AppSettings["VoiceMail"]; }
        }

        public static string AgentForProgrammableVoice
        {
            get { return WebConfigurationManager.AppSettings["AgentForProgrammableVoice"]; }
        }

        public static string AgentForProgrammableSMS
        {
            get { return WebConfigurationManager.AppSettings["AgentForProgrammableSMS"]; }
        }
    }
}