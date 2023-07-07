using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using EnumsNET;
using Paychex.Api.Models.Common;
using RestSharp;

namespace Paychex.Api.Api
{
    /// <summary>
    /// 
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Return a datetime in javascript/json standard format
        /// </summary>
        /// <param name="date">A datetime.</param>
        /// <returns>string converted as advertised</returns>
        /// <remarks>Thought this might be the same as ToString("o") but apparently not</remarks>
        public static string ToJsonDate(this DateTime date) => date.ToString("yyyy-MM-ddTHH:mm:ssK");

        /// <summary>
        /// Extension to <see cref="RestSharp">RestSharp</see> <see cref="Parameter">Parameters</see>. Adds support
        /// for an optional parameter that's only evaluated when the supplied predicate is true.
        /// </summary>
        /// <param name="request">Request object.</param>
        /// <param name="parameterName">Name of parameter.</param>
        /// <param name="parameterFunc">Function that produces the value. A func is used here so the value isn't actually evaluated until the predicate passes.</param>
        /// <param name="parameterType">Type of <see cref="ParameterType">parameter</see>.</param>
        /// <param name="condition">Predicate to evaluate before evaluating the parameter value.</param>
        /// <returns>Original request to allow fluent chaining.</returns>
        public static RestRequest AddOptionalParameter(
            this RestRequest request,
            string parameterName,
            Func<object> parameterFunc,
            ParameterType parameterType,
            Func<bool> condition
        )
        {
            if ((condition?.Invoke()).GetValueOrDefault())
            {
                request.AddParameter(parameterName, parameterFunc?.Invoke(), parameterType);
            }
            return request;
        }

        /// <summary>
        /// Extension to <see cref="RestSharp">RestSharp</see> <see cref="Parameter">Parameters</see>. Adds support
        /// for an optional parameter that's only evaluated when the supplied predicate is true. Defaults to GetOrPost type parameter.
        /// </summary>
        /// <param name="request">Request object</param>
        /// <param name="parameterName">Name of parameter</param>
        /// <param name="parameterFunc">Function that produces the value. A func is used here so the value isn't actually evaluated until the predicate passes.</param>
        /// <param name="condition">Predicate to evaluate before evaluating the parameter value.</param>
        /// <returns>Original request to allow fluent chaining.</returns>
        public static RestRequest AddOptionalParameter(
            this RestRequest request,
            string parameterName,
            Func<object> parameterFunc,
            Func<bool> condition
        ) => AddOptionalParameter(request, parameterName, parameterFunc, ParameterType.GetOrPost, condition);

        /// <summary>
        /// Extension to <see cref="RestSharp">RestSharp</see> <see cref="Parameter">Parameters</see>. Adds support
        /// for an optional parameter that's only evaluated when the supplied predicate is true. 
        /// </summary>
        /// <param name="request">Request object</param>
        /// <param name="parameterName">Name of parameter</param>
        /// <param name="parameterFunc">Function that produces the value. A func is used here so the value isn't actually evaluated until the predicate passes.</param>
        /// <param name="condition">Predicate to evaluate before evaluating the parameter value.</param>
        public static RestRequest AddOptionalQueryParameter(
            this RestRequest request,
            string parameterName,
            Func<object> parameterFunc,
            Func<bool> condition
        ) => AddOptionalParameter(request, parameterName, parameterFunc, ParameterType.QueryString, condition);

