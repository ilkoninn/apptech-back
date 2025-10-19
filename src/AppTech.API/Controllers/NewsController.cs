using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.NewsDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Validators.NewsValidator;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class NewsController : APIController
    {
        private readonly INewsService _newsService;

        public NewsController(INewsService NewsService)
        {
            _newsService = NewsService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            var dto = new GetAllNewsDTO();

            return Ok(await _newsService.GetAllAsync(dto));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = new GetByIdNewsDTO { Id = id };
            var validations = await new GetByIdNewsDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _newsService.GetByIdAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreateNewsDTO dto)
        {
            var validations = await new CreateNewsDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _newsService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });

        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var dto = new DeleteNewsDTO { Id = id };
            var validations = await new DeleteNewsDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _newsService.DeleteAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateNewsDTO dto)
        {
            var validations = await new UpdateNewsDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _newsService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }
    }
}
