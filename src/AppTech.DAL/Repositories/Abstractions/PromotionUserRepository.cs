using AppTech.Core.Entities.Identity;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class PromotionUserRepository : Repository<PromotionUser>, IPromotionUserRepository
    {
        public PromotionUserRepository(AppDbContext context) : base(context) { }
    }
}
