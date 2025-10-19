using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.ContactUsDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Validators.ContactUsValidator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class ContactUsController : BaseAPIController
    {
        private readonly IContactUsService _ContactUsService;

        public ContactUsController(IContactUsService ContactUsService)
        {
            _ContactUsService = ContactUsService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _ContactUsService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = new GetByIdContactUsDTO { Id = id };
            var validations = await new GetByIdContactUsDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _ContactUsService.GetByIdAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreateContactUsDTO dto)
        {
            var validations = await new CreateContactUsDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _ContactUsService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var dto = new DeleteContactUsDTO { Id = id };
            var validations = await new DeleteContactUsDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _ContactUsService.DeleteAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }


    }
}
