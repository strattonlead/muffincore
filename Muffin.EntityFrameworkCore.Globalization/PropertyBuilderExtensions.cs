using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Linq.Expressions;

namespace Muffin.EntityFrameworkCore.Globalization
{
    //public static class Configuration
    //{
    //    public static string[] PropertyTrimming { get; set; } = new string[] { "Key", "Ref" };
    //    public static bool PropertyToLower { get; set; } = true;
    //}

    public static class PropertyBuilderExtensions
    {
        public static void LocalizedStringProperty<TEntity, TProperty>(this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> navigationExpression, Expression<Func<TEntity, object>> foreignKeyExpression)
                where TEntity : class
                where TProperty : class
        {
            builder.HasOne(navigationExpression).WithOne().HasForeignKey(foreignKeyExpression);
            builder.Navigation(navigationExpression).AutoInclude();
        }

        //    public static void LocalizedStringProperty<TEntity, TProperty>(this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> propertyExpression, Expression<Func<TEntity, object>> keyExpression)
        //        where TEntity : class
        //        where TProperty : class
        //    {
        //        var entityTypeName = typeof(TEntity).Name.Replace("Entity", "").ToLower();

        //        var expression = (MemberExpression)keyExpression.Body;
        //        var propertyName = expression.Member.Name;
        //        foreach (var item in Configuration.PropertyTrimming)
        //        {
        //            propertyName = propertyName.TrimEnd(item);
        //        }

        //        if (Configuration.PropertyToLower)
        //        {
        //            propertyName = propertyName.ToLower();
        //        }


        //        var keyProperties = typeof(TEntity).GetProperties().Where(x => x.GetCustomAttribute<KeyAttribute>() != null && x.GetCustomAttribute<NotMappedAttribute>() == null).ToArray();
        //        var keyPropertiesCount = Math.Max(keyProperties.Length, 1);
        //        var segmentLength = (450 - (entityTypeName.Length + propertyName.Length + 2)) / keyPropertiesCount;
        //        var keySegments = keyProperties.Select(x => $"CAST([{x.Name}] as nvarchar({segmentLength}))").ToArray();
        //        var keySegment = string.Join(". + ", keySegments);
        //        if (string.IsNullOrWhiteSpace(keySegment))
        //        {
        //            keySegment = $"CAST([Id] as nvarchar({segmentLength}))";
        //        }

        //        builder.Property(keyExpression)
        //            .HasColumnType("nvarchar(450)")
        //            .HasComputedColumnSql($"'{entityTypeName}.' + {keySegment} + '.{propertyName}'")
        //            .ValueGeneratedOnAddOrUpdate();

        //        if (propertyExpression != null)
        //        {
        //            builder.ComputedForeignKey(propertyExpression, keyExpression);
        //        }

        //        if (!EFBuild.IsMigration)
        //        {
        //            builder.Navigation(propertyExpression).AutoInclude();
        //        }
        //    }

        //    public static string TrimEnd(this string input, string suffixToRemove, StringComparison comparisonType = StringComparison.CurrentCulture)
        //    {
        //        if (suffixToRemove != null && input.EndsWith(suffixToRemove, comparisonType))
        //        {
        //            return input.Substring(0, input.Length - suffixToRemove.Length);
        //        }

        //        return input;
        //    }

        //    public static void ComputedForeignKey<TEntity, TProperty>(this EntityTypeBuilder<TEntity> builder, Expression<Func<TEntity, TProperty>> propertyExpression, Expression<Func<TEntity, object>> keyExpression)
        //        where TEntity : class
        //        where TProperty : class
        //    {
        //        //var expression = (MemberExpression)propertyExpression.Body;
        //        //builder.Ignore(expression.Member.Name);

        //        //builder.HasOne(propertyExpression).WithOne().HasForeignKey(keyExpression);
        //        if (!EFBuild.IsMigration)
        //        {
        //            builder.HasOne(propertyExpression).WithOne().HasForeignKey(keyExpression);
        //        }
        //        else
        //        {
        //            var expression = (MemberExpression)propertyExpression.Body;
        //            builder.Ignore(expression.Member.Name);
        //        }
        //    }
    }

    public class EFBuild
    {
        private static bool? _set;
        private static object _lock = new object();
        public static bool IsMigration
        {
            get
            {
                lock (_lock)
                {
                    if (_set.HasValue)
                    {
                        return _set.Value;
                    }
                    _set = false;
                    //CheckSymbol();
                    return _set.Value;
                }
            }
            set
            {
                lock (_lock)
                {
                    if (_set.HasValue)
                    {
                        return;
                    }
                    _set = value;
                }
            }
        }

        //public static void CheckSymbol()
        //{
        //    _set = SymbolProvider.Current?.Symbols?.Contains("EF");
        //}
    }

    //public class SymbolProvider
    //{
    //    public static SymbolProvider Current = new SymbolProvider();
    //    public readonly string[] Symbols;
    //    public SymbolProvider() { Symbols = new string[0]; }
    //    public SymbolProvider(params string[] symbols)
    //    {
    //        Symbols = symbols?.ToArray() ?? new string[0];
    //    }
    //}

    //public static class SymbolProviderHelper
    //{
    //    public static void AddSymbolProvider(this IServiceCollection services, params string[] symbols)
    //    {
    //        SymbolProvider.Current = new SymbolProvider(symbols);
    //        services.AddSingleton(SymbolProvider.Current);
    //    }
    //}
}
