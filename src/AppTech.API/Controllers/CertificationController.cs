using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.CertificationDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Validators.CertificationValidator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class CertificationController : BaseAPIController
    {
        private readonly ICertificationService _certificationService;

        public CertificationController(ICertificationService CertificationService)
        {
            _certificationService = CertificationService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync([FromQuery] GetAllCertificationDTO dto)
        {
            var validations = await new GetAllCertificationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _certificationService.GetAllAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpGet("id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = new GetByIdCertificationDTO { Id = id };

            return Ok(await _certificationService.GetByIdAsync(dto));
        }

        [HttpGet("{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBySlugAsync(string slug)
        {
            var dto = new GetBySlugCertificationDTO { slug = slug };

            return Ok(await _certificationService.GetBySlugAsync(dto));
        }

        [HttpGet("dashboard/{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin, Student, Moderator, Teacher")]
        public async Task<IActionResult> GetBySlugByUserAsync(string slug)
        {
            var dto = new GetBySlugCertificationDTO { slug = slug };

            return Ok(await _certificationService.GetBySlugByUserAsync(dto));
        }

        [HttpPost]
        [Authorize(Roles = "Admin, Moderator")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreateCertificationDTO dto)
        {
            var validations = await new CreateCertificationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _certificationService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });

        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, Moderator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var dto = new DeleteCertificationDTO { Id = id };

            return Ok(await _certificationService.DeleteAsync(dto));
        }

        [HttpPut]
        [Authorize(Roles = "Admin, Moderator")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateCertificationDTO dto)
        {
            var validations = await new UpdateCertificationDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _certificationService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        // User Certification Relational Methods
        [HttpGet("user/{userId}")]
        [Authorize(Roles = "Admin, Student, Moderator, Teacher")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetAllByUserIdAsync(string userId)
        {
            var dto = new GetAllByUserDTO { userId = userId };

            return Ok(await _certificationService.GetAllByUserAsync(dto));
        }
    }
}
