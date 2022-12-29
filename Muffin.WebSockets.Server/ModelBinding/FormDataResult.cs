using Microsoft.AspNetCore.Mvc.ModelBinding;
using Muffin.Common.Util;
using Newtonsoft.Json;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Muffin.WebSockets.Server.ModelBinding
{
    public abstract class FormDataResult
    {
        [JsonProperty(PropertyName = "error", NullValueHandling = NullValueHandling.Ignore)]
        public FormDataErrorContainer Errors { get; set; }

        /// <summary>
        /// Fügt ein Fehler Bindung hinzu
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="error">LocalizedString oder ein keypath String angeben</param>
        public void AddError(string fieldName, object error)
        {
            if (Errors == null)
            {
                Errors = new FormDataErrorContainer();
            }
            if (!Errors.TryGetValue(fieldName, out FormDataError formDataError))
            {
                formDataError = new FormDataError();
            }
            formDataError.Messages.Add(error);
            Errors[fieldName] = formDataError;
        }

        /// <summary>
        /// Fügt ein Fehler Bindung hinzu
        /// </summary>
        /// <param name="languageName"></param>
        /// <param name="fieldName"></param>
        /// <param name="error">LocalizedString oder ein keypath String angeben</param>
        public void AddError(string languageName, string fieldName, object error)
        {
            if (Errors == null)
            {
                Errors = new FormDataErrorContainer();
            }

            Errors.AddError(languageName, fieldName, error);
        }

        public void AddErrors(Dictionary<string, object> errors)
        {
            if (errors == null)
            {
                return;
            }

            foreach (var pair in errors)
            {
                AddError(pair.Key, pair.Value);
            }
        }

        public void AddErrors(string fieldName, IEnumerable<string> errors)
        {
            if (errors == null)
            {
                return;
            }

            foreach (var error in errors)
            {
                AddError(fieldName, error);
            }
        }

        public static FormDataResult<TFormData> FromObject<TFormData>(TFormData formData)
        {
            return new FormDataResult<TFormData>()
            {
                Data = formData
            };
        }

        public static FormDataResult New
        {
            get => new FormDataResult<object>();
        }

        [JsonIgnore]
        public bool HasErrors { get => Errors?.Any() ?? false; }

        [JsonIgnore]
        public bool ReturnAsApiError { get; set; }

        public void AddModelStateErrors(ModelStateDictionary modelState)
        {
            foreach (var key in modelState.Keys)
            {
                var modelStateEntry = modelState[key];
                if (modelStateEntry.ValidationState == ModelValidationState.Invalid)
                {
                    foreach (var error in modelStateEntry.Errors)
                    {
                        AddError(key.FirstCharToLowerCase(), error.ErrorMessage);
                    }
                }
            }
        }
    }

    public class FormDataResult<T> : FormDataResult
    {
        [JsonProperty(PropertyName = "value", NullValueHandling = NullValueHandling.Ignore)]
        public T Data { get; set; }
    }

    [JsonConverter(typeof(FormDataErrorContainerJsonConverter))]
    public class FormDataErrorContainer : IDictionary<string, FormDataError>
    {
        private Dictionary<string, FormDataError> Proxy = new Dictionary<string, FormDataError>();

        [JsonProperty(PropertyName = "i18n", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, Dictionary<string, List<object>>> ErrorContainer { get; set; }

        public void AddError(string languageCode, string fieldName, object errorValue)
        {
            if (ErrorContainer == null)
            {
                ErrorContainer = new Dictionary<string, Dictionary<string, List<object>>>();
            }

            if (!ErrorContainer.TryGetValue(languageCode, out Dictionary<string, List<object>> languageBasedErrorContainer))
            {
                languageBasedErrorContainer = new Dictionary<string, List<object>>();
                ErrorContainer[languageCode] = languageBasedErrorContainer;
            }

            if (!languageBasedErrorContainer.TryGetValue(fieldName, out List<object> errorList))
            {
                errorList = new List<object>();
                languageBasedErrorContainer[fieldName] = errorList;
            }

            errorList.Add(errorValue);
        }

        #region IDictionary<string, FormDataError>

        public FormDataError this[string key] { get => Proxy[key]; set => Proxy[key] = value; }

        public ICollection<string> Keys => Proxy.Keys;

        public ICollection<FormDataError> Values => Proxy.Values;

        public int Count => Proxy.Count;

        public bool IsReadOnly => false;

        public void Add(string key, FormDataError value)
        {
            Proxy.Add(key, value);
        }

        public void Add(KeyValuePair<string, FormDataError> item)
        {
            Proxy.Add(item.Key, item.Value);
        }

        public void Clear()
        {
            Proxy.Clear();
        }

        public bool Contains(KeyValuePair<string, FormDataError> item)
        {
            return Proxy.Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return Proxy.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, FormDataError>[] array, int arrayIndex)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, FormDataError>> GetEnumerator()
        {
            return Proxy.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return Proxy.Remove(key);
        }

        public bool Remove(KeyValuePair<string, FormDataError> item)
        {
            return Proxy.Remove(item.Key);
        }

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out FormDataError value)
        {
            return Proxy.TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        #region Helper

        public Dictionary<string, object> ToDictionary()
        {
            var result = Proxy.ToDictionary(x => x.Key, x => (object)x.Value);
            if (ErrorContainer != null)
            {
                result["i18n"] = ErrorContainer;
            }

            return result;
        }

        #endregion
    }

    public class FormDataErrorContainerJsonConverter : JsonConverter
    {
        public override bool CanConvert(System.Type objectType)
        {
            return true;
        }

        public override bool CanRead => false;
        public override bool CanWrite => true;

        public override object ReadJson(JsonReader reader, System.Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new System.NotImplementedException();
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var container = value as FormDataErrorContainer;

            var dyn = container.ToDictionary().AggregateToDynamic();
            serializer.Serialize(writer, dyn);
            //foreach (var pair in dict)
            //{
            //    writer.WriteStartObject();
            //    writer.WritePropertyName(pair.Key);
            //    serializer.Serialize(writer, pair.Value);
            //    writer.WriteEndObject();
            //}
        }
    }

    /*
     * {
     *  
     */
}