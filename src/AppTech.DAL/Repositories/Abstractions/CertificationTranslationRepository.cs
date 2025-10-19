using AppTech.Core.Entities;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class CertificationTranslationRepository : Repository<CertificationTranslation>, ICertificationTranslationRepository
    {
        public CertificationTranslationRepository(AppDbContext context) : base(context) { }
    }
}
