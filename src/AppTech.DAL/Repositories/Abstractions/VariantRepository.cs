using AppTech.Core.Entities;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class VariantRepository : Repository<Variant>, IVariantRepository
    {
        public VariantRepository(AppDbContext context) : base(context) { }
    }
}
