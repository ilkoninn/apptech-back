using AppTech.Core.Entities;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class QuestionImageRepository : Repository<QuestionImage>, IQuestionImageRepository
    {
        public QuestionImageRepository(AppDbContext context) : base(context) { }
    }
}
