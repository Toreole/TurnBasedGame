using System;


[AttributeUsage(AttributeTargets.Field)]
public class InjectedAttribute : Attribute
{
    public bool IsFatal { get; private set; }

    public InjectedAttribute(bool isFatal)
    {
        IsFatal = isFatal;
    }
}

