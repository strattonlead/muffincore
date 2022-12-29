using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Muffin.Common.Util;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Muffin.WebSockets.Server.Binders
{
    public class JsonModelBinder : IModelBinder
    {
        private static ExpirableDictionary<object, Dictionary<string, object>> RequestCache = new ExpirableDictionary<object, Dictionary<string, object>>(TimeSpan.FromSeconds(60));

        public readonly JsonSerializerSettings JsonSerializerSettings;
        public readonly JsonConverter[] JsonConverters;
        public readonly IServiceProvider ServiceProvider;
        protected readonly ILogger Logger;

        public JsonModelBinder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Logger = serviceProvider.GetService<ILogger<JsonModelBinder>>();
        }

        public JsonModelBinder(IServiceProvider serviceProvider, JsonSerializerSettings jsonSerializerSettings)
            : this(serviceProvider)
        {
            JsonSerializerSettings = jsonSerializerSettings;
        }

        public JsonModelBinder(IServiceProvider serviceProvider, params JsonConverter[] jsonConverters)
            : this(serviceProvider)
        {
            JsonConverters = jsonConverters;
        }

        public JsonModelBinder(IServiceProvider serviceProvider, JsonSerializerSettings jsonSerializerSettings, params JsonConverter[] jsonConverters)
            : this(serviceProvider)
        {
            JsonSerializerSettings = jsonSerializerSettings;
            JsonConverters = jsonConverters;
        }

        public async Task BindModelAsync(ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
            {
                throw new ArgumentNullException(nameof(bindingContext));
            }

            if (bindingContext.HttpContext.Request.Method?.ToUpper() == "POST")
            {
                string body = "";
                if (!RequestCache.TryGetValue(bindingContext.HttpContext.TraceIdentifier, out Dictionary<string, object> dict))
                {
                    using (var streamReader = new StreamReader(bindingContext.HttpContext.Request.Body))
                    {
                        body = await streamReader.ReadToEndAsync();

                        if (body.StartsWith("[") && bindingContext.ModelType.IsArray)
                        {
                            var obj = JsonConvert.DeserializeObject(body, bindingContext.ModelType);
                            bindingContext.Result = ModelBindingResult.Success(obj);
                            return;
                        }

                        try
                        {
                            dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(body);
                        }
                        catch (Exception e) { throw new Exception(body, e); }
                        RequestCache.Add(bindingContext.HttpContext.TraceIdentifier, dict);
                    }
                }

                if (dict != null && dict.TryGetValue(bindingContext.FieldName, out object field))
                {
                    try
                    {
                        Logger?.LogInformation($"Bind {bindingContext.FieldName} obj: {field}");
                        bindingContext.Result = ModelBindingResult.Success(Convert.ChangeType(field, bindingContext.ModelType));
                        Logger?.LogInformation($"Bind {bindingContext.FieldName} success");
                        return;
                    }
                    catch
                    {
                        Logger?.LogInformation($"Bind {bindingContext.FieldName} fail");
                    }

                    try
                    {
                        Logger?.LogInformation($"Bind {bindingContext.FieldName} try with JsonConvert");
                        bindingContext.Result = ModelBindingResult.Success(DeserializeObjectWithOptions(JsonConvert.SerializeObject(field), bindingContext.ModelType));
                        return;
                    }
                    catch
                    {
                        Logger?.LogInformation($"Bind {bindingContext.FieldName} fail");
                    }
                }
                else if (bindingContext.IsTopLevelObject)
                {
                    try
                    {
                        Logger?.LogInformation($"Bind {bindingContext.FieldName} as top level object");
                        bindingContext.Result = ModelBindingResult.Success(DeserializeObjectWithOptions(body, bindingContext.ModelType));
                        Logger?.LogInformation($"Bind {bindingContext.FieldName} success");
                    }
                    catch (Exception e)
                    {
                        Logger?.LogInformation($"Bind {bindingContext.FieldName} fail");
                    }
                }
                else if (Nullable.GetUnderlyingType(bindingContext.ModelType) != null)
                {
                    Logger?.LogInformation($"Bind {bindingContext.FieldName} as null");
                    bindingContext.Result = ModelBindingResult.Success(null);
                    Logger?.LogInformation($"Bind {bindingContext.FieldName} success");
                }
            }
        }

        private object DeserializeObjectWithOptions(string body, Type type)
        {
            if (JsonSerializerSettings != null)
            {
                return JsonConvert.DeserializeObject(body, type, JsonSerializerSettings);
            }

            if (JsonConverters?.Any() ?? false)
            {
                return JsonConvert.DeserializeObject(body, type, JsonConverters);
            }

            return JsonConvert.DeserializeObject(body, type);
        }
    }

    //public class JsonModelBinder : IModelBinder
    //{


    //    private readonly MvcNewtonsoftJsonOptions _options;

    //    public JsonModelBinder(IOptions<MvcNewtonsoftJsonOptions> options) =>
    //        _options = options.Value;


    //    public Task BindModelAsync(ModelBindingContext bindingContext)
    //    {
    //        if (bindingContext is null)
    //            throw new ArgumentNullException(nameof(bindingContext));

    //        // Test if a value is received
    //        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);
    //        if (valueProviderResult == ValueProviderResult.None)
    //            return Task.CompletedTask;

    //        bindingContext.ModelState.SetModelValue(bindingContext.ModelName, valueProviderResult);

    //        // Get the json serialized value as string
    //        string serialized = valueProviderResult.FirstValue;

    //        // Return a successful binding for empty strings or nulls
    //        if (String.IsNullOrEmpty(serialized))
    //        {
    //            bindingContext.Result = ModelBindingResult.Success(null);
    //            return Task.CompletedTask;
    //        }

    //        // Deserialize json string using custom json options defined in startup, if available
    //        object deserialized = _options?.SerializerSettings is null ?
    //            JsonConvert.DeserializeObject(serialized, bindingContext.ModelType) :
    //            JsonConvert.DeserializeObject(serialized, bindingContext.ModelType, _options.SerializerSettings);

    //        // Run data annotation validation to validate properties and fields on deserialized model
    //        var validationResultProps = from property in TypeDescriptor.GetProperties(deserialized).Cast<PropertyDescriptor>()
    //                                    from attribute in property.Attributes.OfType<ValidationAttribute>()
    //                                    where !attribute.IsValid(property.GetValue(deserialized))
    //                                    select new
    //                                    {
    //                                        Member = property.Name,
    //                                        ErrorMessage = attribute.FormatErrorMessage(String.Empty)
    //                                    };

    //        var validationResultFields = from field in TypeDescriptor.GetReflectionType(deserialized).GetFields().Cast<FieldInfo>()
    //                                     from attribute in field.GetCustomAttributes<ValidationAttribute>()
    //                                     where !attribute.IsValid(field.GetValue(deserialized))
    //                                     select new
    //                                     {
    //                                         Member = field.Name,
    //                                         ErrorMessage = attribute.FormatErrorMessage(String.Empty)
    //                                     };

    //        // Add the validation results to the model state
    //        var errors = validationResultFields.Concat(validationResultProps);
    //        foreach (var validationResultItem in errors)
    //            bindingContext.ModelState.AddModelError(validationResultItem.Member, validationResultItem.ErrorMessage);

    //        // Set successful binding result
    //        bindingContext.Result = ModelBindingResult.Success(deserialized);

    //        return Task.CompletedTask;
    //    }
    //}

    public class JsonModelBinderProvider : IModelBinderProvider
    {
        public const string JsonContentType = "application/json";
        public readonly JsonSerializerSettings JsonSerializerSettings;
        public readonly JsonConverter[] JsonConverters;

        public JsonModelBinderProvider() { }

        public JsonModelBinderProvider(JsonSerializerSettings jsonSerializerSettings)
        {
            JsonSerializerSettings = jsonSerializerSettings;
        }

        public JsonModelBinderProvider(params JsonConverter[] jsonConverters)
        {
            JsonConverters = jsonConverters;
        }

        public JsonModelBinderProvider(JsonSerializerSettings jsonSerializerSettings, params JsonConverter[] jsonConverters)
        {
            JsonSerializerSettings = jsonSerializerSettings;
            JsonConverters = jsonConverters;
        }

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            var httpContextAccessor = context.Services.GetRequiredService<IHttpContextAccessor>();

            if (string.Equals(httpContextAccessor.HttpContext.Request.ContentType, JsonContentType))
            {
                return new JsonModelBinder(context.Services, JsonSerializerSettings, JsonConverters);
            }

            return null;
        }
    }

    //public class IdStringEmptyJsonConverter : JsonConverter
    //{
    //    public override bool CanConvert(Type objectType)
    //    {
    //        return objectType == typeof(long) || objectType == typeof(int);
    //    }

    //    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    //    {
    //        JToken jt = JValue.ReadFrom(reader);
    //        try
    //        {
    //            if (jt.Value<string>() == "")
    //            {
    //                return 0L;
    //            }
    //        }
    //        catch { }
    //        try
    //        {
    //            return jt.Value<long>();
    //        }
    //        catch { }
    //        return 0L;
    //    }

    //    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    //    {
    //        serializer.Serialize(writer, value);
    //    }
    //}
}
