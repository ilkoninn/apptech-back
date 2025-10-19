using AppTech.Core.Entities;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class CompanyRepository : Repository<Company>, ICompanyRepository
    {
        public CompanyRepository(AppDbContext context) : base(context) { }
    }

}
