using AppTech.Core.Entities;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class CompanyTranslationRepository : Repository<CompanyTranslation>, ICompanyTranslationRepository
    {
        public CompanyTranslationRepository(AppDbContext context) : base(context) { }
    }
}
