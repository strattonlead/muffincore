//using Microsoft.Extensions.DependencyInjection;
//using System;
//using System.Reflection;

//namespace Muffin.StateManagement
//{
//    public interface IStateMergeTypeConfigurationProvider
//    {
//        IStateMergeTypeConfiguration GetStateMergeTypeConfiguration(Type type);
//        IStateMergeTypeConfiguration<T, TAppState, TStateMergeContext> GetStateMergeTypeConfiguration<T, TAppState, TStateMergeContext>()
//            where TStateMergeContext : IStateMergeContext<T>;
//    }

//    public class StateMergeTypeConfigurationProviderConfiguration
//    {
//        public Assembly Assembly { get; set; }
//    }

//    public interface IStateMergeTypeConfiguration { }
//    public interface IStateMergeTypeConfiguration<T, TAppState, TStateMergeContext> : IStateMergeTypeConfiguration
//        where TStateMergeContext : IStateMergeContext<T>
//    {
//        public void Merge(TStateMergeContext context, ref TAppState appState, out PartialAppState partialAppState);
//    }

//    public interface IStateMergeContext<T>
//    {
//        T Object { get; set; }
//    }

//    //public class StateMergeContext<T> : IStateMergeContext<T>
//    //{
//    //    public readonly ITranslationManager TranslationManager;
//    //    public readonly ISelectListDataProvider SelectListDataProvider;
//    //    public readonly ISearchMaskDataProvider SearchMaskDataProvider;
//    //    public readonly IFormDataProvider FormDataProvider;
//    //    public readonly IViewTemplateProvider ViewTemplateProvider;

//    //    public T Object { get; set; }

//    //    public StateMergeContext(IServiceProvider serviceProvider)
//    //    {
//    //        TranslationManager = serviceProvider.GetRequiredService<ITranslationManager>();
//    //        SelectListDataProvider = serviceProvider.GetRequiredService<ISelectListDataProvider>();
//    //        SearchMaskDataProvider = serviceProvider.GetRequiredService<ISearchMaskDataProvider>();
//    //        FormDataProvider = serviceProvider.GetRequiredService<IFormDataProvider>();
//    //        ViewTemplateProvider = serviceProvider.GetRequiredService<IViewTemplateProvider>();
//    //    }
//    //}

//    public static class StateMergeTypeConfigurationExtensions
//    {
//        public static void AddStateMergeTypeConfigurations(this IServiceCollection services, Assembly assembly)
//        {
//            var configuration = new StateMergeTypeConfigurationProviderConfiguration() { Assembly = assembly };
//            services.AddSingleton(configuration);
//        }
//    }
//}
