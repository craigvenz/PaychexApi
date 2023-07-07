using System;
using System.Text;
using Paychex.Api.Models.Common;
using Paychex.Api.Models.Companies;

namespace Paychex.Api.Models.Payroll
{
    /// <summary>
    /// Paycheck line component information.
    /// </summary>
    public class Earning
    {
        /// <summary>
        /// Id of the type of <see cref="PayrollComponent">paycheck line component</see>.
        /// </summary>
        public string ComponentId { get; set; }
        /// <summary>
        /// Primary key of this earning.
        /// </summary>
        public string CheckComponentId { get; set; }
        /// <summary>
        /// Identifying name of the component
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Type of component. Seems to be a lot of possible values here
        /// </summary>
        public string ClassificationType { get; set; }
        /// <summary>
        /// Effect on pay.
        /// </summary>
        public EffectOnPayType EffectOnPay { get; set; }
        /// <summary>
        /// Hours that don't have a rate but need to be tracked - example being fringe benefit that uses the hours for a calculation
        /// </summary>
        public bool Memoed { get; set; }
        /// <summary>
        /// associated <see cref="LaborAssignment">labor assignment/craft</see>
        /// </summary>
        public string LaborAssignmentId { get; set; }
        /// <summary>
        /// associated <see cref="Common.Organization">Organization</see>.
        /// </summary>
        public Organization Organization { get; set; }
        /// <summary>
        /// Associated <see cref="Companies.Job">Job</see>.
        /// </summary>
        public string JobId { get; set; }
        /// <summary>
        /// If representing a flat amount, this value is specified here.
        /// </summary>
        public decimal? Amount { get; set; }
        /// <summary>
        /// Date this value was incurred. Can be null.
        /// </summary>
        public DateTime? LineDate { get; set; }
        /// <summary>
        /// Rate of pay per hour.
        /// </summary>
        public decimal? Rate { get; set; }
        /// <summary>
        /// Number of hours associated.
        /// </summary>
        public decimal? Hours { get; set; }
        /// <summary>
        /// Name of the job - seems to be used when no job id is given. Ugh.
        /// </summary>
        public string JobName { get; set; }
        /// <summary>
        /// Navigation property for the <see cref="Companies.Job">job</see> this earning is from
        /// </summary>
        [JsonIgnore]
        public Job Job {get;set;}
        /// <summary>
        /// Navigation property for the <see cref="LaborAssignment">assignment</see> on this earning
        /// </summary>
        [JsonIgnore]
        public LaborAssignment Assignment {get;set;}
        /// <summary>
        /// Navigation property for the <see cref="PayrollComponent">payroll component</see> used on this earning
        /// </summary>
        [JsonIgnore]
        public PayrollComponent Component {get;set;}

        /// <summary>
        /// debug info
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{0}: {1}", CheckComponentId, Name);
            if (LineDate.HasValue)
                sb.AppendFormat(" {0}", LineDate.Value.ToShortDateString());
            if (Rate.HasValue)
            {
                sb.AppendFormat(" {0}hrs @ {1:c} = {2:c}", Hours, Rate, Amount.GetValueOrDefault(Hours.GetValueOrDefault() * Rate.GetValueOrDefault()));
            }
            else
            {
                sb.AppendFormat("{0:c}", Amount);
            }

            sb.AppendFormat(" {0}", Job?.ToString() ?? JobName);
            sb.AppendFormat(" {0}", Assignment);
            sb.AppendFormat(" {0}", Organization);
            return sb.ToString();
        }
    }
}
