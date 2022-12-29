using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Muffin.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class MaxValueAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly double _maxValue;

        public MaxValueAttribute(double maxValue)
        {
            _maxValue = maxValue;
            ErrorMessage = string.Format(DEFAULT_ERROR_MESSAGE, _maxValue);
        }

        public MaxValueAttribute(int maxValue)
        {
            _maxValue = maxValue;
            ErrorMessage = string.Format(DEFAULT_ERROR_MESSAGE, _maxValue);
        }

        public const string ERROR_MESSAGE_KEY = "muffin.componentmodel.dataannotations.maxvalueattribute.errormessage";
        public const string DEFAULT_ERROR_MESSAGE = "Enter a value smaller or equal to {0}";

        public override bool IsValid(object value)
        {
            return Convert.ToDouble(value) <= _maxValue;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            var errorMessage = FormatErrorMessage(context.ModelMetadata.GetDisplayName());
            MergeAttribute(context.Attributes, "data-val-maxvalue", errorMessage);
        }

        private bool MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
            {
                return false;
            }
            attributes.Add(key, value);
            return true;
        }
    }
}
