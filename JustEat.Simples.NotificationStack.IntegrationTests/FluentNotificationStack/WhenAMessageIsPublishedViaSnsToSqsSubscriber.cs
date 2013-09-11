﻿using System.Threading;
using JustEat.Simples.NotificationStack.Messaging;
using JustEat.Simples.NotificationStack.Messaging.MessageHandling;
using JustEat.Simples.NotificationStack.Messaging.Messages.OrderDispatch;
using NSubstitute;
using NUnit.Framework;

namespace NotificationStack.IntegrationTests.FluentNotificationStack
{
    [TestFixture]
    public class WhenAMessageIsPublishedViaSnsToSqsSubscriber
    {
        private readonly IHandler<OrderAccepted> _handler = Substitute.For<IHandler<OrderAccepted>>();
        private JustEat.Simples.NotificationStack.Stack.FluentNotificationStack _publisher;

        [SetUp]
        public void Given()
        {
            _handler.Handle(Arg.Any<OrderAccepted>()).Returns(true);

            var publisher = JustEat.Simples.NotificationStack.Stack.FluentNotificationStack.Register(c =>
                                                                        {
                                                                            c.Component = "OrderEngine";
                                                                            c.Tenant = "uk";
                                                                            c.Environment = "integrationTest";
                                                                            c.PublishFailureBackoffMilliseconds = 1;
                                                                            c.PublishFailureReAttempts = 3;
                                                                        })
                .WithSnsMessagePublisher<OrderAccepted>(NotificationTopic.CustomerCommunication)
                .WithSqsTopicSubscriber(NotificationTopic.CustomerCommunication, 60)
                .WithMessageHandler(_handler);

            publisher.StartListening();
            _publisher = publisher;
        }

        [Test]
        public void ThenItGetsHandled()
        {
            _publisher.Publish(new OrderAccepted(1, 2, 3));
            Thread.Sleep(500);
            _handler.Received().Handle(Arg.Any<OrderAccepted>());
        }

        [TearDown]
        public void ByeBye()
        {
            _publisher.StopListening();
            _publisher = null;
        }
    }
}
