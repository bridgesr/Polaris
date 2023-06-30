using Common.Telemetry;
using Xunit;

namespace Common.tests.Telemetry
{

    public class BaseTelemetryEventTests
    {
        [Fact]
        public void GetDurationSeconds_ReturnsExpectedValue()
        {
            // Arrange
            var startTime = new DateTime(2020, 1, 1, 0, 0, 0, 100, DateTimeKind.Utc);
            var endTime = new DateTime(2020, 1, 1, 0, 0, 59, 200, DateTimeKind.Utc);

            // Act
            var duration = BaseTelemetryEvent.GetDurationSeconds(startTime, endTime);

            // Assert
            Assert.Equal(59.1, duration);
        }
    }
}