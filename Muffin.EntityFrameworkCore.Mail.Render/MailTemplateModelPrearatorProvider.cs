using Microsoft.Extensions.DependencyInjection;
using Muffin.Mail.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Muffin.EntityFrameworkCore.Mail.Renderer
{
    public class MailTemplateModelPrearatorProvider : IMailTemplateModelPrearatorProvider
    {
        #region Properties

        private readonly IServiceProvider ServiceProvider;
        private readonly MailTemplateModelPrearatorProviderOptions Options;

        #endregion

        #region Constructor

        public MailTemplateModelPrearatorProvider(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Options = serviceProvider.GetService<MailTemplateModelPrearatorProviderOptions>();
        }

        #endregion

        #region IMailTemplateModelPrearatorProvider

        public MailTemplateModelContext<T> GetMailTemplateModelContext<T>(T model)
        {
            var context = ServiceProvider.GetService<MailTemplateModelContext<T>>();
            if (context != null)
            {
                context.Model = model;
            }
            return context;
        }

        public MailTemplateModelContext GetMailTemplateModelContextDetermineConcreteType(object model)
        {
            var baseType = typeof(MailTemplateModelContext<>);
            var modelType = model.GetType();
            var concreteType = baseType.MakeGenericType(modelType);

            var context = (MailTemplateModelContext)ServiceProvider.GetService(concreteType);
            if (context != null)
            {
                context.SetModel(model);
            }
            return context;
        }

        public IMailTemplateModelPrearator<T> GetMailTemplateModelPrearator<T>()
        {
            return ServiceProvider.GetService<IMailTemplateModelPrearator<T>>();
        }

        public IMailTemplateModelPrearator GetMailTemplateModelPrearator(object model)
        {
            var baseType = typeof(IMailTemplateModelPrearator<>);
            var modelType = model.GetType();
            var concreteType = baseType.MakeGenericType(modelType);

            return (IMailTemplateModelPrearator)ServiceProvider.GetService(concreteType);
        }

        #endregion
    }

    public class MailTemplateModelPrearatorProviderOptions
    {
        public List<Assembly> Assemblies { get; set; } = new List<Assembly>();
    }

    public class MailTemplateModelPrearatorProviderOptionsBuilder
    {
        private MailTemplateModelPrearatorProviderOptions Options = new MailTemplateModelPrearatorProviderOptions();

        public void AddAssembly(Assembly assembly)
        {
            Options.Assemblies.Add(assembly);
        }

        public void AddAssemblies(IEnumerable<Assembly> assemblies)
        {
            Options.Assemblies.AddRange(assemblies);
        }

        public MailTemplateModelPrearatorProviderOptions Build()
        {
            return Options;
        }
    }

    public static class MailTemplateModelPrearatorProviderExtensions
    {
        public static void AddMailTemplateModelPrearatorProvider(this IServiceCollection services, Action<MailTemplateModelPrearatorProviderOptionsBuilder> builder)
        {
            if (builder != null)
            {
                var optionsBuilder = new MailTemplateModelPrearatorProviderOptionsBuilder();
                builder.Invoke(optionsBuilder);
                var options = optionsBuilder.Build();
                services.AddSingleton(options);

                var interfaceType = typeof(IMailTemplateModelPrearator);
                var types = options
                    .Assemblies
                    .SelectMany(x => x.GetTypes())
                    .Where(x => !x.IsAbstract && !x.IsInterface && interfaceType.IsAssignableFrom(x))
                    .ToArray();

                var baseContextType = typeof(MailTemplateModelContext<>);
                var basePreparaorType = typeof(IMailTemplateModelPrearator<>);
                foreach (var type in types)
                {
                    var entityType = type.GetInterfaces().FirstOrDefault(x => interfaceType.IsAssignableFrom(x))?.GetGenericArguments()?.FirstOrDefault();

                    var contextType = baseContextType.MakeGenericType(entityType);
                    var preparaorType = basePreparaorType.MakeGenericType(entityType);

                    services.AddScoped(contextType);
                    services.AddScoped(preparaorType, type);
                }
            }

            services.AddScoped<IMailTemplateModelPrearatorProvider, MailTemplateModelPrearatorProvider>();
        }
    }
}
