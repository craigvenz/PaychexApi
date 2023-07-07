using System.ComponentModel;

namespace Paychex.Api.Models.Payroll
{
    /// <summary>
    /// The effect that the pay component will have on the check amount.
    /// </summary>
    public enum EffectOnPayType
    {
        /// <summary>
        /// Adds to pay for tax calculations, but is subtracted from net
        /// </summary>
        [Description("Addition but only applies to tax calculations")]
        ADDITION_WITH_IN_OUT, 
        /// <summary>
        /// Adds to pay
        /// </summary>
        [Description("Adds to pay")]
        ADDITION, 
        /// <summary>
        /// Reduces pay
        /// </summary>
        [Description("Reduces pay")]
        REDUCTION,
        /// <summary>
        /// No effect on pay
        /// </summary>
        [Description("No effect on pay")]
        EMPLOYER_INFORMATIONAL
    }
}
