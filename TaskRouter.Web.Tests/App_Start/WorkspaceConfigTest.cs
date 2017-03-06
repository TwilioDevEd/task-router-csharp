using System;
using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using System.Web.Helpers;
using TaskRouter.Web.Infrastructure;
using Twilio.Rest.Taskrouter.V1;
using Twilio.Rest.Taskrouter.V1.Workspace;

namespace TaskRouter.Web.Tests.App_Start
{
    public class WorkspaceConfigTest
    {
        private WorkspaceResource workspaceFromObject(object obj)
        {
            return WorkspaceResource.FromJson(Json.Encode(obj));
        }

        private ActivityResource activityFromObject(object obj)
        {
            return ActivityResource.FromJson(Json.Encode(obj));
        }

        private WorkerResource workerFromObject(object obj)
        {
            return WorkerResource.FromJson(Json.Encode(obj));
        }

        private TaskQueueResource taskQueueFromObject(object obj)
        {
            return TaskQueueResource.FromJson(Json.Encode(obj));
        }

        private WorkflowResource workflowFromObject(object obj)
        {
            return WorkflowResource.FromJson(Json.Encode(obj));
        }

        [SetUp]
        public void SetUp()
        {
            var _workspaceConfig = new Mock<WorkspaceConfig> () { CallBase = true };

            _workspaceConfig
                .Setup(w => w.GetActivityByFriendlyName(It.IsAny<string>(), It.IsAny<string>()))
                .Returns((string wsSid, string friendlyName) => activityFromObject(
                    new {
                            Sid = "WAXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                            friendlyName
                    })
                );

            _workspaceConfig
                .Setup(w => w.GetWorkspaceByFriendlyName(It.IsAny<string>()))
                .Returns((string friendlyName) => workspaceFromObject(
                    new
                    {
                        Sid = "WAXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
                        friendlyName
                    })
                );

           _workspaceConfig 
                .Setup(w => w.CreateWorkspace(It.IsAny<string>(), It.IsAny<Uri>()))
                .Returns(workspaceFromObject(
                    new { Sid = "WSXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" }));

           _workspaceConfig 
                .Setup(w => w.CreateWorker(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(workerFromObject(
                    new { Sid = "WKXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" }));

            _workspaceConfig            
                .Setup(w => w.CreateTaskQueue(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(taskQueueFromObject(
                    new { Sid = "WQXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" }));

            _workspaceConfig
                .Setup(w => w.CreateWorkflow(
                    It.IsAny<string>(),
                    It.IsAny<IDictionary<string,TaskQueueResource>>()))
                .Returns(workflowFromObject(
                    new { Sid = "WWXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" }));

            _workspaceConfig
                .Setup(w => w.DeleteAndCreateWorkspace(
                    It.IsAny<string>(),
                    It.IsAny<string>()))
                .Returns(workspaceFromObject(
                    new { Sid = "WWXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" }));

            _workspaceConfig.Object.Register();
        }

        [Test]
        public void Register_CreatesAWorkflowSid()
        {
            Assert.That(Singleton.Instance.WorkflowSid, Is.EqualTo("WWXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"));
        }

        [Test]
        public void Register_CreatesAPostWorkActivitySid()
        {
            Assert.That(Singleton.Instance.PostWorkActivitySid, Is.EqualTo("WAXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX"));
        }
    }
}
