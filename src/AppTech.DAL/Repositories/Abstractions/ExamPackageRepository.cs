using AppTech.Core.Entities;
using AppTech.DAL.Persistence;
using AppTech.DAL.Repositories.Interfaces;

namespace AppTech.DAL.Repositories.Abstractions
{
    public class ExamPackageRepository : Repository<ExamPackage>, IExamPackageRepository
    {
        public ExamPackageRepository(AppDbContext context) : base(context) { }
    }
}
