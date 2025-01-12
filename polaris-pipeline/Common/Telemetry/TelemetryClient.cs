using System.Collections.Generic;
using System.Linq;
using Common.Domain.Extensions;
using Common.Telemetry.Contracts;
using AppInsights = Microsoft.ApplicationInsights;

namespace Common.Telemetry
{
    public class TelemetryClient : ITelemetryClient
    {
        public const string telemetryVersion = nameof(telemetryVersion);

        public const string Version = "0.1";

        protected readonly AppInsights.TelemetryClient _telemetryClient;

        public TelemetryClient(AppInsights.TelemetryClient telemetryClient)
        {
            _telemetryClient = telemetryClient;
        }

        public void TrackEvent(BaseTelemetryEvent baseTelemetryEvent)
        {
            TrackEventInternal(baseTelemetryEvent, isFailure: false);
        }

        public void TrackEventFailure(BaseTelemetryEvent baseTelemetryEvent)
        {
            TrackEventInternal(baseTelemetryEvent, isFailure: true);
        }

        private void TrackEventInternal(BaseTelemetryEvent baseTelemetryEvent, bool isFailure)
        {
            if (baseTelemetryEvent == null)
            {
                // As this is telemetry just silently fail
                // todo: a better/more assertive approach
                return;
            }


            var (properties, metrics) = baseTelemetryEvent.ToTelemetryEventProps();

            // filter metrics for only entries where we have a value
            var nonNullMetrics = metrics
                .Where(kvp => kvp.Value.HasValue)
                .ToDictionary(kvp => kvp.Key, kvp => (double)kvp.Value);

            properties.Add(telemetryVersion, Version);
            if (isFailure)
            {
                properties.Add("isFailure", "true");
            }

            _telemetryClient.TrackEvent(
                PrepareEventName(baseTelemetryEvent.EventName),
                PrepareKeyNames(properties),
                PrepareKeyNames(nonNullMetrics)
            );
        }

        private static string PrepareEventName(string source)
        {
            if (!source.EndsWith("Event"))
                return source;

            return source.Remove(source.LastIndexOf("Event"));
        }

        private static IDictionary<string, T> PrepareKeyNames<T>(IDictionary<string, T> properties)
        {
            var cleanedProperties = new Dictionary<string, T>();

            foreach (var property in properties)
            {
                cleanedProperties.Add(CleanPropertyName(property.Key), property.Value);
            }

            return cleanedProperties;
        }

        private static string CleanPropertyName(string name)
        {
            return name
                // If the fields being captured are private and follow  _foo convention
                // then we need to remove the leading underscore
                .Replace("_", string.Empty)
                // If the fields being captured are public and follow Foo convention
                // then we need to lowercase the first character
                .ToLowerFirstChar();
        }
    }
}
