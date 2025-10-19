using AppTech.Business.DTOs.AvatarDTOs;
using AppTech.Business.Helpers;
using AppTech.Business.Services.InternalServices.Interfaces;
using AppTech.Core.Entities.Identity;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class AvatarService : IAvatarService
    {
        protected readonly IAvatarRepository _avatarRepository;
        protected readonly IAvatarHandler _avatarHandler;
        protected readonly IHttpContextAccessor _http;
        protected readonly IMapper _mapper;

        public AvatarService(IAvatarRepository AvatarRepository, IMapper mapper, IAvatarHandler AvatarHandler, IHttpContextAccessor http)
        {
            _avatarRepository = AvatarRepository;
            _avatarHandler = AvatarHandler;
            _mapper = mapper;
            _http = http;
        }

        public async Task<IQueryable<AvatarDTO>> GetAllAsync()
        {
            var language = LanguageChanger.Change(new LanguageCatcher(_http).GetLanguage());

            var entities = (await _avatarRepository.GetAllAsync(
                x => x.IsDeleted == false))
                .Select(e => new AvatarDTO
                {
                    Id = e.Id,
                    ImageUrl = e.ImageUrl,
                    Gender = e.Gender
                });

            return entities.AsQueryable();
        }

        public async Task<AvatarDTO> GetByIdAsync(GetByIdAvatarDTO dto)
        {
            var entity = _avatarHandler.HandleEntityAsync(
                await _avatarRepository.GetByIdAsync(x => x.Id == dto.Id));

            return new AvatarDTO
            {
                Id = entity.Id,
                ImageUrl = entity.ImageUrl,
                Gender = entity.Gender
            };
        }

        public async Task<AvatarDTO> AddAsync(CreateAvatarDTO dto)
        {
            var entity = await _avatarRepository.AddAsync(_mapper.Map<Avatar>(dto));

            return new AvatarDTO
            {
                Id = entity.Id,
                ImageUrl = entity.ImageUrl,
                Gender = entity.Gender
            };
        }

        public async Task<AvatarDTO> DeleteAsync(DeleteAvatarDTO dto)
        {
            var entity = await _avatarRepository.DeleteAsync(
                _avatarHandler.HandleEntityAsync(
                await _avatarRepository.GetByIdAsync(x => x.Id == dto.Id)));

            return new AvatarDTO
            {
                Id = entity.Id,
                ImageUrl = entity.ImageUrl,
                Gender = entity.Gender
            };
        }

        public async Task<AvatarDTO> UpdateAsync(UpdateAvatarDTO dto)
        {
            var entity = await _avatarRepository.UpdateAsync(_mapper.Map(dto,
                _avatarHandler.HandleEntityAsync(
                await _avatarRepository.GetByIdAsync(x => x.Id == dto.Id))));

            return new AvatarDTO
            {
                Id = entity.Id,
                ImageUrl = entity.ImageUrl,
                Gender = entity.Gender
            };
        }
    }
}
