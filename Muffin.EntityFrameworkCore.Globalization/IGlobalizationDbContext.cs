using Microsoft.EntityFrameworkCore;

namespace Muffin.EntityFrameworkCore.Globalization
{
    public interface IGlobalizationDbContext
    {
        DbSet<LocalizedStringEntity> LocalizedStrings { get; set; }
        DbSet<LocalizedStringValueEntity> LocalizedStringValues { get; set; }
        DbSet<LanguageEntity> Languages { get; set; }

        object[] AddedEntries { get; set; }
        object[] ModifiedEntries { get; set; }
        object[] DeletedEntries { get; set; }
    }
}
