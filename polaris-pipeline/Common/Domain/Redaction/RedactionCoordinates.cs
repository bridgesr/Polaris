﻿using Newtonsoft.Json;

namespace Common.Domain.Redaction
{
    public class RedactionCoordinates
    {
        [JsonProperty("x1")]
        public double X1 { get; set; }

        [JsonProperty("y1")]
        public double Y1 { get; set; }

        [JsonProperty("x2")]
        public double X2 { get; set; }

        [JsonProperty("y2")]
        public double Y2 { get; set; }
    }
}
