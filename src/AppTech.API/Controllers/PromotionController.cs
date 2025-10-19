using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.PromotionDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Validators.CertificationValidator;
using AppTech.Business.Validators.PromotionValidator;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class PromotionController : APIController
    {
        private readonly IPromotionService _PromotionService;

        public PromotionController(IPromotionService PromotionService)
        {
            _PromotionService = PromotionService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _PromotionService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync([FromForm] GetByIdPromotionDTO dto)
        {
            var validations = await new GetByIdPromotionDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _PromotionService.GetByIdAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreatePromotionDTO dto)
        {
            var validations = await new CreatePromotionDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _PromotionService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });

        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync([FromForm] DeletePromotionDTO dto)
        {
            var validations = await new DeletePromotionDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _PromotionService.DeleteAsync(dto.Id)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdatePromotionDTO dto)
        {
            var validations = new UpdatePromotionDTOValidator();

            return (await validations.ValidateAsync(dto)).IsValid ? Ok(await _PromotionService.UpdateAsync(dto)) :
                BadRequest(new { Errors = (await validations.ValidateAsync(dto)).Errors.Select(e => e.ErrorMessage).ToList() });
        }
    }
}
