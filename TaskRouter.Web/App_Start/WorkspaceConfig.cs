using System.Collections.Generic;
using System.Linq;
using TaskRouter.Web.Infraestructure;
using Twilio.TaskRouter;

namespace TaskRouter.Web
{
    public class WorkspaceConfig
    {
        private readonly TaskRouterClient _client;

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
            var workspace = DeleteAndCreateWorkspace("Twilio Workspace", "https://sb.ngrok.io/call/events");
            var workspaceSid = workspace.Sid;

            CreateWorkers(workspaceSid);

            var reservationActivity = GetActivityByFriendlyName(workspaceSid, "Reserved");
            var assignmentActivity = GetActivityByFriendlyName(workspaceSid, "Busy");

            var voiceQueue = CreateTaskQueue(
                workspaceSid, "Voice",
                reservationActivity.Sid, assignmentActivity.Sid, "products HAS 'ProgrammableVoice'");

            var smsQueue = CreateTaskQueue(
                workspaceSid, "SMS",
                reservationActivity.Sid, assignmentActivity.Sid, "products HAS 'ProgrammableSMS'");

            var allQueue = CreateTaskQueue(
                workspaceSid, "All",
                reservationActivity.Sid, assignmentActivity.Sid, "1 == 1");

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
                "https://sb.ngrok.io/call/assignment",
                "https://sb.ngrok.io/call/assignment",
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

            workspace = _client.AddWorkspace(friendlyName, eventCallbackUrl, null);

            var idle = GetActivityByFriendlyName(workspace.Sid, "Idle");
            return _client.UpdateWorkspace(workspace.Sid, friendlyName, eventCallbackUrl, idle.Sid, null);
        }

        private void CreateWorkers(string workspaceSid)
        {
            var attributesForBob =
                "{\"products\": [\"ProgrammableSMS\"], \"contact_uri\": \"+593992670240\"}";
            _client.AddWorker(workspaceSid, "Bob", null, attributesForBob);

            var attributesForAlice =
                "{\"products\": [\"ProgrammableVoice\"], \"contact_uri\": \"+593999031619\"}";
            _client.AddWorker(workspaceSid, "Alice", null, attributesForAlice);
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