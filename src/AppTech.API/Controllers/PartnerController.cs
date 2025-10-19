using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.PartnerDTOs;
using AppTech.Business.Services.InternalServices.Interfaces;
using AppTech.Business.Validators.PartnerValidator;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class PartnerController : BaseAPIController
    {
        private readonly IPartnerService _partnerService;

        public PartnerController(IPartnerService PartnerService)
        {
            _partnerService = PartnerService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _partnerService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = new GetByIdPartnerDTO { Id = id };
            var validations = await new GetByIdPartnerDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _partnerService.GetByIdAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost("contact")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddPartnetOnlineAsync([FromBody] CreatePartnerOnlineDTO dto)
        {
            var validations = await new CreatePartnerOnlineDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _partnerService.AddPartnerOnlineAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreatePartnerDTO dto)
        {
            var validations = await new CreatePartnerDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _partnerService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });

        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var dto = new DeletePartnerDTO { Id = id };
            var validations = await new DeletePartnerDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _partnerService.DeleteAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdatePartnerDTO dto)
        {
            var validations = await new UpdatePartnerDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _partnerService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }
    }
}
