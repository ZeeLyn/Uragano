using System;
using System.Text.RegularExpressions;

namespace Uragano.Abstractions
{
    public class EnvironmentVariableReader
    {
        public static T Get<T>(string variable, T defaultValue = default)
        {
            var value = Environment.GetEnvironmentVariable(variable);
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;

            value = value.ReplaceIpPlaceholder();

            var matches = Regex.Matches(value, "[{](.*?)[}]");
            if (matches.Count > 0)
            {
                foreach (var match in matches)
                {
                    value = value.Replace(match.ToString(), Environment.GetEnvironmentVariable(match.ToString().TrimStart('{').TrimEnd('}')));
                }
            }
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
