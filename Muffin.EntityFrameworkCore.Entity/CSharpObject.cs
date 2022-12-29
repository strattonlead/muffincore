using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Newtonsoft.Json;
using System;

namespace Muffin.EntityFrameworkCore.Entity
{
    public class CSharpObject
    {
        public string AssemblyQualifiedName { get; set; }

        public string JsonData { get; set; }

        [JsonIgnore]
        public Type Type
        {
            get
            {
                try
                {
                    return Type.GetType(AssemblyQualifiedName);
                }
                catch { }
                return null;
            }
        }

        public void SetObject(object obj)
        {
            if (obj == null)
            {
                JsonData = null;
                AssemblyQualifiedName = null;
                return;
            }

            JsonData = JsonConvert.SerializeObject(obj);
            AssemblyQualifiedName = obj.GetType().AssemblyQualifiedName;
        }

        public object GetObject()
        {
            if (AssemblyQualifiedName == null || JsonData == null)
            {
                return null;
            }

            try
            {
                return JsonConvert.DeserializeObject(JsonData, Type);
            }
            catch { }
            return null;
        }
    }

    public static class CSharpObjectExtensions
    {
        public static PropertyBuilder<TProperty> HasJsonConversion<TProperty>(this PropertyBuilder<TProperty> builder)
            where TProperty : class
        {
            return builder.HasConversion(x => JsonConvert.SerializeObject(x), x => x != null ? JsonConvert.DeserializeObject<TProperty>(x) : (TProperty)null);
        }
    }
}
