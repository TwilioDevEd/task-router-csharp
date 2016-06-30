using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using TaskRouter.Web.Infrastructure;
using Twilio.TaskRouter;

namespace TaskRouter.Web.Tests.App_Start
{
    public class WorkspaceConfigTest
    {
        private WorkspaceConfig _workspaceConfig;
        private Mock<TaskRouterClient> _mockClient;

        [SetUp]
        public void SetUp()
        {
            _mockClient = new Mock<TaskRouterClient>(string.Empty, string.Empty);
            _workspaceConfig = new WorkspaceConfig(_mockClient.Object);

            var workspaceResult = new WorkspaceResult();
            workspaceResult.Workspaces = new List<Workspace>();
            workspaceResult.Workspaces.Add(new Workspace { FriendlyName = "Twilio Workspace" });
            _mockClient.Setup(c => c.ListWorkspaces()).Returns(workspaceResult);

            var activityResult = new ActivityResult();
            activityResult.Activities = new List<Activity>();
            activityResult.Activities.Add(new Activity { Sid = "WAXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", FriendlyName = "Reserved" });
            activityResult.Activities.Add(new Activity { Sid = "WAXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", FriendlyName = "Busy" });
            activityResult.Activities.Add(new Activity { Sid = "WAXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", FriendlyName = "Idle" });
            activityResult.Activities.Add(new Activity { Sid = "WAXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX", FriendlyName = "Offline" });
            _mockClient.Setup(c => c.ListActivities(It.IsAny<string>())).Returns(activityResult);


            var workspace = new Workspace { Sid = "WSXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" };
            _mockClient
                .Setup(c => c.AddWorkspace(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(workspace);

            _mockClient
                .Setup(c => c.AddWorker(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .Returns(new Worker { Sid = "WKXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" });

            _mockClient
                .Setup(c => c.AddTaskQueue(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null))
                .Returns(new TaskQueue { Sid = "WQXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" });

            _mockClient
                .Setup(c => c.AddWorkflow(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>()))
                .Returns(new Workflow { Sid = "WWXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX" });

            _workspaceConfig.Register();
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
