using System;

namespace Uragano.Abstractions
{
    public class EnvironmentVariableReader
    {
        public static T Get<T>(string variable, T defaultValue = default)
        {
            var value = Environment.GetEnvironmentVariable(variable);
            if (string.IsNullOrWhiteSpace(value))
                return defaultValue;
            return (T)Convert.ChangeType(value, typeof(T));
        }
    }
}
