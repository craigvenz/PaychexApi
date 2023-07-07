namespace Paychex.Api.Models.Common
{
    /// <summary>
    /// Client to client relationships where payroll needs to be split into separate groups but there's one Federal employer ID.
    /// Probably not really useful to us at this time.
    /// </summary>
    public class Organization : BaseObject
    {
        /// <summary>
        /// The unique identifier associated with this organizations representation.
        /// </summary>
        public string OrganizationId { get; set; }
        /// <summary>
        /// The name of the organization.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The number assigned to the organization.
        /// </summary>
        public string Number { get; set; }
        /// <summary>
        /// The level number within the organizational structures hierarchy.
        /// </summary>
        public string Level { get; set; }

        /// <summary>
        /// Debug info
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"{Name} ({Number})";
    }
}
