using Microsoft.AspNetCore.Mvc;
using AppTech.DAL.Repositories.Interfaces; 
using System.Threading.Tasks;

namespace AppTech.MVC.ViewComponents
{
    public class ReportedQuestionCountViewComponent : ViewComponent
    {
        private readonly IQuestionRepository _questionRepository;

        public ReportedQuestionCountViewComponent(IQuestionRepository questionRepository)
        {
            _questionRepository = questionRepository;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var reportedQuestionCount = (await _questionRepository.GetAllAsync(q => q.IsReported && !q.IsDeleted)).Count;
            return View(reportedQuestionCount);
        }
    }
}
