using System.Collections.Generic;

namespace TaskRouter.Web.Infrastructure
{
    public class Singleton
    {
        private static Singleton instance;

        private Singleton() {}

        public static Singleton Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new Singleton();
                    instance.Workers = new Dictionary<string, string>();
                }

                return instance;
            }
        }

        public string WorkspaceSid { get; set; }

        public string WorkflowSid { get; set; }

        public string PostWorkActivitySid { get; set; }

        public string IdleActivitySid { get; set; }

        public string OfflineActivitySid { get; set; }

        public IDictionary<string, string> Workers { get; set; }
    }
}