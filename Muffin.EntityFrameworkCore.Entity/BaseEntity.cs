using Muffin.EntityFrameworkCore.Entity.Abstraction;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Muffin.EntityFrameworkCore.Entity
{
    public interface ILocalizableEntity
    {
        //Dictionary<string, Dictionary<string, string>> LocalizedStrings { get; set; }
    }

    public class BaseEntity<TKey> : IBaseEntity<TKey>, ILocalizableEntity
    {
        #region Properties

        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), JsonProperty(PropertyName = "id")]
        public virtual TKey Id { get; set; }

        #endregion

        #region Constructor

        public BaseEntity() { }

        #endregion
    }

    public class BaseEntity : BaseEntity<long>, IBaseEntity
    {
        #region Constructor

        public BaseEntity() { }

        #endregion
    }
}
