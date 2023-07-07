using System.IO;
using Paychex.Api.Api.Interfaces;
using Paychex.Api.Models.Authentication;

namespace Paychex.Api.Api
{
    /// <inheritdoc />
    /// <summary>
    ///     Quick and dirty cache implementation - just writes a file to disk. Don't use for web app usage.
    /// </summary>
    public class FileCache : IPaychexTokenCache
    {
        private readonly string _filename;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filename">Name of file to save the auth token to</param>
        public FileCache(string filename) => _filename = filename;

        /// <inheritdoc />
        public void Save(PaychexAuthToken token)
        {
            var serializer = JsonSerializer.Create();
            using (var jw = new JsonTextWriter(new StreamWriter(File.OpenWrite(_filename))))
            {
                serializer.Serialize(jw, token);
            }
        }

        /// <inheritdoc />
        public PaychexAuthToken Load()
        {
            if (!File.Exists(_filename))
                return null;

            var serializer = JsonSerializer.Create();
            using (var jr = new JsonTextReader(new StreamReader(File.OpenRead(_filename))))
            {
                return serializer.Deserialize<PaychexAuthToken>(jr);
            }
        }

        /// <inheritdoc />
        public void Invalidate()
        {
            if (File.Exists(_filename))
                File.Delete(_filename);
        }
    }
}
