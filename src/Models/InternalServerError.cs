using System;

namespace Paychex.Api.Models
{
    /// <summary>
    /// A different model discovered during testing, if the server returns a 500.
    /// </summary>
    public class InternalServerError
    {
        /// <summary>
        /// Error occurred at this time.
        /// </summary>
        public DateTime Timestamp {get;set;}
        /// <summary>
        /// Url of call.
        /// </summary>
        public string Path {get;set;}
        /// <summary>
        /// Status code.
        /// </summary>
        public int Status {get;set;}
        /// <summary>
        /// Error description.
        /// </summary>
        public string Error {get;set;}
    }
}
