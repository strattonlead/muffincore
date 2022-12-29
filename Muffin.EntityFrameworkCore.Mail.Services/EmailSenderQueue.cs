using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Muffin.EntityFrameworkCore.Mail.Render;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Muffin.EntityFrameworkCore.Mail.Services
{
    public class EmailSenderQueue<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>
            where TContext : DbContext, IMailDbContext<TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>
            where TSendMailJob : class, ISendMailJob
            where TSmtpCredential : class, ISmtpCredential
            where TMailTemplate : class, IMailTemplate
            where TSendMailJobRecipient : class, ISendMailJobRecipient
            where TSendMailJobError : class, ISendMailJobError
    {
        #region Properties

        private readonly TContext DbContext;
        private readonly EmailRendererService EmailRendererService;
        private readonly EmailSenderQueueEvents Events;

        #endregion

        #region Constructor

        public EmailSenderQueue(IServiceProvider serviceProvider)
        {
            DbContext = serviceProvider.GetRequiredService<TContext>();
            EmailRendererService = serviceProvider.GetRequiredService<EmailRendererService>();
            Events = serviceProvider.GetRequiredService<EmailSenderQueueEvents>();
        }

        #endregion

        #region Queue

        public async Task<TSendMailJob> EnqueueEmail<T>(string recipient, long smtpCredentialId, long mailTemplateId, string languageRef, T model)
        {
            return await EnqueueEmail(new string[] { recipient }, smtpCredentialId, mailTemplateId, languageRef, model);
        }

        public async Task<TSendMailJob> EnqueueEmail<T>(IEnumerable<string> recipients, long smtpCredentialId, long mailTemplateId, string languageRef, T model)
        {
            var smtpCredential = DbContext.SmtpCredentials.Find(smtpCredentialId);
            if (smtpCredential == null)
            {
                Events.InvokeOnError(this, smtpCredential, new Exception($"No credential for id {smtpCredentialId}"));
                return null;
            }

            var mailTemplate = DbContext.MailTemplates.Find(mailTemplateId);
            if (mailTemplate == null)
            {
                Events.InvokeOnError(this, smtpCredential, new Exception($"No mail template for id {mailTemplateId}"));
                return null;
            }

            var renderResult = await EmailRendererService.RenderAsync(mailTemplate, languageRef, model);
            if (renderResult.HasError)
            {
                Events.InvokeOnError(this, smtpCredential, renderResult.Error);
                return null;
            }

            return EnqueueEmail(recipients, renderResult.Subject, renderResult.Body, smtpCredential, mailTemplate);
        }

        public TSendMailJob EnqueueEmail(string recipient, string subject, string body, TSmtpCredential smtpCredential)
        {
            return EnqueueEmail(new string[] { recipient }, subject, body, smtpCredential);
        }

        public TSendMailJob EnqueueEmail(IEnumerable<string> recipients, string subject, string body, TSmtpCredential smtpCredential)
        {
            return EnqueueEmail(recipients, subject, body, smtpCredential, null);
        }

        private TSendMailJob EnqueueEmail(IEnumerable<string> recipients, string subject, string body, TSmtpCredential smtpCredential, TMailTemplate mailTemplate)
        {
            var sendMailJob = Activator.CreateInstance<TSendMailJob>();
            sendMailJob.Subject = subject;
            sendMailJob.Body = body;
            sendMailJob.MailTemplateId = mailTemplate.Id;
            sendMailJob.SmtpCredentialId = smtpCredential.Id;

            var sendMailJobRecipients = recipients.Select(x =>
            {
                var temp = Activator.CreateInstance<TSendMailJobRecipient>();
                temp.Recipient = x;
                return temp;
            }).ToList();

            DbContext.Add(sendMailJob);
            DbContext.AddRange(sendMailJobRecipients);
            DbContext.SaveChanges();
            Events.InvokeOnEnqueue(this, smtpCredential, mailTemplate, sendMailJob);
            return sendMailJob;
        }

        #endregion
    }
}