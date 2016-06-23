namespace TaskRouter.Web.Domain
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
                }

                return instance;
            }
        }

        public string WorkflowSid { get; set; }

        public string PostWorkActivitySid { get; set; }
    }
}