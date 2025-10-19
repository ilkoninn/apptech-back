using AppTech.Core.Entities;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class CertificationPromotionRepository : Repository<CertificationPromotion>, ICertificationPromotionRepository
    {
        public CertificationPromotionRepository(AppDbContext context) : base(context) { }
    }
}
