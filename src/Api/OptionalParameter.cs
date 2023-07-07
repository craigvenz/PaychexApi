using System;
using RestSharp;

namespace Paychex.Api.Api;

internal record OptionalParameter : Parameter
{
    public OptionalParameter() { }

    public OptionalParameter(string name, Func<object> valueFunc, ParameterType type, Func<bool> condition)
        : base(name, null, type)
    {
        ValueFunc = valueFunc;
        Condition = condition;
    }

    public OptionalParameter(string name,
                             Func<object> valueFunc,
                             string contentType,
                             ParameterType type,
                             Func<bool> condition)
        : base(name, null, contentType, type)
    {
        ValueFunc = valueFunc;
        ContentType = contentType;
        Condition = condition;
    }

    public new object Value => ValueFunc?.Invoke();

    public Func<bool> Condition { get; set; } = () => true;

    public Func<object> ValueFunc { get; set; }

    public override string ToString() => $"optional {Name}={(Condition() ? ValueFunc() : "(Not evaluated)")}";
}
