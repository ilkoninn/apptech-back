using AppTech.Core.Entities;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class CertificationRepository : Repository<Certification>, ICertificationRepository
    {
        public CertificationRepository(AppDbContext context) : base(context) { }
    }
}
