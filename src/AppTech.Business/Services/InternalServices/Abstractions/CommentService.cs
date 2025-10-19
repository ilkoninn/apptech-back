using System.Text.RegularExpressions;
using AppTech.Business.DTOs.CommentDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.Core.Exceptions.CertificationExceptions;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class CommentService : ICommentService
    {
        protected readonly ICertificationUserRepository _certificationUserRepository;
        protected readonly ICertificationRepository _certificationRepository;
        protected readonly ICommentRepository _commentRepository;
        protected readonly ICommentHandler _commentHandler;
        protected readonly IMapper _mapper;

        public CommentService(ICommentRepository commentRepository, IMapper mapper, ICommentHandler commentHandler, ICertificationRepository certificationRepository, ICertificationUserRepository certificationUserRepository)
        {
            _commentRepository = commentRepository;
            _commentHandler = commentHandler;
            _mapper = mapper;
            _certificationRepository = certificationRepository;
            _certificationUserRepository = certificationUserRepository;
        }

        public async Task<IQueryable<CommentDTO>> GetAllAsync()
        {
            var entities = (await _commentRepository.GetAllAsync(x => x.IsDeleted == false, u => u.User))
                .Select(c => new CommentDTO
                {
                    UserName = c.User.UserName,
                    UserImage = c.User.ImageUrl,
                    Subject = c.Subject,
                    CreatedAt = c.CreatedOn.GetValueOrDefault().ToLocalTime(),
                    CertificationSlug = c.CertificationSlug,
                });

            return entities.AsQueryable();
        }

        public async Task<IQueryable<CommentDTO>> GetBySlugAsync(GetBySlugCommentDTO dto)
        {
            var entities = (await _commentRepository.GetAllAsync(x => x.IsDeleted == false, u => u.User))
                .Where(x => x.CertificationSlug == dto.slug)
                .OrderByDescending(t => t.UpdatedOn)
                .Select(c => new CommentDTO
                {
                    UserName = c.User.UserName,
                    UserImage = c.User.ImageUrl,
                    Subject = c.Subject,
                    CreatedAt = c.CreatedOn.GetValueOrDefault().ToLocalTime(),
                    CertificationSlug = c.CertificationSlug,
                });

            return entities.AsQueryable();
        }

        public async Task<CommentDTO> GetByIdAsync(GetByIdCommentDTO dto)
        {
            var entity = _commentHandler.HandleEntityAsync(
                await _commentRepository.GetByIdAsync(x => x.Id == dto.Id,
                u => u.User, c => c.Certification));

            return new CommentDTO
            {
                UserImage = entity.User.ImageUrl,
                UserName = entity.User.UserName,
                Subject = entity.Subject,
                CreatedAt = entity.CreatedOn.GetValueOrDefault().ToLocalTime(),
                CertificationSlug = entity.CertificationSlug
            };
        }

        public async Task<CommentDTO> AddAsync(CreateCommentDTO dto)
        {
            var isUserBoughtThisCertification = (await _certificationUserRepository.GetAllAsync(x => !x.IsDeleted,
                         u => u.User,
                         c => c.Certification))
                         .Any(x => x.UserId == dto.userId && x.Certification.Slug == dto.certificationSlug);

            if (!isUserBoughtThisCertification)
                throw new UserNotBoughtCurrentCertificationException();

            dto.subject = RemoveHtmlTags(dto.subject);
            var newComment = _mapper.Map<Comment>(dto);

            var entity = await _commentRepository.AddAsync(newComment);

            return new CommentDTO
            {
                Subject = entity.Subject,
                CreatedAt = entity.CreatedOn.GetValueOrDefault().ToLocalTime(),
                CertificationSlug = entity.CertificationSlug
            };
        }

        private string RemoveHtmlTags(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;

            return Regex.Replace(input, "<.*?>", string.Empty);
        }

        public async Task<CommentDTO> DeleteAsync(DeleteCommentDTO dto)
        {
            var entity = await _commentRepository.DeleteAsync(
                _commentHandler.HandleEntityAsync(
                await _commentRepository.GetByIdAsync(x => x.Id == dto.Id)));

            return new CommentDTO
            {
                Subject = entity.Subject,
                CreatedAt = entity.CreatedOn.GetValueOrDefault().ToLocalTime(),
                CertificationSlug = entity.CertificationSlug
            };
        }

        public async Task<CommentDTO> UpdateAsync(UpdateCommentDTO dto)
        {
            var entity = await _commentRepository.UpdateAsync(_mapper.Map(dto,
                _commentHandler.HandleEntityAsync(
                await _commentRepository.GetByIdAsync(x => x.Id == dto.Id))));

            return new CommentDTO
            {
                Subject = entity.Subject,
                CreatedAt = entity.CreatedOn.GetValueOrDefault().ToLocalTime(),
                CertificationSlug = entity.CertificationSlug
            };
        }

    }
}
