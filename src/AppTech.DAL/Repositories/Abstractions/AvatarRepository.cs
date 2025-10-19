using AppTech.Core.Entities.Identity;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class AvatarRepository : Repository<Avatar>, IAvatarRepository
    {
        public AvatarRepository(AppDbContext context) : base(context) { }
    }
}
