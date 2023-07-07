using System;
using System.Collections.Generic;
using System.IO;
using System.Security;
using Paychex.Api.Api.Interfaces;

/// <summary>
///This is a debugging tool and is added to the client by dependency injection
///and thus can be replaced with another class
/// </summary>

namespace Paychex.Api.Api
{
    /// <inheritdoc />
    public class FileSystemDataCache : IPaychexDataCache
    {
        private readonly string _directory;
        private readonly JsonSerializer _jsonSerializer;

        /// <summary>
        /// Caching system for debugging. Saves responses to file based on request url. 
        /// </summary>
        /// <param name="directory">Folder location of files</param>
        public FileSystemDataCache(string directory)
        {
            _directory = directory;
            _jsonSerializer = JsonSerializer.Create(
                new JsonSerializerSettings()
                {
                    Formatting = Formatting.Indented,
                    MissingMemberHandling = MissingMemberHandling.Error,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    Converters = new List<JsonConverter>
                    {
                        new TolerantEnumConverter()
                    }
                }
            );
        }

        /// <inheritdoc />
        public T Get<T>(string r)
        {
            if (IgnoreCacheReads || !File.Exists(GetFile(r)))
                return default(T);

            var serializer = _jsonSerializer;
            using (var jr = new JsonTextReader(new StreamReader(File.OpenRead(GetFile(r)))))
            {
                return serializer.Deserialize<T>(jr);
            }
        }

        /// <inheritdoc />
        public void Set<T>(string r, T value)
        {
            using (var jw = new JsonTextWriter(new StreamWriter(File.Create(GetFile(r)))))
            {
                _jsonSerializer.Serialize(jw, value);
            }
        }

        /// <inheritdoc />
        public void Clear()
        {
            foreach (var f in new DirectoryInfo(_directory).EnumerateFileSystemInfos("*.json"))
            {
                try
                {
                    f.Delete();
                }
                catch (Exception ex) when (ex is IOException
                                           || ex is SecurityException
                                           || ex is UnauthorizedAccessException)
                {
                    // Do nothing, don't really care, we tried to delete it. ¯\_(ツ)_/¯
                }
            }
        }

        /// <inheritdoc />
        public bool IgnoreCacheReads { get; set; } = false;

        private string GetFile(string key) => $"{_directory}\\{key}.json";
    }
}
