using AppTech.API.Controllers.Commons;
using AppTech.Business.DTOs.CommentDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Business.Validators.CommentValidator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppTech.API.Controllers
{
    public class CommentController : BaseAPIController
    {
        private readonly ICommentService _commentService;

        public CommentController(ICommentService CommentService)
        {
            _commentService = CommentService;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllAsync()
        {
            return Ok(await _commentService.GetAllAsync());
        }

        [HttpGet("id")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var dto = new GetByIdCommentDTO { Id = id };
            var validations = await new GetByIdCommentDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _commentService.GetByIdAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpGet("{slug}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetBySlugAsync(string slug)
        {
            var dto = new GetBySlugCommentDTO { slug = slug };

            return Ok(await _commentService.GetBySlugAsync(dto));
        }


        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin, Student, Moderator, Teacher")]
        public async Task<IActionResult> AddAsync([FromBody] CreateCommentDTO dto)
        {
            var validations = await new CreateCommentDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? StatusCode(201, await _commentService.AddAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });

        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin, Student, Moderator, Teacher")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var dto = new DeleteCommentDTO { Id = id };
            var validations = await new DeleteCommentDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _commentService.DeleteAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(Roles = "Admin, Student, Moderator, Teacher")]
        public async Task<IActionResult> UpdateAsync([FromForm] UpdateCommentDTO dto)
        {
            var validations = await new UpdateCommentDTOValidator().ValidateAsync(dto);

            return validations.IsValid ? Ok(await _commentService.UpdateAsync(dto)) :
                BadRequest(new { Errors = validations.Errors.Select(e => e.ErrorMessage).ToList() });
        }
    }
}
