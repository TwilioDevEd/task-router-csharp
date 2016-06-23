using System;
using System.Collections.Generic;
using System.Linq;
using TaskRouter.Web.Domain;
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

        public void Register()
        {
            var workspace = DeleteAndCreateWorkspace("Twilio Workspace", "https://sb.ngrok.io/events");
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
                Targets = new List<Target>() { new Target { Queue = voiceQueue.Sid } }
            };

            var smsFilter = new Filter()
            {
                FriendlyName = "SMS",
                Expression = "selected_product==\"ProgrammableSMS\"",
                Targets = new List<Target>() { new Target { Queue = smsQueue.Sid } }
            };

            var workflowConfiguration = new WorkflowConfiguration();

            workflowConfiguration.Filters.Add(voiceFilter);
            workflowConfiguration.Filters.Add(smsFilter);
            workflowConfiguration.DefaultFilter = new Target() { Queue = allQueue.Sid };

            // Convert to JSON
            var workflowJSON = "{\"task_routing\":" + workflowConfiguration.ToString() + "}";

            // Call REST API
            Workflow workflow = _client.AddWorkflow(
                workspaceSid,
                "Tech Support",
                workflowJSON,
                "https://sb.ngrok.io/assignment",
                "https://sb.ngrok.io/assignment",
                60);
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
            var attributesForBob =
                "{\"products\": [\"ProgrammableSMS\"], \"contact_uri\": \"+593992670240\"}";
            _client.AddWorker(workspaceSid, "Bob", null, attributesForBob);

            var attributesForAlice =
                "{\"products\": [\"ProgrammableVoice\"], \"contact_uri\": \"+593987908027\"}";
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