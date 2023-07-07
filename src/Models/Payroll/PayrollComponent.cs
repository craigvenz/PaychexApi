using System;
using System.Collections.Generic;

namespace Paychex.Api.Models.Payroll
{
    /// <summary>
    /// The representational state of pay components.
    /// </summary>
    public class PayrollComponent
    {
        /// <summary>
        /// The unique identifier used to identify a pay component.
        /// </summary>
        public string ComponentId { get; set; }
        /// <summary>
        /// Name given to the pay component.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The category that this component falls into. - https://developer.paychex.com/api-documentation-and-exploration/api-references/payroll/paycomponents
        /// </summary>
        public string ClassificationType { get; set; }
        /// <summary>
        /// The description of the Pay Component.
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The code of the Pay Component.
        /// </summary>
        public int Code { get; set; }
        /// <summary>
        /// The effect that the pay component will have on the check amount.
        /// </summary>
        public EffectOnPayType EffectOnPay { get; set; }
        /// <summary>
        /// The date that the pay component is able to be applied on a check.
        /// </summary>
        public DateTime StartDate { get; set; }

        /// <summary>
        /// List of workerTypes that the component can be applied to.
        /// </summary>
        public List<WorkerType> AppliesToWorkerTypes { get; set; }

        /// <summary>
        /// debug info
        /// </summary>
        /// <returns></returns>
        public override string ToString() =>
            $"({EffectOnPay}) {ComponentId}: {Name}, {ClassificationType}, applies to: {string.Join(",", AppliesToWorkerTypes)}, starts {StartDate.ToShortDateString()}";
    }
}
