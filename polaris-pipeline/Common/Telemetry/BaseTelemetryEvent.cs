using System;
using System.Collections.Generic;

namespace Common.Telemetry
{
    public abstract class BaseTelemetryEvent
    {
        protected const string durationSeconds = nameof(durationSeconds);

        public string EventName
        {
            get
            {
                return this.GetType().Name;
            }
        }
        abstract public (IDictionary<string, string>, IDictionary<string, double>) ToTelemetryEventProps();

        public static double GetDurationSeconds(DateTime startTime, DateTime endTime)
        {
            return (double)(endTime - startTime).TotalSeconds;
        }
    }
}