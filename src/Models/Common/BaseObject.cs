using System.Collections.Generic;

namespace Paychex.Api.Models.Common
{
    /// <summary>
    /// Base object for most Paychex entities - common place to store the HATEOAS linkages
    /// https://developer.paychex.com/api-documentation-and-exploration/documentation/documentation/hypermedia-controls
    /// </summary>
    public class BaseObject
    {
        /// <summary>
        /// List of <see cref="Link">Links</see> associated with this object.
        /// </summary>
        public List<Link> Links { get; set; }
    }
}
