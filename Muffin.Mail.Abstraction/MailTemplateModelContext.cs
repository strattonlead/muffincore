using System;
using System.Collections.Generic;

namespace Muffin.Mail.Abstraction
{
    public abstract class MailTemplateModelContext
    {
        public IServiceProvider ServiceProvider { get; set; }

        private Dictionary<Type, object> _services = new Dictionary<Type, object>();
        public MailTemplateModelContext(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public TService GetService<TService>()
        {
            if (_services.TryGetValue(typeof(TService), out var service))
            {
                return (TService)service;
            }

            service = ServiceProvider.GetService(typeof(TService));
            if (service != null)
            {
                _services[typeof(TService)] = service;
            }

            return (TService)service;
        }

        public abstract void SetModel(object model);
    }
    public class MailTemplateModelContext<T> : MailTemplateModelContext
    {
        public T Model { get; set; }

        public MailTemplateModelContext(IServiceProvider serviceProvider)
        : base(serviceProvider) { }

        public override void SetModel(object model)
        {
            Model = (T)model;
        }
    }
}
