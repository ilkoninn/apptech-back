
using AppTech.Core.Entities;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class GiftCardRepository : Repository<GiftCard>, IGiftCardRepository
    {
        public GiftCardRepository(AppDbContext context) : base(context) { }
    }
}
