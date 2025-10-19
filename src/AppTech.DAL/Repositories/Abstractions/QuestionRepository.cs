using AppTech.Core.Entities;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class QuestionRepository : Repository<Question>, IQuestionRepository
    {
        public QuestionRepository(AppDbContext dbContext) : base(dbContext) { }
    }
}
