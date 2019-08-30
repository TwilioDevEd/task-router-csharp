using System.Web.Configuration;

namespace TaskRouter.Web.Infrastructure
{
    public class Config
    {
        public Config() { }

        public virtual string ENV
        {
            get { return WebConfigurationManager.AppSettings["ENV"]; }
        }

        public virtual string AccountSID
        {
            get { return WebConfigurationManager.AppSettings["AccountSID"]; }
        }

        public virtual string AuthToken
        {
            get { return WebConfigurationManager.AppSettings["AuthToken"]; }
        }

        public virtual string TwilioNumber
        {
            get { return WebConfigurationManager.AppSettings["TwilioNumber"]; }
        }

        public virtual string HostUrl
        {
            get { return WebConfigurationManager.AppSettings["HostUrl"]; }
        }

        public virtual string VoiceMail
        {
            get { return WebConfigurationManager.AppSettings["VoiceMail"]; }
        }

        public virtual string AgentForProgrammableVoice
        {
            get { return WebConfigurationManager.AppSettings["AgentForProgrammableVoice"]; }
        }

        public virtual string AgentForProgrammableSMS
        {
            get { return WebConfigurationManager.AppSettings["AgentForProgrammableSMS"]; }
        }
    }
}