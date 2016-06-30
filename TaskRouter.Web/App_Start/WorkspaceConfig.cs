using System.Collections.Generic;
using System.Linq;
using TaskRouter.Web.Infrastructure;
using Twilio.TaskRouter;

namespace TaskRouter.Web
{
    public class WorkspaceConfig
    {
        private readonly TaskRouterClient _client;
        private readonly string hostUrl = Config.HostUrl;

        private const string VoiceQueue = "VoiceQueue";
        private const string SMSQueue = "SMSQueue";
        private const string AllQueue = "AllQueue";

        public static void RegisterWorkspace()
        {
            new WorkspaceConfig().Register();
        }

        public WorkspaceConfig()
        {
            _client = new TaskRouterClient(Config.AccountSID, Config.AuthToken);
        }

        public WorkspaceConfig(TaskRouterClient client)
        {
            _client = client;
        }

        public void Register()
        {
            var workspace = DeleteAndCreateWorkspace("Twilio Workspace", string.Format("{0}/callback/events", hostUrl));
            var workspaceSid = workspace.Sid;

            var assignmentActivity = GetActivityByFriendlyName(workspaceSid, "Busy");
            var reservationActivity = GetActivityByFriendlyName(workspaceSid, "Reserved");
            var idleActivity = GetActivityByFriendlyName(workspaceSid, "Idle");
            var offlineActivity = GetActivityByFriendlyName(workspaceSid, "Offline");

            var workers = CreateWorkers(workspaceSid, idleActivity);
            var taskQueues = CreateTaskQueues(workspaceSid, assignmentActivity, reservationActivity);
            var workflow = CreateWorkflow(workspaceSid, taskQueues);

            Singleton.Instance.WorkspaceSid = workspaceSid;
            Singleton.Instance.WorkflowSid = workflow.Sid;
            Singleton.Instance.Workers = workers;
            Singleton.Instance.PostWorkActivitySid = idleActivity.Sid;
            Singleton.Instance.IdleActivitySid = idleActivity.Sid;
            Singleton.Instance.OfflineActivitySid = offlineActivity.Sid;
        }

        private Workspace DeleteAndCreateWorkspace(string friendlyName, string eventCallbackUrl) {
            var workspace = GetWorkspaceByFriendlyName(friendlyName);
            if (workspace != null)
            {
                _client.DeleteWorkspace(workspace.Sid);
            }

            return _client.AddWorkspace(friendlyName, eventCallbackUrl, null);
        }

        private IDictionary<string, string> CreateWorkers(string workspaceSid, Activity activity)
        {
            var attributesForBob =
                "{\"products\": [\"ProgrammableSMS\"], \"contact_uri\": \"" + Config.AgentForProgrammableSMS + "\"}";
            var bob = _client.AddWorker(workspaceSid, "Bob", activity.Sid, attributesForBob);

            var attributesForAlice =
                "{\"products\": [\"ProgrammableVoice\"], \"contact_uri\": \"" + Config.AgentForProgrammableVoice + "\"}";
            var alice = _client.AddWorker(workspaceSid, "Alice", activity.Sid, attributesForAlice);

            return new Dictionary<string, string>
            {
                { Config.AgentForProgrammableSMS, bob.Sid },
                { Config.AgentForProgrammableVoice, alice.Sid },
            };
        }

        private TaskQueue CreateTaskQueue(
            string workspaceSid, string friendlyName,
            string assignmentActivitySid, string reservationActivitySid, string targetWorkers)
        {
            var queue = _client.AddTaskQueue(
                workspaceSid, friendlyName, assignmentActivitySid, reservationActivitySid, string.Empty, null);
            _client.UpdateTaskQueue(
                workspaceSid,
                queue.Sid,
                friendlyName,
                assignmentActivitySid,
                reservationActivitySid,
                targetWorkers, 1);

            return queue;
        }

        private IDictionary<string, TaskQueue> CreateTaskQueues(
            string workspaceSid, Activity assignmentActivity, Activity reservationActivity)
        {

            var voiceQueue = CreateTaskQueue(
                workspaceSid, "Voice",
                assignmentActivity.Sid, reservationActivity.Sid, "products HAS 'ProgrammableVoice'");

            var smsQueue = CreateTaskQueue(
                workspaceSid, "SMS",
                assignmentActivity.Sid, reservationActivity.Sid, "products HAS 'ProgrammableSMS'");

            var allQueue = CreateTaskQueue(
                workspaceSid, "All",
                assignmentActivity.Sid, reservationActivity.Sid, "1 == 1");

            return new Dictionary<string, TaskQueue> {
                { VoiceQueue, voiceQueue },
                { SMSQueue, smsQueue },
                { AllQueue, allQueue }
            };
        }

        private Workflow CreateWorkflow(string workspaceSid, IDictionary<string, TaskQueue> taskQueues)
        {
            var voiceQueue = taskQueues[VoiceQueue];
            var smsQueue = taskQueues[SMSQueue];
            var allQueue = taskQueues[AllQueue];

            var voiceFilter = new Filter()
            {
                FriendlyName = "Voice",
                Expression = "selected_product==\"ProgrammableVoice\"",
                Targets = new List<Target>() {
                    new Target { Queue = voiceQueue.Sid, Priority = "5", Timeout = "30" },
                    new Target { Queue = allQueue.Sid, Expression = "1==1", Priority = "1", Timeout = "30" }
                }
            };

            var smsFilter = new Filter()
            {
                FriendlyName = "SMS",
                Expression = "selected_product==\"ProgrammableSMS\"",
                Targets = new List<Target>() {
                    new Target { Queue = smsQueue.Sid, Priority = "5", Timeout = "30" },
                    new Target { Queue = allQueue.Sid, Expression = "1==1", Priority = "1", Timeout = "30" }
                }
            };

            var workflowConfiguration = new WorkflowConfiguration();

            workflowConfiguration.Filters.Add(voiceFilter);
            workflowConfiguration.Filters.Add(smsFilter);
            workflowConfiguration.DefaultFilter = new Target() {
                Queue = allQueue.Sid, Expression = "1==1", Priority = "1", Timeout = "30"
            };

            // Convert to JSON
            var workflowJSON = "{\"task_routing\":" + workflowConfiguration.ToString() + "}";

            // Call REST API
            return _client.AddWorkflow(
                workspaceSid,
                "Tech Support",
                workflowJSON,
                string.Format("{0}/callback/assignment", hostUrl),
                string.Format("{0}/callback/assignment", hostUrl),
                15);
        }

        private Activity GetActivityByFriendlyName(string workspaceSid, string friendlyName)
        {
            var activityResult = _client.ListActivities(workspaceSid);
            return activityResult.Activities.First(a => a.FriendlyName == friendlyName);
        }

        private Workspace GetWorkspaceByFriendlyName(string friendlyName)
        {
            var workspaceResult = _client.ListWorkspaces();
            return workspaceResult.Workspaces.FirstOrDefault(w => w.FriendlyName == friendlyName);
        }
    }
}