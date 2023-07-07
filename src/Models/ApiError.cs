using System;
using System.Text;

namespace Paychex.Api.Models
{
    /// <summary>
    /// Exception information returned by a Paychex call
    /// </summary>
    [Serializable]
    public class ApiError : PaychexError
    {
        /// <summary>
        /// default constructor
        /// </summary>
        public ApiError() { }

        /// <summary>
        /// Constructor, create from an IPaychexError interface object.
        /// </summary>
        /// <param name="e"></param>
        public ApiError(PaychexError e) : base(e.error, e.error_description) { }

        /// <summary>
        /// Create api error from exception
        /// </summary>
        /// <param name="e"></param>
        public ApiError(Exception e)
        {
            error = e.GetType()
                     .Name;
            _errorDescription = e.Message;
        }

        /// <summary>
        /// transaction id from headers. Used for correlation on the Paychex
        /// </summary>
        public string transactionId { get; set; }

        /// <summary>
        /// error code from the api
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// error description from the api
        /// </summary>
        public string description { get; set; }

        /// <summary>
        /// hint on how to resolve the error
        /// </summary>
        public string resolution { get; set; }

        /// <inheritdoc />
        public override string error { get; set; }

        private string _errorDescription;

        /// <inheritdoc />
        public override string error_description
        {
            get => !string.IsNullOrEmpty(_errorDescription) ? _errorDescription : description;
            set => _errorDescription = value;
        }

        /// <summary>
        /// ToString override - hack to put together various strings in possibly a sane resolution
        /// for display when a problem occurs
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder("Api Error: ");
            if (!string.IsNullOrEmpty(code))
                sb.Append(code);
            if (!string.IsNullOrEmpty(error_description))
                sb.AppendFormat("\t{0}", error_description);
            if (!string.IsNullOrEmpty(resolution))
                sb.AppendFormat("\nResolution: {0}", resolution);
            if (sb.Length == 0)
                sb.Append(error);
            
            sb.AppendFormat("\nPaychex TxId: {0}", transactionId);

            return sb.ToString();
        }
    }
}