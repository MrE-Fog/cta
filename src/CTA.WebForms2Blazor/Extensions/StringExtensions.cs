﻿namespace CTA.WebForms2Blazor.Extensions
{
    public static class StringExtensions
    {
        public static string RemoveOuterQuotes(this string input)
        {
            if (input.Length > 1 && ((input.StartsWith("\"") && input.EndsWith("\"")) || (input.StartsWith("'") && input.EndsWith("'"))))
            {
                return input.Substring(1, input.Length - 2);
            }

            return input;
        }
    }
}
