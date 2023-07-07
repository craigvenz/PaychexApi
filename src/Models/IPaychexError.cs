using System;

namespace Paychex.Api.Models
{
    /// <summary>
    /// Not completely sure why I decided I needed this. Possibly for the ApiResponse success property.
    /// </summary>
    [Serializable]
    public class PaychexError
    {
        /// <summary>
        /// 
        /// </summary>
        public PaychexError() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="error"></param>
        /// <param name="description"></param>
        public PaychexError(string error, string description)
        {
            this.error = error;
            error_description = description;
        }

        /// <summary>
        /// error name
        /// </summary>
        public virtual string error { get; set; }

        /// <summary>
        /// error description
        /// </summary>
        public virtual string error_description { get; set; }
    }
}
