using AppTech.Core.Entities;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class SettingTranslationRepository : Repository<SettingTranslation>, ISettingTranslationRepository
    {
        public SettingTranslationRepository(AppDbContext context) : base(context)
        {
        }
    }
}
