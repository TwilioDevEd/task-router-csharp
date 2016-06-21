using System;
using System.Collections.Generic;
using System.Linq;
using Twilio.TaskRouter;

namespace TaskRouter.Web
{
    public class WorkspaceConfig
    {
        public static void RegisterWorkspace()
        {
            var client = new TaskRouterClient("accountSid", "authToken");

            // Workspace
            var workspace = client.AddWorkspace("Twilio Workspace", "https://sb.ngrok.io/events", null);
            var workspaceSid = workspace.Sid;

            var attributesForBob = new Dictionary<string, string>();
            attributesForBob.Add("products", "[ProgrammableSMS]");
            attributesForBob.Add("contact_uri", "+593992670240");
            client.AddWorker(workspaceSid, "Bob", null, attributesForBob);

            var attributesForAlice = new Dictionary<string, string>();
            attributesForAlice.Add("products", "[ProgrammableVoice]");
            attributesForAlice.Add("contact_uri", "+593987908027");
            client.AddWorker(workspaceSid, "Alice", null, attributesForAlice);


            // Task queues
            var voiceQueue = client.AddTaskQueue(workspaceSid, "Voice", null, null, "products HAS 'ProgrammableVoice'", null);
            var smsQueue = client.AddTaskQueue(workspaceSid, "SMS", null, null, "products HAS 'ProgrammableSMS'", null);
            var allQueue = client.AddTaskQueue(workspaceSid, "SMS", null, null, "1==1", null);

            // Workflow
            var voiceFilter = new Filter()
            {
                FriendlyName = "Voice",
                Expression = "selected_product == 'ProgrammableVoice'",
                Targets = new List<Target>()
                {
                    new Target
                    {
                        Queue = voiceQueue.Sid
                    }
                }
            };

            var smsFilter = new Filter()
            {
                FriendlyName = "SMS",
                Expression = "selected_product == 'ProgrammableSMS'",
                Targets = new List<Target>()
                {
                    new Target
                    {
                        Queue = smsQueue.Sid
                    }
                }
            };

            var workflowConfiguration = new WorkflowConfiguration();

            workflowConfiguration.Filters.Add(voiceFilter);
            workflowConfiguration.Filters.Add(smsFilter);
            workflowConfiguration.DefaultFilter = new Target() { Queue = allQueue.Sid };

            // Convert to json
            var workflowJSON = workflowConfiguration.ToString();

            // Call rest api
            Workflow workflow = client.AddWorkflow(
                workspaceSid, "Tech Support", workflowJSON, "https://sb.ngrok.io/assignment", null, 60);

            Console.WriteLine(workflow.FriendlyName);

            var activityResult = client.ListActivities(workspace.Sid);
            var activity = activityResult.Activities.First(a => a.FriendlyName == "Idle");
        }
    }
}