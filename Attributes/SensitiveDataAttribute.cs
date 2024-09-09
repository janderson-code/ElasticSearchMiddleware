using System;

namespace elasticsearch.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public class SensitiveDataAttribute(SensitivityLevel SensitivityLevel) : Attribute
{
    public SensitivityLevel SensitivityLevel = SensitivityLevel;
}


public enum SensitivityLevel
{
    Document = 1,
    CreditCard = 2,
    InitialDigits = 3,
    LastDigits = 4
}