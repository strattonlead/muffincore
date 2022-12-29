using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Muffin.EntityFrameworkCore.Entity;
using Muffin.Tenancy.Abstraction;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Muffin.EntityFrameworkCore.Globalization
{
    [JsonConverter(typeof(LocalizedStringEntityJsonConverter))]
    [Index(nameof(KeyPath), IsUnique = true)]
    public class LocalizedStringEntity : BaseEntity, ITenantOwned
    {
        #region Properties

        //[Required]
        //[Key]
        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        //public virtual string Key { get; set; }

        //[NotMapped]
        //public override long Id { get; set; }

        public string KeyPath { get; set; }
        //public string Description { get; set; }
        public long? TenantId { get; set; }


        public ICollection<LocalizedStringValueEntity> LocalizedStringValues { get; set; }

        #endregion

        #region Constructor

        public LocalizedStringEntity() { }

        #endregion

        #region Helper

        public static LocalizedStringEntity FromDictionary(IDictionary<string, string> dictionary)
        {
            var localizedString = new LocalizedStringEntity();
            localizedString.LocalizedStringValues = dictionary.Select(x => new LocalizedStringValueEntity()
            {
                LanguageId = x.Key,
                Value = x.Value,
                LocalizedString = localizedString
            }).ToArray();

            return localizedString;
        }

        public override string ToString()
        {
            if (!string.IsNullOrWhiteSpace(KeyPath))
            {
                return KeyPath;
            }

            return Id.ToString();
        }

        public string this[string langCode]
        {
            get
            {
                return LocalizedStringValues?.FirstOrDefault(x => x.LanguageId == langCode)?.Value;
            }
            set
            {
                if (LocalizedStringValues == null)
                {
                    LocalizedStringValues = new List<LocalizedStringValueEntity>();
                }

                var item = LocalizedStringValues.FirstOrDefault(x => x.LanguageId == langCode);
                if (item == null)
                {
                    item = new LocalizedStringValueEntity()
                    {
                        LanguageId = langCode,
                        LocalizedStringId = Id
                    };
                }

                item.Value = value;
            }
        }

        public void SetFromDictionary(IDictionary<string, string> values)
        {
            if (values == null)
            {
                return;
            }

            if (LocalizedStringValues == null)
            {
                LocalizedStringValues = new List<LocalizedStringValueEntity>();
            }

            var fastValues = LocalizedStringValues.ToDictionary(x => x.LanguageId);
            foreach (var kvp in values)
            {
                if (!fastValues.TryGetValue(kvp.Key, out var value))
                {
                    value = new LocalizedStringValueEntity()
                    {
                        LanguageId = kvp.Key
                    };
                    LocalizedStringValues.Add(value);
                }

                value.Value = kvp.Value;
            }
        }

        public void SetFromLocalizedString(LocalizedStringEntity localizedString)
        {
            var dict = localizedString?.ToDictionary();
            if (dict == null)
            {
                return;
            }

            SetFromDictionary(dict);
        }

        public bool? AllNullOrWhitespace()
        {
            if (LocalizedStringValues == null)
            {
                return null;
            }

            if (LocalizedStringValues.Count == 0)
            {
                return false;
            }

            return LocalizedStringValues.Select(x => x.Value).All(x => string.IsNullOrWhiteSpace(x));
        }

        public bool? HasAnyEntry()
        {
            if (LocalizedStringValues == null)
            {
                return null;
            }

            return LocalizedStringValues.Any();
        }

        public Dictionary<string, string> ToDictionary()
        {
            if (LocalizedStringValues == null)
            {
                return null;
            }

            return LocalizedStringValues.GroupBy(x => x.LanguageId).Select(x => x.First()).ToDictionary(x => x.LanguageId, x => x.Value);
        }

        #endregion
    }

    public class LocalizedStringEntityTypeConfiguration : IEntityTypeConfiguration<LocalizedStringEntity>
    {
        public void Configure(EntityTypeBuilder<LocalizedStringEntity> builder)
        {
            builder.HasMany(x => x.LocalizedStringValues)
                .WithOne(x => x.LocalizedString)
                .HasForeignKey(x => x.LocalizedStringId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Navigation(x => x.LocalizedStringValues).AutoInclude();
        }
    }

    public class LocalizedStringEntityJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(LocalizedStringEntity) == objectType;
        }

        public override bool CanRead => true;

        public override bool CanWrite => true;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            try
            {
                var dictionary = serializer.Deserialize<Dictionary<string, string>>(reader);
                var localizedString = LocalizedStringEntity.FromDictionary(dictionary);
                return localizedString;
            }
            catch { }
            return null;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteValue(value?.ToString());
        }
    }

    public static class LocalizedStringEntityHelper
    {
        //public static T CleanEmptyLocalizationKeys<T>(this T baseEntity)
        //    where T : BaseEntity
        //{
        //    if (baseEntity == null)
        //    {
        //        return null;
        //    }

        //    var localizedStringProperties = baseEntity
        //        .GetType()
        //        .GetProperties()
        //        .Where(x => x.PropertyType == typeof(LocalizedStringEntity) && x.GetCustomAttribute<DbAfterSaveIgnoreAttribute>() == null)
        //        .ToArray();

        //    foreach (var localizedStringProperty in localizedStringProperties)
        //    {
        //        var localizedString = localizedStringProperty.GetValue(baseEntity) as LocalizedStringEntity;
        //        if (localizedString != null && string.IsNullOrWhiteSpace(localizedString.Key))
        //        {
        //            localizedStringProperty.SetValue(baseEntity, null);

        //            if (baseEntity.LocalizedStrings == null)
        //            {
        //                baseEntity.LocalizedStrings = new Dictionary<string, Dictionary<string, string>>();
        //            }

        //            if (localizedString.LocalizedStringValues != null)
        //            {
        //                baseEntity.LocalizedStrings[localizedStringProperty.Name.ToLower()] = localizedString.LocalizedStringValues.ToDictionary(x => x.LanguageId, x => x.Value);
        //            }
        //        }
        //    }

        //    return baseEntity;
        //}
    }

    public class LocalizedStringConsumerJsonConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            //return objectType
            //    .GetProperties()
            //    .Where(x => x.PropertyType == typeof(LocalizedStringEntity) && x.GetCustomAttribute<DbAfterSaveIgnoreAttribute>() == null)
            //    .Any();
            return true;
        }

        public override bool CanRead => true;

        public override bool CanWrite => false;

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = Activator.CreateInstance(objectType);
            serializer.Populate(reader, obj);
            //(obj as BaseEntity)?.CleanEmptyLocalizationKeys();
            return obj;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }

    public static class ILocalizableEntityExtensions
    {
        public static TSource AddLocalization<TSource, TMember>(this TSource source, Expression<Func<TSource, TMember>> predicate, string languageKey, string value)
            where TSource : ILocalizableEntity
        {
            return source.AddLocalizations(predicate, new Dictionary<string, string>() { { languageKey, value } });
        }

        public static TSource AddLocalizations<TSource, TMember>(this TSource source, Expression<Func<TSource, TMember>> predicate, Dictionary<string, string> values)
            where TSource : ILocalizableEntity
        {
            var propertyInfo = predicate.GetPropertyAccess();
            if (propertyInfo == null)
            {
                throw new ArgumentException("Expression must be a member Expression");
            }

            var localizedString = (LocalizedStringEntity)propertyInfo.GetValue(source, null);
            if (localizedString == null)
            {
                localizedString = new LocalizedStringEntity();
                propertyInfo.SetValue(source, localizedString);
            }

            localizedString.SetFromDictionary(values);

            return source;
        }
    }
}
