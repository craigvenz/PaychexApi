using System;
using System.Collections.Generic;
using System.Linq;
using Paychex.Api.Models.Common;
using Paychex.Api.Models.Workers;

namespace Paychex.Api.Models.Payroll
{
    /// <summary>
    /// A single paycheck instance.
    /// </summary>
    public class Check : BaseObject
    {
        /// <summary>
        /// Id of Worker who owns this check.
        /// </summary>
        public string WorkerId { get; set; }
        /// <summary>
        /// primary key.
        /// </summary>
        public string PaycheckId { get; set; }
        /// <summary>
        /// id of the pay period this check falls in
        /// </summary>
        public string PayPeriodId { get; set; }
        /// <summary>
        /// List of <see cref="Earning">Earnings</see> on this check. Generally positive additions.
        /// </summary>
        public List<Earning> Earnings { get; set; } = new List<Earning>();
        /// <summary>
        /// List of Deductions on this check. Uses same fields as an <see cref="Earning">Earning</see>.
        /// </summary>
        public List<Earning> Deductions { get; set; } = new List<Earning>();
        /// <summary>
        /// List of Informational line items on this check. Uses same fields as an <see cref="Earning">Earning</see>.
        /// Appears to just be things listed on the check but aren't used for calculation purposes.
        /// </summary>
        public List<Earning> Informational { get; set; }
        /// <summary>
        /// List of Taxes on this check. 
        /// </summary>
        public List<Tax> Taxes { get; set; } = new List<Tax>();
        /// <summary>
        /// Unknown, probably not relevant to us.
        /// </summary>
        public bool? BlockAutoDistribution { get; set; }

        private decimal? _netPay;
        /// <summary>
        /// Net value of the check.
        /// </summary>
        public decimal? NetPay
        {
            get => _netPay ?? CalculateNet();
            set => _netPay = value;
        }
        /// <summary>
        /// Date check was issued.
        /// </summary>
        public DateTime CheckDate { get; set; }
        /// <summary>
        /// Check type? Only seen value of "Regular" so far.
        /// </summary>
        public string CheckType { get; set; }
        /// <summary>
        /// Number on the check. Apparently still given even if this was a direct deposit.
        /// </summary>
        public string CheckNumber { get; set; }
        /// <summary>
        /// Navigation property to the <see cref="Worker">worker</see> who is being paid by this check.
        /// </summary>
        [JsonIgnore]
        public Worker Payee {get;set;}
        /// <summary>
        /// Navigation property to the check's <see cref="PayPeriod">pay period</see>.
        /// </summary>
        [JsonIgnore]
        public PayPeriod PayPeriod {get;set;}

        /// <summary>
        /// Implemented for equality purposes - we consider a check the same if it has the same checkNumber. Seems fine for now, will revisit if needed.
        /// </summary>
        /// <param name="obj">Comparison object</param>
        /// <returns>Equality of this object and the parameter passed.</returns>
        public override bool Equals(object obj) => (CheckNumber??string.Empty).Equals((obj as Check)?.CheckNumber);

        /// <summary>
        /// Naive implementation. Unknown if actually correct.
        /// </summary>
        /// <returns></returns>
        public decimal CalculateNet() =>
              Earnings  .Sum(e => e.Amount.GetValueOrDefault())
            - Deductions.Sum(d => d.Amount.GetValueOrDefault())
            - Taxes     .Sum(t => t.Amount.GetValueOrDefault());

        /// <summary>
        /// Implemented for equality purposes - we consider a check the same if it has the same checkNumber. Seems fine for now, will revisit if needed.
        /// </summary>
        /// <returns>Hash code.</returns>
        public override int GetHashCode()
        {
            var hashCode = 448126915;
            hashCode = hashCode * -1521134295 + (CheckNumber??string.Empty).GetHashCode();
            return hashCode;
        }
        /// <summary>
        /// debug info
        /// </summary>
        /// <returns></returns>
        public override string ToString() => $"Check {CheckNumber}, {CheckDate.ToShortDateString()} - {NetPay:c}";
    }
}
