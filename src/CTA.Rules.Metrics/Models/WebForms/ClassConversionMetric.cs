﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CTA.Rules.Metrics.Models.WebForms
{
    public class ClassConversionMetric : WebFormsActionMetric
    {
        [JsonProperty("actionName", Order = 11)]
        public string ActionName => "ClassConversion";

        public ClassConversionMetric(MetricsContext context, string childActionName, string projectPath)
            : base(context, childActionName, projectPath)
        {
        }
    }
}
