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
            var workspace = GetWorkspaceByFriendlyName("Twilio Workspace");
            if (workspace != null)
            {
                _client.DeleteWorkspace(workspace.Sid);
            }

            workspace = _client.AddWorkspace("Twilio Workspace", "https://sb.ngrok.io/events", null);
            var workspaceSid = workspace.Sid;

            var attributesForBob = "{\"products\": [\"ProgrammableSMS\"], \"contact_uri\": \"+593992670240\"}";
            var bob = _client.AddWorker(workspaceSid, "Bob", null, attributesForBob);

            var attributesForAlice = "{\"products\": [\"ProgrammableVoice\"], \"contact_uri\": \"+593987908027\"}";
            var alice = _client.AddWorker(workspaceSid, "Alice", null, attributesForAlice);

            var reservationActivity = GetActivityByFriendlyName(workspaceSid, "Reserved");
            var assignmentActivity = GetActivityByFriendlyName(workspaceSid, "Busy");

            var voiceQueue = _client.AddTaskQueue(
                workspaceSid,
                "Voice",
                assignmentActivity.Sid,
                reservationActivity.Sid,
                "products HAS \"ProgrammableVoice\"", null);

            var smsQueue = _client.AddTaskQueue(workspaceSid,
                "SMS",
                assignmentActivity.Sid,
                reservationActivity.Sid,
                "products HAS \"ProgrammableSMS\"", null);

            var allQueue = _client.AddTaskQueue(
                workspaceSid,
                "All",
                assignmentActivity.Sid,
                reservationActivity.Sid,
                "1==1", null);

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

            // Convert to json
            var workflowJSON = workflowConfiguration.ToString();
            workflowJSON = "{\"task_routing\":" + workflowJSON + "}";

            // Call rest api
            Workflow workflow = _client.AddWorkflow(
                workspaceSid,
                "Tech Support",
                workflowJSON,
                "https://sb.ngrok.io/assignment",
                "https://sb.ngrok.io/assignment",
                60);

            Console.WriteLine(workflow.FriendlyName);
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