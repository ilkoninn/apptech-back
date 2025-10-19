using AppTech.Core.Entities.Identity;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class PublicIpAddressRepository : Repository<UserPublicIpAddress>, IPublicIpAddressRepository
    {
        public PublicIpAddressRepository(AppDbContext context) : base(context) { }
    }
}
