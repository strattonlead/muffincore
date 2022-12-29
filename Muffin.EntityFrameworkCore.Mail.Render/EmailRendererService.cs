using Microsoft.Extensions.DependencyInjection;
using Muffin.Mail.Abstraction;
using Muffin.Services.Razor;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Muffin.EntityFrameworkCore.Mail.Render
{
    public class EmailRendererService
    {
        #region Properties

        private readonly RazorLightRenderer RazorLightRenderer;
        private readonly IMailTemplateModelPrearatorProvider MailTemplateModelPrearatorProvider;

        #endregion

        #region Constructor

        public EmailRendererService(IServiceProvider serviceProvider)
        {
            RazorLightRenderer = serviceProvider.GetRequiredService<RazorLightRenderer>();
            MailTemplateModelPrearatorProvider = serviceProvider.GetService<IMailTemplateModelPrearatorProvider>();
        }

        #endregion

        #region Helper

        public async Task<RenderResult> RenderAsync<T>(IMailTemplate mailTemplate, string languageCode, T model)
        {
            if (mailTemplate == null)
            {
                return RenderResult.Fail(new ArgumentException("Parameter mailTemplate is null"));
            }

            if (mailTemplate.Subject == null)
            {
                return RenderResult.Fail(new ArgumentException("Parameter Subject is null"));
            }

            if (mailTemplate.Body == null)
            {
                return RenderResult.Fail(new ArgumentException("Parameter Body is null"));
            }

            if (mailTemplate.Subject.LocalizedStringValues == null)
            {
                return RenderResult.Fail(new ArgumentException("Parameter Subject.LocalizedStringValues is null"));
            }

            if (mailTemplate.Body.LocalizedStringValues == null)
            {
                return RenderResult.Fail(new ArgumentException("Parameter Body.LocalizedStringValues is null"));
            }

            if (!mailTemplate.Subject.LocalizedStringValues.Any(x => x.LanguageId == languageCode))
            {
                return RenderResult.Fail(new ArgumentException($"Subject does not contain language code {languageCode}"));
            }

            if (!mailTemplate.Body.LocalizedStringValues.Any(x => x.LanguageId == languageCode))
            {
                return RenderResult.Fail(new ArgumentException($"Body does not contain language code {languageCode}"));
            }

            var subject = mailTemplate.Subject.LocalizedStringValues.FirstOrDefault(x => x.LanguageId == languageCode).Value;
            var body = mailTemplate.Body.LocalizedStringValues.FirstOrDefault(x => x.LanguageId == languageCode).Value;

            if (string.IsNullOrWhiteSpace(subject))
            {
                return RenderResult.Fail(new ArgumentException("Subject is null"));
            }

            if (string.IsNullOrWhiteSpace(body))
            {
                return RenderResult.Fail(new ArgumentException("Body is null"));
            }

            try
            {
                IMailTemplateModelPrearator preparator = MailTemplateModelPrearatorProvider?.GetMailTemplateModelPrearator(model);
                MailTemplateModelContext context;
                if (typeof(T) == typeof(object))
                {
                    context = MailTemplateModelPrearatorProvider.GetMailTemplateModelContextDetermineConcreteType(model);
                }
                else
                {
                    context = MailTemplateModelPrearatorProvider.GetMailTemplateModelContext(model);
                }

                if (preparator != null && context != null)
                {
                    preparator.Prepare(context);
                }
            }
            catch (Exception ex)
            {
                return RenderResult.Fail(new Exception("Unable to prepare model", ex));
            }

            try
            {
                subject = await RazorLightRenderer.RenderAsync(new string(subject), model);
            }
            catch (Exception ex)
            {
                return RenderResult.Fail(new Exception("Unable to render Subject", ex));
            }

            try
            {
                body = await RazorLightRenderer.RenderAsync(new string(body), model);
            }
            catch (Exception ex)
            {
                return RenderResult.Fail(new Exception("Unable to render Body", ex));
            }

            return new RenderResult()
            {
                LanguageRef = languageCode,
                TemplateId = mailTemplate.Id,
                Subject = subject,
                Body = body
            };
        }

        #endregion
    }

    public struct RenderResult
    {
        public string LanguageRef { get; set; }
        public long TemplateId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool HasError => Error != null;
        public Exception Error { get; set; }

        public static RenderResult Fail(Exception e)
        {
            return new RenderResult()
            {
                Error = e
            };
        }
    }

    public static class EmailRendererServiceExtensions
    {
        public static void AddEmailRendererService(this IServiceCollection services)
        {
            services.AddScoped<EmailRendererService>();
        }
    }
}
