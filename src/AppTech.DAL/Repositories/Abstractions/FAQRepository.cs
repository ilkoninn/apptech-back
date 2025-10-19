
using AppTech.Core.Entities;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class FAQRepository : Repository<FAQ>, IFAQRepository
    {
        public FAQRepository(AppDbContext context) : base(context) { }
    }
}
