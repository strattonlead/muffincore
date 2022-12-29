using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Muffin.ComponentModel.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class MinValueAttribute : ValidationAttribute, IClientModelValidator
    {
        private readonly double _minValue;

        public MinValueAttribute(double minValue)
        {
            _minValue = minValue;
            ErrorMessage = string.Format(DEFAULT_ERROR_MESSAGE, minValue);
        }

        public MinValueAttribute(int minValue)
        {
            _minValue = minValue;
            ErrorMessage = string.Format(DEFAULT_ERROR_MESSAGE, minValue);
        }

        public const string ERROR_MESSAGE_KEY = "muffin.componentmodel.dataannotations.minvalueattribute.errormessage";
        public const string DEFAULT_ERROR_MESSAGE = "Enter a value greater or equal to {0}";

        public override bool IsValid(object value)
        {
            return Convert.ToDouble(value) >= _minValue;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            MergeAttribute(context.Attributes, "data-val", "true");
            var errorMessage = FormatErrorMessage(context.ModelMetadata.GetDisplayName());
            MergeAttribute(context.Attributes, "data-val-minvalue", errorMessage);
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
