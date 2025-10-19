using AppTech.Core.Entities;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class ExamTranslationRepository : Repository<ExamTranslation>, IExamTranslationRepository
    {
        public ExamTranslationRepository(AppDbContext context) : base(context) { }
    }
}
