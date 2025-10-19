using AppTech.Core.Entities.Identity;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class ExamResultRepository : Repository<ExamResult>, IExamResultRepository
    {
        public ExamResultRepository(AppDbContext context) : base(context) { }
    }
}
