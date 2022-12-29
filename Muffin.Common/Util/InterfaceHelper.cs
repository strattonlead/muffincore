using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Muffin.Common.Util
{
    public static class InterfaceHelper
    {
        private static ConcurrentDictionary<Type, Type> _typeMappings = new ConcurrentDictionary<Type, Type>();

        public static Dictionary<Type, Type> GetTypeMappings()
        {
            return new Dictionary<Type, Type>(_typeMappings);
        }

        public static void InitializeDeserializableInterfaces(Assembly assembly)
        {
            assembly.GetTypes()
                .Where(x => x.IsInterface && x.HasAttribute<DeserializableInterfaceAttribute>())
                .ForEach(x => GetImplementationType(x));
        }

        public static void InitializeDeserializableInterfaces(Assembly assembly, Assembly implementationAssembly)
        {
            var interfaceTypes = assembly.GetTypes()
                .Where(x => x.IsInterface && x.HasAttribute<DeserializableInterfaceAttribute>())
                .ToArray();

            var implementatinoTypes = implementationAssembly.GetTypes()
                .Where(x => x.GetInterfaces().Any(y => interfaceTypes.Contains(y)))
                .ToArray();

            interfaceTypes.ForEach(x => GetImplementationType(x, implementatinoTypes));
        }

        public static Type GetImplementationType<T>()
        {
            return GetImplementationType(typeof(T));
        }

        public static Type GetImplementationType(Type t, Type[] implementationTypes = null)
        {
            if (!t.IsInterface)
                return t;

            var safeDto = _typeMappings.GetOrAdd(t, type =>
            {
                if (implementationTypes != null)
                {
                    var implementationType = implementationTypes.FirstOrDefault(x => x.GetInterfaces().Contains(t));
                    if (implementationType != null)
                    {
                        return implementationType;
                    }
                }

                var assemblyName = new AssemblyName($"DynamicAssembly_{Guid.NewGuid():N}");

                var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicModule");
                var typeName = $"{type.Name}_{Guid.NewGuid():N}";
                var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);

                typeBuilder.AddInterfaceImplementation(type);

                var properties = type.GetPropertiesAndInterfaceProperties();

                BuildProperties(typeBuilder, properties);

                return typeBuilder.CreateTypeInfo();
            });

            return safeDto;
        }

        public static object CreateInstance(Type type)
        {
            var safeDto = GetImplementationType(type);
            return Activator.CreateInstance(safeDto);
        }

        public static T CreateInstance<T>()
        {
            var safeDto = GetImplementationType<T>();
            return (T)Activator.CreateInstance(safeDto);
        }

        private static void BuildProperties(TypeBuilder typeBuilder, IEnumerable<PropertyInfo> properties)
        {
            foreach (var property in properties)
            {
                BuildProperty(typeBuilder, property);
            }
        }

        private static PropertyBuilder BuildProperty(TypeBuilder typeBuilder, PropertyInfo property)
        {
            var fieldName = $"<{property.Name}>k__BackingField";

            var propertyBuilder = typeBuilder.DefineProperty(property.Name, System.Reflection.PropertyAttributes.None, property.PropertyType, Type.EmptyTypes);

            // Build backing-field.
            var fieldBuilder = typeBuilder.DefineField(fieldName, property.PropertyType, FieldAttributes.Private);

            var getSetAttributes = MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Virtual;

            var getterBuilder = BuildGetter(typeBuilder, property, fieldBuilder, getSetAttributes);
            var setterBuilder = BuildSetter(typeBuilder, property, fieldBuilder, getSetAttributes);

            propertyBuilder.SetGetMethod(getterBuilder);
            propertyBuilder.SetSetMethod(setterBuilder);

            return propertyBuilder;
        }

        private static MethodBuilder BuildGetter(TypeBuilder typeBuilder, PropertyInfo property, FieldBuilder fieldBuilder, MethodAttributes attributes)
        {
            var getterBuilder = typeBuilder.DefineMethod($"get_{property.Name}", attributes, property.PropertyType, Type.EmptyTypes);
            var ilGenerator = getterBuilder.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldfld, fieldBuilder);

            if (property.GetCustomAttribute<PropertyNotNullAttribute>() != null)
            {
                // Build null check
                ilGenerator.Emit(OpCodes.Dup);

                var isFieldNull = ilGenerator.DefineLabel();
                ilGenerator.Emit(OpCodes.Brtrue_S, isFieldNull);
                ilGenerator.Emit(OpCodes.Pop);
                ilGenerator.Emit(OpCodes.Ldstr, $"{property.Name} isn't set.");

                var invalidOperationExceptionConstructor = typeof(InvalidOperationException).GetConstructor(new Type[] { typeof(string) });
                ilGenerator.Emit(OpCodes.Newobj, invalidOperationExceptionConstructor);
                ilGenerator.Emit(OpCodes.Throw);

                ilGenerator.MarkLabel(isFieldNull);
            }
            ilGenerator.Emit(OpCodes.Ret);

            return getterBuilder;
        }

        private static MethodBuilder BuildSetter(TypeBuilder typeBuilder, PropertyInfo property, FieldBuilder fieldBuilder, MethodAttributes attributes)
        {
            var setterBuilder = typeBuilder.DefineMethod($"set_{property.Name}", attributes, null, new Type[] { property.PropertyType });
            var ilGenerator = setterBuilder.GetILGenerator();

            ilGenerator.Emit(OpCodes.Ldarg_0);
            ilGenerator.Emit(OpCodes.Ldarg_1);

            // Build null check

            if (property.GetCustomAttribute<PropertyNotNullAttribute>() != null)
            {
                var isValueNull = ilGenerator.DefineLabel();

                ilGenerator.Emit(OpCodes.Dup);
                ilGenerator.Emit(OpCodes.Brtrue_S, isValueNull);
                ilGenerator.Emit(OpCodes.Pop);
                ilGenerator.Emit(OpCodes.Ldstr, property.Name);

                var argumentNullExceptionConstructor = typeof(ArgumentNullException).GetConstructor(new Type[] { typeof(string) });
                ilGenerator.Emit(OpCodes.Newobj, argumentNullExceptionConstructor);
                ilGenerator.Emit(OpCodes.Throw);

                ilGenerator.MarkLabel(isValueNull);
            }
            ilGenerator.Emit(OpCodes.Stfld, fieldBuilder);
            ilGenerator.Emit(OpCodes.Ret);

            return setterBuilder;
        }
    }

    public class PropertyNotNullAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class DeserializableInterfaceAttribute : Attribute { }

    public static class DeserializableInterfaceJsonHelper
    {
        public static JsonSerializerSettings GetInterfaceJsonSerializerSettings()
        {
            var converters = InterfaceHelper.GetTypeMappings()
                .Select(x => new InterfaceConverter(x.Value, x.Key))
                .Cast<JsonConverter>()
                .ToList();
            return new JsonSerializerSettings
            {
                Converters = converters
            };
        }
    }

    public class InterfaceConverter : JsonConverter
    {
        public Type RealType { get; set; }
        public Type InterfaceType { get; set; }

        public InterfaceConverter(Type realType, Type interfaceType)
        {
            RealType = realType;
            InterfaceType = interfaceType;
        }

        public override bool CanConvert(Type objectType)
            => objectType == InterfaceType;

        public override object ReadJson(JsonReader reader, Type type, object value, JsonSerializer jser)
            => jser.Deserialize(reader, RealType);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer jser)
            => jser.Serialize(writer, value);
    }

    public class InterfaceConverter<TReal, TInterface> : InterfaceConverter
    {
        public InterfaceConverter()
            : base(typeof(TReal), typeof(TInterface)) { }

        public override bool CanConvert(Type objectType)
            => objectType == typeof(TInterface);

        public override object ReadJson(JsonReader reader, Type type, object value, JsonSerializer jser)
            => jser.Deserialize<TReal>(reader);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer jser)
            => jser.Serialize(writer, value);
    }

    public static class PropertyHelperExt
    {
        static public IEnumerable<PropertyInfo> GetPropertiesAndInterfaceProperties(this Type type, BindingFlags bindingAttr = BindingFlags.Public | BindingFlags.Instance)
        {
            if (!type.IsInterface)
            {
                return type.GetProperties(bindingAttr);
            }

            return type.GetInterfaces().Union(new Type[] { type }).SelectMany(i => i.GetProperties(bindingAttr)).Distinct();
        }
    }
}
