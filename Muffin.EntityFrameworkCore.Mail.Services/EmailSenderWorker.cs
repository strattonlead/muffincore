using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Muffin.BackgroundServices;
using Muffin.Tenancy.Abstraction.Services;
using Muffin.Tenancy.Services.Abstraction;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Muffin.EntityFrameworkCore.Mail.Services
{
    public class EmailSenderWorker<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError> : EventBackgroundServiceWithTenancy<EmailSenderWorkerServices<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>>
        where TContext : DbContext, IMailDbContext<TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>
        where TSendMailJob : class, ISendMailJob
        where TSmtpCredential : class, ISmtpCredential
        where TMailTemplate : class, IMailTemplate
        where TSendMailJobRecipient : class, ISendMailJobRecipient
        where TSendMailJobError : class, ISendMailJobError
    {
        #region Properties

        private readonly EmailSenderQueueEvents Events;
        private readonly EmailSenderWorkerOptions<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError> Options;

        #endregion

        #region Constructor

        public EmailSenderWorker(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Events = serviceProvider.GetRequiredService<EmailSenderQueueEvents>();
            Options = serviceProvider.GetRequiredService<EmailSenderWorkerOptions<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>>();
            RunOnStartup = true;
        }

        #endregion

        #region EventBackgroundService

        protected override void OnStart()
        {
            Events.OnEnqueue += (sender, args) =>
            {
                ForceRun();
            };
        }

        protected override Task ExecuteScopedAsync(EmailSenderWorkerServices<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError> scope, CancellationToken cancellationToken)
        {
            //if (scope.IsTenancyEnabled)
            //{
            //    var tenants = scope.TenantEnumerator.GetEnumerator();
            //    foreach (var tenant in tenants)
            //    {
            //        if (cancellationToken.IsCancellationRequested)
            //        {
            //            return Task.CompletedTask;
            //        }
            //        scope.TenantScope.InvokeScoped(tenant, tenantScope =>
            //        {
            //            _runScoped(tenantScope.ServiceProvider, cancellationToken);
            //        });
            //    }
            //}
            //else
            //{
            //    _runScoped(scope.ServiceProvider, cancellationToken);
            //}
            _runScoped(scope.ServiceProvider, cancellationToken);

            return Task.CompletedTask;
        }

        private void _runScoped(IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            using (var dbContext = serviceProvider.GetRequiredService<TContext>())
            {
                var emailSender = serviceProvider.GetRequiredService<EmailSender<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>>();

                while (true)
                {
                    var sendMailJobsIds = dbContext.SendMailJobs.Where(x => x.SendState != SendMailStateTypes.Sent)
                                                .Take(Options.MaxConcurrentSendJobs)
                                                .Select(x => x.Id)
                                                .ToArray();

                    if (sendMailJobsIds.Length == 0)
                    {
                        break;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var sendMailTasks = sendMailJobsIds.Select(x => emailSender.SendEmailAsync(x)).ToArray();
                    Task.WaitAll(sendMailTasks, cancellationToken);

                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                }
            }
        }

        #endregion
    }

    public class EmailSenderWorkerOptions<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>
        where TContext : DbContext, IMailDbContext<TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>
        where TSendMailJob : class, ISendMailJob
        where TSmtpCredential : class, ISmtpCredential
        where TMailTemplate : class, IMailTemplate
        where TSendMailJobRecipient : class, ISendMailJobRecipient
        where TSendMailJobError : class, ISendMailJobError
    {
        public int MaxConcurrentSendJobs { get; set; } = 1;
        public bool IncludeWorker { get; set; }
    }

    public class EmailSenderWorkerOptionsBuilder<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>
        where TContext : DbContext, IMailDbContext<TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>
        where TSendMailJob : class, ISendMailJob
        where TSmtpCredential : class, ISmtpCredential
        where TMailTemplate : class, IMailTemplate
        where TSendMailJobRecipient : class, ISendMailJobRecipient
        where TSendMailJobError : class, ISendMailJobError
    {
        private EmailSenderWorkerOptions<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError> Options = new EmailSenderWorkerOptions<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>();

        public EmailSenderWorkerOptionsBuilder<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError> UseMaxConuccentSendJobs(int maxConcurrentSendJobs)
        {
            if (maxConcurrentSendJobs <= 0)
            {
                throw new ArgumentException($"{nameof(maxConcurrentSendJobs)} must be greater than 0");
            }
            Options.MaxConcurrentSendJobs = maxConcurrentSendJobs;
            return this;
        }

        public EmailSenderWorkerOptionsBuilder<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError> UseBackgroundServiceWorker()
        {
            Options.IncludeWorker = true;
            return this;
        }

        public EmailSenderWorkerOptions<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError> Build()
        {
            return Options;
        }
    }

    public class EmailSenderWorkerServices<TContext, TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>
        where TContext : DbContext, IMailDbContext<TSendMailJob, TSmtpCredential, TMailTemplate, TSendMailJobRecipient, TSendMailJobError>
        where TSendMailJob : class, ISendMailJob
        where TSmtpCredential : class, ISmtpCredential
        where TMailTemplate : class, IMailTemplate
        where TSendMailJobRecipient : class, ISendMailJobRecipient
        where TSendMailJobError : class, ISendMailJobError
    {
        public readonly ITenantEnumerator TenantEnumerator;
        public readonly ITenantScope TenantScope;
        public readonly TContext DbContext;
        public readonly IServiceProvider ServiceProvider;
        public bool IsTenancyEnabled => TenantEnumerator != null && TenantScope != null;

        public EmailSenderWorkerServices(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            DbContext = serviceProvider.GetRequiredService<TContext>();
            TenantEnumerator = serviceProvider.GetService<ITenantEnumerator>();
            TenantScope = serviceProvider.GetService<ITenantScope>();
        }
    }
}
