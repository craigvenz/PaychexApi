using Paychex.Api.Api;
using Paychex.Api.Models.Common;

namespace Paychex.Api.Models.Workers
{
    /// <summary>
    /// The representational state of the workers communications
    /// </summary>
    public class Communication : Common.Communication
    {
        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public Communication() { }

        /// <summary>
        /// Copy from an existing common communication.
        /// </summary>
        /// <param name="other">Other communication object</param>
        public Communication(Common.Communication other)
        {
            CopyFrom(other);
        }

        /// <summary>
        /// Worker this communication information belongs to
        /// </summary>
        public string WorkerId { get; set; }
        /// <summary>
        /// The mailto address as specified in RFC2368.
        /// </summary>
        public string Uri { get; set; }
        /// <summary>
        /// The area dialing code for a communication number.
        /// </summary>
        public string DialArea { get; set; }
        /// <summary>
        /// The communication number, not including country dialing or area dialing codes.
        /// </summary>
        public string DialNumber { get; set; }
        /// <summary>
        /// The country dialing code for a communication number.
        /// </summary>
        public string DialCountry { get; set; }
        /// <summary>
        /// The extension of the associated communication number.
        /// </summary>
        public string DialExtension { get; set; }
        /// <summary>
        /// For EF navigation property mapping. Currently not used.
        /// </summary>
        [JsonIgnore]
        public Worker Worker { get; set; }

        /// <summary>
        /// Debug Info
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            switch (Type)
            {
                case ContactType.MOBILE_PHONE:
                case ContactType.PHONE:
                    return this.AsPhoneNumber();
                case ContactType.EMAIL:
                    return Uri;
                default:
                    return base.ToString();
            }
        }


    }
}
