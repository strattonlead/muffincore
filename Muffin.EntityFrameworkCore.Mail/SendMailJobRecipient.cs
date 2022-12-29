using Muffin.EntityFrameworkCore.Entity;
using Muffin.EntityFrameworkCore.Entity.Abstraction;

namespace Muffin.EntityFrameworkCore.Mail
{
    public interface ISendMailJobRecipient : IBaseEntity
    {
        long? SendMailJobId { get; set; }
        string Recipient { get; set; }
    }

    public class SendMailJobRecipient : BaseEntity, ISendMailJobRecipient
    {
        #region Properties

        public SendMailJob SendMailJob { get; set; }
        public long? SendMailJobId { get; set; }

        [Protected]
        public string Recipient { get; set; }

        #endregion
    }
}
