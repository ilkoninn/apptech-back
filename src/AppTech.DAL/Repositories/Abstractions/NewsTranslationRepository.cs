using AppTech.Core.Entities;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class NewsTranslationRepository : Repository<NewsTranslation>, INewsTranslationRepository
    {
        public NewsTranslationRepository(AppDbContext context) : base(context) { }
    }
}
