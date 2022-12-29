using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.EntityFrameworkCore.Mail.Services
{
    public class EmailSender<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>
        where TContext : DbContext, IMailDbContext<TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>
        where TSendMailJob : class, ISendMailJob
        where TSmtpCredential : class, ISmtpCredential
        where TMailTemplate : class, IMailTemplate
        where TSendMailJobRecipient : class, ISendMailJobRecipient
        where TSendMailJobError : class, ISendMailJobError
    {
        #region Properties

        private readonly IHostApplicationLifetime HostApplicationLifetime;
        private readonly IServiceScopeFactory ServiceScopeFactory;

        #endregion

        #region Constructor

        public EmailSender(IServiceProvider serviceProvider)
        {
            HostApplicationLifetime = serviceProvider.GetRequiredService<IHostApplicationLifetime>();
            ServiceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
        }

        #endregion

        #region IEmailSender

        public async Task SendEmailAsync(long sendMailJobId)
        {
            await Task.Run(() =>
                {
                    using (var scope = ServiceScopeFactory.CreateScope())
                    using (var dbContext = scope.ServiceProvider.GetRequiredService<TContext>())
                    {
                        var joinResult = dbContext
                            .SendMailJobs
                            .Join(dbContext.SmtpCredentials, x => x.SmtpCredentialId, x => x.Id,
                            (x, y) => new { SendMailJob = x, SmtpCredential = y })
                            .FirstOrDefault(x => x.SendMailJob.Id == sendMailJobId);

                        if (joinResult.SendMailJob == null || joinResult.SmtpCredential == null)
                        {
                            return;
                        }

                        if (joinResult.SendMailJob.SendState != SendMailStateTypes.NotSent)
                        {
                            return;
                        }


                        var client = new SmtpClient(joinResult.SmtpCredential.Server, joinResult.SmtpCredential.Port);
                        client.EnableSsl = joinResult.SmtpCredential.UseSsl;

                        if (!joinResult.SmtpCredential.UseCredentials)
                        {
                            client.Credentials = new NetworkCredential(joinResult.SmtpCredential.Username, joinResult.SmtpCredential.Password);
                        }

                        var mre = new ManualResetEvent(false);
                        client.SendCompleted += (sender, e) =>
                            {
                                if (e.Error != null)
                                {
                                    var error = new SendMailJobError()
                                    {
                                        SendMailJobRef = sendMailJobId,
                                        ErrorMessage = e.Error.Message
                                    };

                                    var maxSentAttemptsCount = joinResult.SmtpCredential.MaxSendAttempts;
                                    var template = dbContext.MailTemplates.FirstOrDefault(x => x.Id == joinResult.SendMailJob.MailTemplateId);
                                    if (template != null && template.MaxSendAttemptsCount.HasValue)
                                    {
                                        maxSentAttemptsCount = template.MaxSendAttemptsCount.Value;
                                    }

                                    if (!maxSentAttemptsCount.HasValue)
                                    {
                                        maxSentAttemptsCount = 3;
                                    }

                                    if (joinResult.SendMailJob.SendAttemptsCount >= maxSentAttemptsCount)
                                    {
                                        joinResult.SendMailJob.SendState = SendMailStateTypes.TooManyInvalidSentAttempts;
                                    }

                                    joinResult.SendMailJob.LastErrorDateUtc = DateTime.UtcNow;
                                    dbContext.Add(error);
                                }
                                else
                                {
                                    joinResult.SendMailJob.SendState = SendMailStateTypes.Sent;
                                    joinResult.SendMailJob.SentDateUtc = DateTime.UtcNow;
                                }
                                dbContext.Update(joinResult.SendMailJob);
                                dbContext.SaveChanges();
                                mre.Set();

                            };

                        var mailMessage = new MailMessage();
                        mailMessage.From = new MailAddress(joinResult.SmtpCredential.From, joinResult.SmtpCredential.DisplayName, Encoding.UTF8);

                        joinResult.SendMailJob.SendDateUtc = DateTime.UtcNow;
                        joinResult.SendMailJob.SendAttemptsCount++;
                        dbContext.Update(joinResult.SendMailJob);
                        dbContext.SaveChanges();
                        client.SendMailAsync(mailMessage).Wait(HostApplicationLifetime.ApplicationStopping);
                        mre.WaitOne();
                    }
                });
        }

        public Task SendEmailAsync(TSmtpCredential smtpCredential, string email, string subject, string htmlMessage)
        {
            using (var scope = ServiceScopeFactory.CreateScope())
            {
                var queue = scope.ServiceProvider.GetRequiredService<EmailSenderQueue<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>>();
                queue.EnqueueEmail(new string[] { email }, subject, htmlMessage, smtpCredential);
            }

            return Task.CompletedTask;
        }

        #endregion
    }

    public static class EmailSenderExtensions
    {
        public static void AddEmailSender<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>(this IServiceCollection services, Action<EmailSenderWorkerOptionsBuilder<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>> optionsBuilder)
            where TContext : DbContext, IMailDbContext<TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>
            where TSendMailJob : class, ISendMailJob
            where TSmtpCredential : class, ISmtpCredential
            where TMailTemplate : class, IMailTemplate
            where TSendMailJobRecipient : class, ISendMailJobRecipient
            where TSendMailJobError : class, ISendMailJobError
        {
            services.AddSingleton<EmailSender<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>>();
            services.AddScoped<EmailSenderQueue<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>>();
            services.AddSingleton<EmailSenderQueueEvents>();

            var builder = new EmailSenderWorkerOptionsBuilder<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>();
            optionsBuilder?.Invoke(builder);
            var options = builder.Build();

            if (options.IncludeWorker)
            {
                services.AddEmailSenderWorker(options);
            }
        }

        public static void AddEmailSenderQueue<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>(this IServiceCollection services)
           where TContext : DbContext, IMailDbContext<TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>
           where TSendMailJob : class, ISendMailJob
           where TSmtpCredential : class, ISmtpCredential
           where TMailTemplate : class, IMailTemplate
           where TSendMailJobRecipient : class, ISendMailJobRecipient
           where TSendMailJobError : class, ISendMailJobError
        {
            services.AddScoped<EmailSenderQueue<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>>();
            services.AddSingleton<EmailSenderQueueEvents>();
        }



        public static void AddEmailSenderWorker<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>(this IServiceCollection services, Action<EmailSenderWorkerOptionsBuilder<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>> optionsBuilder)
            where TContext : DbContext, IMailDbContext<TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>
            where TSendMailJob : class, ISendMailJob
            where TSmtpCredential : class, ISmtpCredential
            where TMailTemplate : class, IMailTemplate
            where TSendMailJobRecipient : class, ISendMailJobRecipient
            where TSendMailJobError : class, ISendMailJobError
        {
            var builder = new EmailSenderWorkerOptionsBuilder<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>();
            optionsBuilder?.Invoke(builder);
            var options = builder.Build();

            services.AddEmailSenderWorker(options);
        }

        public static void AddEmailSenderWorker<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>(this IServiceCollection services, EmailSenderWorkerOptions<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError> options)
            where TContext : DbContext, IMailDbContext<TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>
            where TSendMailJob : class, ISendMailJob
            where TSmtpCredential : class, ISmtpCredential
            where TMailTemplate : class, IMailTemplate
            where TSendMailJobRecipient : class, ISendMailJobRecipient
            where TSendMailJobError : class, ISendMailJobError
        {

            services.AddSingleton(options);
            services.AddScoped<EmailSenderWorkerServices<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>>();
            services.AddHostedService<EmailSenderWorker<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>>();
        }
    }
}
