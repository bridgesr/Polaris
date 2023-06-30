using System.Collections.Concurrent;
using AI = Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Xunit;
using Common.Telemetry;
using Moq;
using FluentAssertions;
using Microsoft.ApplicationInsights.DataContracts;

namespace Common.tests.Telemetry
{

    public class TelemetryClientTests
    {
        TelemetryClient _telemetryClient;
        Mock<BaseTelemetryEvent> _mockEvent;

        MockTelemetryChannel _mockTelemetryChannel;
        Dictionary<string, string> _mockEventProperties = new Dictionary<string, string>();
        Dictionary<string, double> _mockEventMetrics = new Dictionary<string, double>();
        public TelemetryClientTests()
        {
            _mockTelemetryChannel = new MockTelemetryChannel();

            var aiTelemetryConfig = new TelemetryConfiguration
            {
                TelemetryChannel = _mockTelemetryChannel,
                InstrumentationKey = Guid.NewGuid().ToString(),
            };
            var aiTelemetryClient = new AI.TelemetryClient(aiTelemetryConfig);

            _mockEvent = new Mock<BaseTelemetryEvent>();
            _mockEvent.Setup(x => x.ToTelemetryEventProps()).Returns((_mockEventProperties, _mockEventMetrics));

            _telemetryClient = new TelemetryClient(aiTelemetryClient);
        }

        [Fact]
        public void TelemetryClient_TrackEvent_RegistersVersion()
        {
            // Act
            _telemetryClient.TrackEvent(_mockEvent.Object);

            // Assert
            var receivedEvent = _mockTelemetryChannel.SentTelemetries.First() as EventTelemetry;
            receivedEvent?.Properties[TelemetryClient.telemetryVersion].Should().Be(TelemetryClient.Version);
        }

        [Fact]
        public void TelemetryClient_TrackEvent_RegistersPropertiesAndMetricsUsingCleanedPropertyNames()
        {
            // Arrange
            _mockEventProperties["_testFoo"] = "foo";
            _mockEventProperties["_testBar"] = "bar";
            _mockEventMetrics["_testBaz"] = 1.0;
            _mockEventMetrics["_testQux"] = 2.991919;
            // Act
            _telemetryClient.TrackEvent(_mockEvent.Object);

            // Assert
            var receivedEvent = _mockTelemetryChannel.SentTelemetries.First() as EventTelemetry;
            receivedEvent?.Properties["testFoo"].Should().Be("foo");
            receivedEvent?.Properties["testBar"].Should().Be("bar");
            receivedEvent?.Metrics["testBaz"].Should().Be(1.0);
            receivedEvent?.Metrics["testQux"].Should().Be(2.991919);
        }

        [Fact]
        public void TelemetryClient_TrackEvent_RegistersACleanedEventName
        ()
        {
            // Act
            _telemetryClient.TrackEvent(new AnEventWithANameEvent());

            // Assert
            var receivedEvent = _mockTelemetryChannel.SentTelemetries.First() as EventTelemetry;
            receivedEvent?.Name.Should().Be("AnEventWithAName");
        }
    }

    public class MockTelemetryChannel : ITelemetryChannel
    {
        public ConcurrentBag<ITelemetry> SentTelemetries = new ConcurrentBag<ITelemetry>();
        public bool IsFlushed { get; private set; }
        public bool? DeveloperMode { get; set; }
        public string EndpointAddress { get; set; }

        public void Send(ITelemetry item)
        {
            this.SentTelemetries.Add(item);
        }

        public void Flush()
        {
            this.IsFlushed = true;
        }

        public void Dispose()
        {

        }
    }

    public class AnEventWithANameEvent : BaseTelemetryEvent
    {
        public override (IDictionary<string, string>, IDictionary<string, double>) ToTelemetryEventProps()
        {
            return (new Dictionary<string, string>(), new Dictionary<string, double>());
        }
    }


}