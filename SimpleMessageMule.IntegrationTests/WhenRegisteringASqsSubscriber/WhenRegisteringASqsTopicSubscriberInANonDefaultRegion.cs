﻿using Amazon;
using Amazon.SimpleNotificationService.Model;
using JustEat.Simples.NotificationStack.Messaging.MessageHandling;
using JustEat.Simples.NotificationStack.Messaging.Messages;
using JustEat.Testing;
using NSubstitute;
using NUnit.Framework;
using SimpleMessageMule;

namespace NotificationStack.IntegrationTests.WhenRegisteringASqsSubscriber
{
    public class WhenRegisteringASqsTopicSubscriberInANonDefaultRegion : FluentNotificationStackTestBase
    {
        private string _topicName;
        private RegionEndpoint _regionEndpoint;

        protected override void Given()
        {
            _topicName = "IntegrationTest";
            _regionEndpoint = RegionEndpoint.SAEast1;

            MockNotidicationStack();

            Configuration = new MessagingConfig
            {
                Region = _regionEndpoint.SystemName
            };

            DeleteQueueIfItAlreadyExists(_regionEndpoint, _topicName);
            DeleteTopicIfItAlreadyExists(_regionEndpoint, _topicName);
        }

        protected override void When()
        {
            SystemUnderTest.WithSqsTopicSubscriber(_topicName, 60)
                .WithMessageHandler(Substitute.For<IHandler<Message>>());
        }

        [Then]
        public void QueueAndTopicAreCreatedAndQueueIsSubscribedToTheTopicWithCorrectPermissions()
        {
            //This is a bad test as we're testing 4 things in 1 test, oh well.

            Topic topic;
            Assert.IsTrue(TryGetTopic(_regionEndpoint, _topicName, out topic), "Topic does not exist");

            string queueUrl;
            Assert.IsTrue(WaitForQueueToExist(_regionEndpoint, _topicName, out queueUrl), "Queue does not exist");

            Assert.IsTrue(IsQueueSubscribedToTopic(_regionEndpoint, topic, queueUrl), "Queue is not subscribed to the topic");

            Assert.IsTrue(QueueHasPolicyForTopic(_regionEndpoint, topic, queueUrl), "Queue does not have a policy for the topic");

        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            DeleteQueueIfItAlreadyExists(_regionEndpoint, _topicName);
            DeleteTopicIfItAlreadyExists(_regionEndpoint, _topicName);
        }
    }
}