namespace Paychex.Api.Models.Common
{
    /// <summary>
    /// Link object used for HATEOAS discoverability in api results
    /// </summary>
    public class Link
    {
        /// <summary>
        /// The link relation that describes the state transition
        /// </summary>
        public string Rel { get; set; }

        /// <summary>
        /// The URL (resource) to use to transition to the next state
        /// </summary>
        public string Href { get; set; }

        /// <summary>
        /// unknown, possibly not used? Guessing it's for the language of the link destination (eg en, zh)
        /// </summary>
        public string Hreflang { get; set; }

        /// <summary>
        /// probably media type of destination. Unknown if used
        /// </summary>
        public string Media { get; set; }

        /// <summary>
        /// probably title to show instead of href. Unknown if used.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// unknown if used
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// suggests link should not be followed? unknown if used.
        /// </summary>
        public string Deprecation { get; set; }
    }
}
