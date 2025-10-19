using AppTech.Core.Entities;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class CertificationUserRepository : Repository<CertificationUser>, ICertificationUserRepository
    {
        public CertificationUserRepository(AppDbContext context) : base(context) { }
    }
}
