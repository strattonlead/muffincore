using Muffin.EntityFrameworkCore.Entity;
using Muffin.Vimeo.Abstraction;
using System;

namespace Muffin.EntityFrameworkCore.Vimeo
{
    //[Table("VimeoTokens")]
    public class VimeoToken : BaseEntity, IVimeoToken
    {
        #region Properties

        public long? ExternalId { get; set; }
        public string Name { get; set; }
        public string ExternalProfileImageUrl { get; set; }
        public DateTime AccountCreatedDateutc { get; set; }
        public string AccessToken { get; set; }
        public string Scope { get; set; }
        public string TokenType { get; set; }

        #endregion

        #region Constructor

        public VimeoToken() { }

        #endregion
    }
}
