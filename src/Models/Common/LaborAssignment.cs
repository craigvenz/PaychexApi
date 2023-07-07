using System;
using System.Text;

namespace Paychex.Api.Models.Common
{
    /// <summary>
    /// Labor assignment - probably equivalent to Craft for us.
    /// </summary>
    public class LaborAssignment : BaseObject
    {
        /// <summary>
        /// The unique identifier associated with this labor assignment representation.
        /// </summary>
        public string LaborAssignmentId { get; set; }
        /// <summary>
        /// The name of the labor assignment.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The start date associated with this labor assignment.
        /// </summary>
        public DateTime? StartDate { get; set; }
        /// <summary>
        /// The end date associated with this labor assignment.
        /// </summary>
        public DateTime? EndDate { get; set; }
        /// <summary>
        /// The organization associated with this labor assignment.
        /// </summary>
        public string OrganizationId { get; set; }
        /// <summary>
        /// The position associated with this labor assignment - according to their docs this is a reference to JobTitles
        /// </summary>
        public string PositionId { get; set; }
        /// <summary>
        /// The locations associated with this labor assignment - refers to paychex Locations objects which we aren't using
        /// </summary>
        public string LocationId { get; set; }

        /// <summary>
        /// Navigation property added for using EF to store an upload into a dedicated db - not done
        /// </summary>
        [JsonIgnore]
        public Organization Organization { get; set; }

        /// <inheritdoc />
        public override bool Equals(object obj) => obj is LaborAssignment other && Equals(other);

        /// <summary>
        /// Equality method - implemented to make linq to objects group join work
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        protected bool Equals(LaborAssignment other) => LaborAssignmentId == other.LaborAssignmentId;

        /// <inheritdoc />
        public override int GetHashCode() => (LaborAssignmentId?.GetHashCode()).GetValueOrDefault();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Name);
            if (StartDate.GetValueOrDefault() != DateTime.MinValue)
            {
                if (EndDate.GetValueOrDefault() != DateTime.MinValue)
                {
                    sb.AppendFormat(
                        " - {0}=>{1}",
                        StartDate.GetValueOrDefault()
                                 .ToShortDateString(),
                        EndDate.GetValueOrDefault()
                               .ToShortDateString()
                    );
                }
                else
                {
                    sb.AppendFormat(
                        " - {0}",
                        StartDate.GetValueOrDefault()
                                 .ToShortDateString()
                    );
                }
            }

            return sb.ToString();
        }
    }
}
