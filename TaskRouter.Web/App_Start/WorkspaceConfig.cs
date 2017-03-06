using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Helpers;
using TaskRouter.Web.Infrastructure;
using Twilio;
using Twilio.Rest.Taskrouter.V1;
using Twilio.Rest.Taskrouter.V1.Workspace;

namespace TaskRouter.Web
{
    public class WorkspaceConfig
    {
        private readonly string _hostUrl = Config.HostUrl;

        private const string VoiceQueue = "VoiceQueue";
        private const string SmsQueue = "SMSQueue";
        private const string AllQueue = "AllQueue";

        public static void RegisterWorkspace()
        {
            new WorkspaceConfig().Register();
        }

        public WorkspaceConfig()
        {
            if (Config.ENV != "test")
            {
                TwilioClient.Init(Config.AccountSID, Config.AuthToken);
            }
        }

        public WorkspaceConfig(Type workspaceResource):this()
        {
        }

        public virtual ActivityResource GetActivityByFriendlyName(string workspaceSid, string friendlyName)
        {
            return ActivityResource.Read(workspaceSid, friendlyName).First();
        }

        public virtual WorkspaceResource GetWorkspaceByFriendlyName(string friendlyName)
        {
            return WorkspaceResource.Read(friendlyName).FirstOrDefault();
        }

        public virtual WorkspaceResource CreateWorkspace(string friendlyName, Uri eventCallbackUrl)
        {
            return WorkspaceResource.Create(friendlyName, eventCallbackUrl);
        }

        public virtual bool DeleteWorkspace(string workspaceSid)
        {
            return WorkspaceResource.Delete(workspaceSid);
        }

        public virtual WorkerResource CreateWorker(string workspaceSid, string bob, string activitySid, string attributes)
        {
            return WorkerResource.Create(workspaceSid, bob, activitySid, attributes);
        }

        public void Register()
        {
            var workspace = DeleteAndCreateWorkspace("Twilio Workspace", $"{_hostUrl}/callback/events");
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

        public virtual WorkspaceResource DeleteAndCreateWorkspace(string friendlyName, string eventCallbackUrl) {
            var workspace = GetWorkspaceByFriendlyName(friendlyName);
            if (workspace != null)
            {
                DeleteWorkspace(workspace.Sid);
            }

            return CreateWorkspace(friendlyName, new Uri(eventCallbackUrl));
        }

        private IDictionary<string, string> CreateWorkers(string workspaceSid, ActivityResource activity)
        {
            var attributesForBob = new
            {
                products = new List<object>()
                {
                    "ProgrammableSMS"
                },
                contact_uri = Config.AgentForProgrammableSMS
            };

            var bobWorker = CreateWorker(workspaceSid, "Bob", activity.Sid, Json.Encode(attributesForBob));

            var attributesForAlice = new
            {
                products = new List<object>()
                {
                    "ProgrammableVoice"
                },
                contact_uri = Config.AgentForProgrammableVoice
            };

            var alice = CreateWorker(workspaceSid, "Alice", activity.Sid, Json.Encode(attributesForAlice));

            return new Dictionary<string, string>
            {
                { Config.AgentForProgrammableSMS, bobWorker.Sid },
                { Config.AgentForProgrammableVoice, alice.Sid },
            };
        }

        public virtual TaskQueueResource CreateTaskQueue(
            string workspaceSid, string friendlyName,
            string assignmentActivitySid, string reservationActivitySid, string targetWorkers)
        {
            var queue = TaskQueueResource.Create(
                workspaceSid, friendlyName, assignmentActivitySid, reservationActivitySid);

            TaskQueueResource.Update(
                workspaceSid,
                queue.Sid,
                friendlyName,
                targetWorkers,
                assignmentActivitySid,
                reservationActivitySid,
                1);

            return queue;
        }

        private IDictionary<string, TaskQueueResource> CreateTaskQueues(
            string workspaceSid, ActivityResource assignmentActivity, ActivityResource reservationActivity)
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

            return new Dictionary<string, TaskQueueResource> {
                { VoiceQueue, voiceQueue },
                { SmsQueue, smsQueue },
                { AllQueue, allQueue }
            };
        }

        public virtual WorkflowResource CreateWorkflow(string workspaceSid, IDictionary<string, TaskQueueResource> taskQueues)
        {
            var voiceQueue = taskQueues[VoiceQueue];
            var smsQueue = taskQueues[SmsQueue];
            var allQueue = taskQueues[AllQueue];

            var voiceFilter = new {
                friendlyName = "Voice",
                expression = "selected_product==\"ProgrammableVoice\"",
                targets = new List<object>() {
                    new { queue = voiceQueue.Sid, Priority = "5", Timeout = "30" },
                    new { queue = allQueue.Sid, Expression = "1==1", Priority = "1", Timeout = "30" }
                }
            };

            var smsFilter = new {
                friendlyName = "SMS",
                expression = "selected_product==\"ProgrammableSMS\"",
                targets = new List<object>() {
                    new { queue = smsQueue.Sid, Priority = "5", Timeout = "30" },
                    new { queue = allQueue.Sid, Expression = "1==1", Priority = "1", Timeout = "30" }
                }
            };

            var workflowConfiguration = new
            {
                task_routing = new
                {
                    filters = new List<object>()
                    {
                        voiceFilter,
                        smsFilter
                    },
                    default_filter = new
                    {
                        queue = allQueue.Sid,
                        expression = "1==1",
                        priority = "1",
                        timeout = "30"
                    }
                }
            };

            // Call REST API
            return WorkflowResource.Create(
                workspaceSid,
                "Tech Support",
                Json.Encode(workflowConfiguration),
                new Uri($"{_hostUrl}/callback/assignment"),
                new Uri($"{_hostUrl}/callback/assignment"),
                15);
        }
    }
}