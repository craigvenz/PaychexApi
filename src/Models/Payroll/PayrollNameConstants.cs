namespace Paychex.Api.Models.Payroll
{
    /// <summary>
    /// Defined strings for hourly payroll components.
    /// Who knows if these are actually constant on Paychex's side?
    /// Nope, they aren't.
    /// </summary>
    public static class PayrollNameConstants
    {
        /// <summary>
        /// Hourly rate
        /// </summary>
        public const string Hourly = "Hourly";
        /// <summary>
        /// Overtime rate
        /// </summary>
        public const string OverTime = "Overtime";
        /// <summary>
        /// Double Overtime rate
        /// </summary>
        public const string DoubleOverTime = "Overtime 2.0";

        /// <summary>
        /// Shortcut to determine if the <see cref="PayrollComponent">payroll component</see> name is an hourly component
        /// TODO: need to revisit since the assumptions on the above constants is incorrect
        /// </summary>
        /// <param name="name">Component name</param>
        /// <returns>If it's an hourly component</returns>
        public static bool IsHourly(string name) =>
            name == Hourly || name == OverTime || name == DoubleOverTime;
    }
}
