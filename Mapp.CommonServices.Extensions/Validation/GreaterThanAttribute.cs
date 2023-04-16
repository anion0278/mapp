using System.ComponentModel.DataAnnotations;
using System;

namespace Shmap.CommonServices.Validation;

public class GreaterThanAttribute<T>: ValidationAttribute where T:IComparable<T>
{
    public GreaterThanAttribute(T threshold)
    {
        Threshold = threshold;
    }

    public T Threshold { get; }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var comparableCurrentValue = (IComparable<T>)value;
        if (comparableCurrentValue.CompareTo(Threshold) > 0)
        {
            return ValidationResult.Success;
        }
        return new($"The current value is less than {Threshold}");
    }
}