using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.AvatarDTOs;
using AppTech.Business.Services.InternalServices.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class AvatarController : BaseAPIController
    {
        private readonly IAvatarService _AvatarService;

        public AvatarController(IAvatarService AvatarService)
        {
            _AvatarService = AvatarService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _AvatarService.GetAllAsync());
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = new GetByIdAvatarDTO { Id = id };

            return Ok(await _AvatarService.GetByIdAsync(dto));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AddAsync([FromForm] CreateAvatarDTO dto)
        {
            return StatusCode(201, await _AvatarService.AddAsync(dto));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var dto = new DeleteAvatarDTO { Id = id };

            return Ok(await _AvatarService.DeleteAsync(dto));
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateAvatarDTO dto)
        {
            return Ok(await _AvatarService.UpdateAsync(dto));
        }
    }
}
