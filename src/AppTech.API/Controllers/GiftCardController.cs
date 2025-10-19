using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.GiftCardDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Validators.GiftCardDTOs;
using AppTech.Business.Validators.GiftCardValidator;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class GiftCardController : APIController
    {
        private readonly IGiftCardService _giftCardService;

        public GiftCardController(IGiftCardService giftCardService)
        {
            _giftCardService = giftCardService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _giftCardService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync([FromQuery] GetByIdGiftCardDTO dto)
        {
            var validations = await new GetByIdGiftCardDTOsValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _giftCardService.GetByIdAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreateGiftCardDTO dto)
        {
            var validations = await new CreateGiftCardDTOsValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _giftCardService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync([FromForm] DeleteGiftCardDTO dto)
        {
            var validations = await new DeleteGiftCardDTOsValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _giftCardService.DeleteAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateGiftCardDTO dto)
        {
            var validations = await new UpdateGiftCardDTOsValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _giftCardService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPost("recipient")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Async([FromBody] GiftcardRecipientDTO dto)
        {
            var validations = await new GiftcardRecipientDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _giftCardService.SendMoneyToRecipientEmail(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }
    }
}
