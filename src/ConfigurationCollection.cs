using System;
using System.Collections.Specialized;

namespace Paychex.Api
{
    public abstract class ConfigurationCollection
    {
        private readonly NameValueCollection _settings;
        protected ConfigurationCollection(NameValueCollection settings) => _settings = settings;
        protected string GetSetting(string v)
        {
            var value = _settings[v];
            if (string.IsNullOrEmpty(value))
                throw new ArgumentNullException(v, $"No configuration value set for key {v}");
            return value;
        }
        protected T GetSetting<T>(string key, T defaultValue) where T : struct
        {
            var v = _settings[key];
            try
            {
                if (v == null) return defaultValue;
                return (T) Convert.ChangeType(v, typeof(T));
            }
            catch (Exception ex) when (ex is InvalidCastException || ex is FormatException)
            {
                return defaultValue;
            }
        }
    }
}