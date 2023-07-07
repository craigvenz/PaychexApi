using System.Collections.Generic;
using System.Linq;
using Paychex.Api.Models.Common;

namespace Paychex.Api.Models
{
    /// <summary>
    /// Container class that wraps Paychex API responses. 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class ApiResponse<T> : BaseObject
    {
        /// <summary>
        /// Create empty response - mostly used for unit tests
        /// </summary>
        public ApiResponse()
        {
        }

        /// <summary>
        /// Create a response from an enumerable - fills in metadata class. Mostly for unit tests
        /// </summary>
        /// <param name="c"></param>
        public ApiResponse(IEnumerable<T> c)
        {
            content = c?.ToList() ?? new List<T>();
            metadata = new Metadata { ContentItemCount = content.Count };
        }

        /// <summary>
        /// Create a response from a list of errors.
        /// </summary>
        /// <param name="errors"></param>
        public ApiResponse(IEnumerable<ApiError> errors)
        {
            this.errors = errors.ToList();
        }

        /// <summary>
        /// Create a response from an ApiError
        /// </summary>
        /// <param name="ex"></param>
        public ApiResponse(ApiError ex)
        {
            errors.Add(ex);
        }

        /// <summary>
        /// Pagination and other information
        /// </summary> 
        public Metadata metadata { get; set; } = new Metadata { ContentItemCount = 0 };

        /// <summary>
        /// Data that was requested.
        /// </summary>
        public List<T> content { get; set; } = new List<T>();

        /// <summary>
        /// List of errors encountered, if any.
        /// </summary>
        public List<ApiError> errors { get; set; } = new List<ApiError>();

        /// <summary>
        /// Shortcut to tell if a call was successful.
        /// </summary>
        [JsonIgnore]
        public bool success => string.IsNullOrEmpty(error) && (!errors?.Any() ?? true);

        /// <summary>
        /// Transaction Id returned by each call for correlating problems with Paychex logs.
        /// </summary>
        public string paychexTransactionId { get; set; }

        /// <summary>
        /// Correlation Id is our internal value we generate and then should receive back to determine which
        /// transaction this result is an answer to, for example if we are making many calls at once. Not 
        /// really sure if this is any use to us since we are using async/await.
        /// </summary>
        public string correlationId { get; set; }

        private ApiError GetLastError()
        {
            var e = errors.LastOrDefault();
            if (e == null)
            {
                e = new ApiError();
                errors.Add(e);
            }
            return e;
        }

        /// <summary>
        /// 
        /// </summary>
        public string error
        {
            get => errors.LastOrDefault()?.error;
        }

        /// <summary>
        /// 
        /// </summary>
        public string error_description
        {
            get => errors.LastOrDefault()?.error_description;
        }

        /// <summary>
        /// Debug info
        /// </summary>
        /// <returns>debug info</returns>
        public override string ToString() => $"ApiResponse<{typeof(T).Name}> - {metadata}";
    }
}