        /// <summary>
        /// Debugging aid that produces a string from an enumerable of objects, optionally which satisfy the predicate.
        /// </summary>
        /// <param name="list">List of TL.</param>
        /// <param name="d">Separator to use in the string output.</param>
        /// <param name="predicate">Output a value if this condition is true.</param>
        /// <typeparam name="TL">Obvious.</typeparam>
        /// <returns>String representation of the list.</returns>
        public static string ListOrNull<TL>(this IEnumerable<TL> list, string d = ", ", Func<TL, bool> predicate = null)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("IEnumerable<")
              .Append(typeof(TL).Name)
              .Append("> ");
            if (list == null)
            {
                sb.Append("(null)");
            }
            else
            {
                var filtered = (predicate != null ? list.Where(predicate) : list).ToList();
                sb.Append(filtered.Count == 0 ? "[]" : $"[{string.Join(d, filtered)}]");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Retrieves the phone number from a contact. Since contacts can be of
        /// various <see cref="ContactType">types</see>, this is a shortcut for
        /// resolving it from its phone number specific fields. Probably should just be on the class
        /// itself but wanted to avoid implementations in the models.
        /// </summary>
        /// <param name="contact">Contact object.</param>
        /// <returns>Phone number or empty string if this isn't a phone number.</returns>
        public static string AsPhoneNumber(this Models.Workers.Communication contact)
        {
            if (contact?.Type != ContactType.PHONE && contact?.Type != ContactType.MOBILE_PHONE)
                return string.Empty;

            void IfStringChanged(StringWriter txt, params (string str, Action<string> preAction, Action postAction)[] actions)
            {
                var length = txt.ToString().Length;
                foreach (var (str, preAction, postAction) in actions)
                {
                    if (!string.IsNullOrWhiteSpace(str))
                        preAction(str);
                    if (txt.ToString().Length == length)
                        continue;
                    postAction?.Invoke();
                    length = txt.ToString().Length;
                }
            }
            using (var sw = new StringWriter())
            {
                // ReSharper disable AccessToDisposedClosure
                IfStringChanged(
                    sw,
                    (contact.DialCountry, s => sw.Write($"+{s}"), () => sw.Write(" ")),
                    (contact.DialArea, s => sw.Write($"({s})"), () => sw.Write(" ")),
                    (contact.DialNumber, s =>
                                         {
                                             var m = Regex.Match(s, "(?<fp>[0-9]{3})(-)?(?<sp>[0-9]+)");
                                             sw.Write(m.Success ? $"{m.Groups["fp"].Value}-{m.Groups["sp"].Value}" : s);
                                         }, null),
                    (contact.DialExtension, s => sw.Write($" ext {s}"), null)
                );
                // ReSharper enable AccessToDisposedClosure
                return sw.GetStringBuilder()
                         .ToString();
            }
        }

        /// <summary>
        /// Creates <see cref="RestSharp">RestSharp</see> <see cref="Parameter">parameters</see>
        /// from a Paychex <see cref="Pagination">pagination</see> object.
        /// </summary>
        /// <param name="pagination"></param>
        /// <returns></returns>
        public static IEnumerable<Parameter> AddPageParameters(this Pagination pagination)
        {
            if (!pagination.Limit.HasValue)
                yield break;
            if (pagination.Offset.HasValue)
                yield return MakeParam(Constants.PaginationOffset, pagination.Offset.Value.ToString());
            yield return MakeParam(Constants.PaginationRecordLimit, pagination.Limit.Value.ToString());
            yield return MakeParam(Constants.PaginationId, pagination.ETag, ParameterType.HttpHeader);
        }

        /// <summary>
        /// 'Shortcut' for generating a <see cref="RestSharp">RestSharp</see> <see cref="Parameter">parameter</see>
        /// that defaults to type <see cref="ParameterType.GetOrPost">GetOrPost</see> if not given. 
        /// Just gives a possibly more succinct instantiation syntax without using object creation syntax.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <param name="paramType"></param>
        /// <returns></returns>
        public static Parameter
            MakeParam(string name, string value, ParameterType paramType = ParameterType.GetOrPost) =>
            Parameter.CreateParameter(name, value, paramType);

        /// <summary>
        /// Create an enumerable of an Exception and it's inner exception chain.
        /// </summary>
        /// <param name="root">An exception.</param>
        /// <returns>The same exception but in an enumerable format.</returns>
        public static IEnumerable<Exception> ToEnumerable(this Exception root)
        {
            return new[] {root}
                   .Concat(
                       root is AggregateException ae
                           ? ae.InnerExceptions?.SelectMany(ToEnumerable)
                           : new Exception[] { }
                   )
                   .Concat(root.InnerException?.ToEnumerable() ?? new Exception[] { });
        }

        /// <summary>
        /// shortcut to enums.net method to get description
        /// </summary>
        /// <typeparam name="T">enum type</typeparam>
        /// <param name="val">enum value</param>
        /// <returns>description on the value or ToString if it doesn't have one</returns>
        public static string GetDescription<T>(this T val) where T : struct, Enum =>
            val.GetAttributes()
               .Get<DescriptionAttribute>()?
               .Description
            ?? val.ToString();

        /// <summary>
        /// shortcut to enums.net method to get description for nullable enums
        /// </summary>
        /// <typeparam name="T">enum type</typeparam>
        /// <param name="val">enum value</param>
        /// <returns>description on the value, ToString if it doesn't have one, or empty string if null</returns>
        public static string GetDescription<T>(this T? val) where T : struct, Enum =>
            val?.GetDescription() ?? string.Empty;
    }
}
