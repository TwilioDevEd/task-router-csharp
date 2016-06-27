using System.Collections.Generic;
using System.Linq;
using TaskRouter.Web.Infraestructure;
using Twilio.TaskRouter;

namespace TaskRouter.Web
{
    public class WorkspaceConfig
    {
        private readonly TaskRouterClient _client;
        private readonly string hostUrl = Config.HostUrl;

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
            var workspace = DeleteAndCreateWorkspace("Twilio Workspace", string.Format("{0}/call/events", hostUrl));
            var workspaceSid = workspace.Sid;

            CreateWorkers(workspaceSid);

            var reservationActivity = GetActivityByFriendlyName(workspaceSid, "Reserved");
            var assignmentActivity = GetActivityByFriendlyName(workspaceSid, "Busy");

            var voiceQueue = CreateTaskQueue(
                workspaceSid, "Voice",
                assignmentActivity.Sid, reservationActivity.Sid, "products HAS 'ProgrammableVoice'");

            var smsQueue = CreateTaskQueue(
                workspaceSid, "SMS",
                assignmentActivity.Sid, reservationActivity.Sid, "products HAS 'ProgrammableSMS'");

            var allQueue = CreateTaskQueue(
                workspaceSid, "All",
                assignmentActivity.Sid, reservationActivity.Sid, "1 == 1");

            // Workflow
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
            Workflow workflow = _client.AddWorkflow(
                workspaceSid,
                "Tech Support",
                workflowJSON,
                string.Format("{0}/call/assignment", hostUrl),
                string.Format("{0}/call/assignment", hostUrl),
                15);
            Singleton.Instance.WorkflowSid = workflow.Sid;

            var idle = GetActivityByFriendlyName(workspaceSid, "Idle");
            Singleton.Instance.PostWorkActivitySid = idle.Sid;
        }

        private Workspace DeleteAndCreateWorkspace(string friendlyName, string eventCallbackUrl) {
            var workspace = GetWorkspaceByFriendlyName(friendlyName);
            if (workspace != null)
            {
                _client.DeleteWorkspace(workspace.Sid);
            }

            return _client.AddWorkspace(friendlyName, eventCallbackUrl, null);
        }

        private void CreateWorkers(string workspaceSid)
        {
            var idle = GetActivityByFriendlyName(workspaceSid, "Idle");
            var attributesForBob =
                "{\"products\": [\"ProgrammableSMS\"], \"contact_uri\": \"" + Config.AgentForProgrammableSMS + "\"}";
            _client.AddWorker(workspaceSid, "Bob", idle.Sid, attributesForBob);

            var attributesForAlice =
                "{\"products\": [\"ProgrammableVoice\"], \"contact_uri\": \"" + Config.AgentForProgrammableVoice + "\"}";
            _client.AddWorker(workspaceSid, "Alice", idle.Sid, attributesForAlice);
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