//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Reflection;
//using System.Text;

//namespace Muffin.StateManagement
//{
//    public class StateMergeTypeConfigurationProvider : IStateMergeTypeConfigurationProvider
//    {
//        #region Properties

//        private readonly StateMergeTypeConfigurationProviderConfiguration Configuration;

//        private static object _lock = new object();
//        private static Dictionary<Type, IStateMergeTypeConfiguration> Types;

//        #endregion

//        #region Constructor

//        public StateMergeTypeConfigurationProvider(IServiceProvider serviceProvider)
//        {
//            Configuration = serviceProvider.GetRequiredService<StateMergeTypeConfigurationProviderConfiguration>();

//            lock (_lock)
//            {
//                if (Types == null)
//                {
//                    var configurationTypes = StateMergeTypeConfigurationProviderExtensions.GetConfigurationTypes(Configuration.Assembly);

//                    Types = configurationTypes.ToDictionary(x => x.GetInterfaces().FirstOrDefault(x => typeof(IStateMergeTypeConfiguration).IsAssignableFrom(x) && x.IsGenericType).GetGenericArguments().First(), x => (IStateMergeTypeConfiguration)Activator.CreateInstance(x));
//                }

//            }
//        }

//        #endregion

//        #region IStateMergeTypeConfigurationProvider

//        public IStateMergeTypeConfiguration GetStateMergeTypeConfiguration(Type type)
//        {
//            if (Types.TryGetValue(type, out IStateMergeTypeConfiguration result))
//            {
//                return result;
//            }
//            return null;
//        }

//        public IStateMergeTypeConfiguration<T, TAppState, TStateMergeContext> GetStateMergeTypeConfiguration<T, TAppState, TStateMergeContext>()
//            where TStateMergeContext : IStateMergeContext<T>
//        { 
//            return (IStateMergeTypeConfiguration<T, TAppState, TStateMergeContext>)GetStateMergeTypeConfiguration(typeof(T));
//        }

//        #endregion
//    }

//    public static class StateMergeTypeConfigurationProviderExtensions
//    {
//        public static Type[] GetConfigurationTypes(Assembly assembly)
//        {
//            return assembly.GetTypes()
//                        .Where(x => !x.IsInterface && !x.IsAbstract && typeof(IStateMergeTypeConfiguration).IsAssignableFrom(x))
//                        .ToArray();
//        }

//        public static void AddStateMergeTypeConfigurationProvider<TAppState>(this IServiceCollection services, Assembly assembly)
//        {
//            services.AddStateMergeTypeConfigurations(assembly);
//            services.AddScoped<IStateMergeTypeConfigurationProvider, StateMergeTypeConfigurationProvider>();

//            var configurationTypes = GetConfigurationTypes(assembly);
//            foreach (var configurationType in configurationTypes)
//            {
//                var type = typeof(IStateMergeContext<>);
//                var interfaceType = configurationType.GetInterfaces().FirstOrDefault(x => typeof(IStateMergeTypeConfiguration).IsAssignableFrom(x) && x.IsGenericType);
//                var genericType = type.MakeGenericType(new Type[] { interfaceType.GetGenericArguments().First(), typeof(TAppState) });
//                services.AddScoped(genericType);
//            }

//        }
//    }
//}
