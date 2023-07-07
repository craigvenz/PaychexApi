using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Paychex.Api.Models.Common;
using Paychex.Api.Models.Payroll;
using Paychex.Api.Models.Workers;
using WorkerCommunication = Paychex.Api.Models.Workers.Communication;

namespace Paychex.Api
{
    /// <summary>
    /// Extension methods for convenience. Like it says.
    /// </summary>
    public static class PaychexConvenienceExtensions
    {
        /// <summary>
        /// Retrieve the street address from the worker list of contacts.
        /// </summary>
        /// <param name="contacts">Contacts object on Worker</param>
        /// <returns>Address if found or null</returns>
        public static WorkerCommunication GetStreetAddress(this IEnumerable<WorkerCommunication> contacts) =>
            (from wc in contacts
             where wc.Type == ContactType.STREET_ADDRESS 
                   || wc.Type == ContactType.PO_BOX_ADDRESS
             orderby wc.UsageType
             select wc)
            .FirstOrDefault();

        /// <summary>
        /// Retrieve the phone number from the worker list of contacts.
        /// </summary>
        /// <param name="contacts">Contacts object on Worker</param>
        /// <returns>Phone number if found or null</returns>
        public static WorkerCommunication GetPhoneNumber(this IEnumerable<WorkerCommunication> contacts) =>
            (
                from wc in contacts
                where wc.Type == ContactType.PHONE
                      || wc.Type == ContactType.MOBILE_PHONE
                orderby wc.UsageType
                select wc)
            .FirstOrDefault();

        /// <summary>
        /// Straight up laziness. Between function from SQL. This one accepts a tuple of two items. For fun.
        /// </summary>
        /// <param name="value">Object to check</param>
        /// <param name="range">Range as a tuple: (X begin, X end)</param>
        /// <param name="rangeBoundary">Enum for specifying inclusion of boundaries.</param>
        /// <returns>True if the value falls in the range.</returns>
        public static bool Between(
            this IComparable value,
            (IComparable begin, IComparable end) range,
            RangeBoundaryType rangeBoundary = RangeBoundaryType.Inclusive) =>
            Between(value, range.begin, range.end, rangeBoundary);

        public enum RangeBoundaryType
        {
            [Description("Exclusive")]
            Exclusive,

            [Description("Inclusive on both boundaries")]
            Inclusive,

            [Description("Inclusive on only the lower boundary")]
            InclusiveLowerBoundaryOnly,

            [Description("Inclusive on only the upper boundary")]
            InclusiveUpperBoundaryOnly
        }

        /// <summary>
        /// Straight up laziness. Between function from SQL.
        /// https://stackoverflow.com/questions/5708986/is-there-between-datetime-in-c-sharp-just-like-sql-does/28857242#28857242
        /// </summary>
        /// <param name="value">Object to check</param>
        /// <param name="begin">Range start.</param>
        /// <param name="end">Range end.</param>
        /// <param name="rangeBoundary">Enum for specifying inclusion of boundaries.</param>
        /// <returns>True if the value falls in the range.</returns>
        public static bool Between(
            this IComparable value,
            IComparable begin,
            IComparable end,
            RangeBoundaryType rangeBoundary = RangeBoundaryType.Inclusive)
        {
            switch (rangeBoundary)
            {
                case RangeBoundaryType.Exclusive:
                    return value.CompareTo(begin) > 0 && value.CompareTo(end) < 0;

                case RangeBoundaryType.Inclusive:
                    return value.CompareTo(begin) >= 0 && value.CompareTo(end) <= 0;

                case RangeBoundaryType.InclusiveLowerBoundaryOnly:
                    return value.CompareTo(begin) >= 0 && value.CompareTo(end) < 0;

                case RangeBoundaryType.InclusiveUpperBoundaryOnly:
                    return value.CompareTo(begin) > 0 && value.CompareTo(end) <= 0;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Laziness shortcut for checking a string against a list of others using case insensitive.
        /// </summary>
        /// <param name="first"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public static bool MatchesAny(this string first, params string[] list) =>
            list.Any(s => string.Equals(first, s, StringComparison.CurrentCultureIgnoreCase));

        /// <summary>
        /// Convert Worker ethnicity type to string value LCP database expects.
        /// </summary>
        /// <param name="ethnicityType">Ethnicity type.</param>
        /// <returns>String representation.</returns>
        public static string TranslateString(this EthnicityType ethnicityType)
        {
            switch (ethnicityType)
            {
                case EthnicityType.HISPANIC_OR_LATINO:
                    return "HISPANIC";
                case EthnicityType.WHITE_NOT_OF_HISPANIC_ORIGIN:
                    return "CAUCASIAN";
                case EthnicityType.BLACK_OR_AFRICAN_AMERICAN:
                    return "AFRICAN AMERICAN";
                case EthnicityType.AMERICAN_INDIAN_OR_ALASKAN_NATIVE:
                    return "NATIVE AMERICAN";
                case EthnicityType.TWO_OR_MORE_RACES:
                    return "TWO OR MORE RACES";
                case EthnicityType.NATIVE_HAWAIIAN_OR_OTHER_PACIFIC_ISLAND: // ??
                case EthnicityType.ASIAN_OR_PACIFIC_ISLANDER:
                case EthnicityType.ASIAN:
                    return "ASIAN";
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Convert Worker Gender type to string value LCP database expects.
        /// </summary>
        /// <param name="genderType">Gender type.</param>
        /// <returns>String representation.</returns>
        public static string TranslateString(this Gender genderType)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (genderType)
            {
                case Gender.MALE:
                case Gender.FEMALE:
                    return genderType.ToString()[..1];
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// Probably going to find this is something that's used in a lot of places so put in here
        /// </summary>
        /// <param name="comps">list of <see cref="PayrollComponent">Payroll components</see>.</param>
        /// <returns> four component types for the three hourly types and fringe benefit.</returns>
        public static (PayrollComponent hourly, PayrollComponent overTime, PayrollComponent doubleOverTime, PayrollComponent fringe) 
            GetHourlyComponents(this ICollection<PayrollComponent> comps)
        {
            var hourly = comps.FirstOrDefault(x => x.Name == PayrollNameConstants.Hourly);
            var     ot = comps.FirstOrDefault(x => x.Name == PayrollNameConstants.OverTime);
            var    dot = comps.FirstOrDefault(x => x.Name == PayrollNameConstants.DoubleOverTime);
            var fr = comps.FirstOrDefault(x => x.Name == "Fringe");
            return (hourly, ot, dot, fr);
        }

        /// <summary>
        /// Check the various places in order for a hire date since there is a hierarchy of
        /// job and assignment
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        public static DateTime? GetHireDate(this CheckLine line)
        {
            var result =
                new[]
                {
                    line.Job?.StartDate,
                    line.Assignment?.Assignment?.StartDate,
                    line.ContainingCheck?.Payee?.HomeJob?.StartDate,
                    line.ContainingCheck?.Payee?.HomeAssignment?.StartDate,
                    line.ContainingCheck?.Payee?.Job?.StartDate
                }.FirstOrDefault(x => x.HasValue);
            return result;
        }
    }
}
