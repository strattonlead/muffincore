using Muffin.EntityFrameworkCore.Entity;
using Muffin.Vimeo.Abstraction;

namespace Muffin.EntityFrameworkCore.Vimeo
{
    //[Table("VimeoCredentials")]
    public class VimeoCredential : BaseEntity, IVimeoCredential
    {
        #region Properties

        public string ClientId { get; set; }
        public string ClientSecret { get; set; }

        #endregion

        #region Constructor

        public VimeoCredential() { }

        #endregion
    }
}
