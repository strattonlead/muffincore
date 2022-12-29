using Muffin.EntityFrameworkCore.Entity;
using Muffin.EntityFrameworkCore.Entity.Abstraction;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muffin.EntityFrameworkCore.Mail
{
    public interface ISendMailJobError : IBaseEntity
    {
        long? SendMailJobRef { get; set; }
        string ErrorMessage { get; set; }
    }

    public class SendMailJobError : BaseEntity, ISendMailJobError
    {
        #region Properties

        [ForeignKey("SendMailJobId")]
        public virtual SendMailJob SendMailJob { get; set; }
        public long? SendMailJobRef { get; set; }

        public string ErrorMessage { get; set; }

        #endregion
    }
}