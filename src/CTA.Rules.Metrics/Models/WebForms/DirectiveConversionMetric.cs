﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CTA.Rules.Metrics.Models.WebForms
{
    public class DirectiveConversionMetric : WebFormsActionMetric
    {
        [JsonProperty("actionName", Order = 11)]
        public string ActionName => "DirectiveConversion";

        public DirectiveConversionMetric(MetricsContext context, string childActionName, string projectPath)
            : base(context, childActionName, projectPath)
        {
        }
    }
}
