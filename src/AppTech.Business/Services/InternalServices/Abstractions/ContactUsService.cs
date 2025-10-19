using AppTech.Business.DTOs.ContactUsDTOs;
using AppTech.Business.Services.Interfaces;
using AppTech.Core.Entities;
using AppTech.DAL.Handlers.Interfaces;
using AppTech.DAL.Repositories.Interfaces;
using AutoMapper;

namespace AppTech.Business.Services.InternalServices.Abstractions
{
    public class ContactUsService : IContactUsService
    {
        protected readonly IContactUsRepository _contactUsRepository;
        protected readonly IContactUsHandler _contactUsHandler;
        protected readonly IMapper _mapper;

        public ContactUsService(IContactUsRepository ContactUsRepository, IMapper mapper, IContactUsHandler ContactUsHandler)
        {
            _contactUsRepository = ContactUsRepository;
            _mapper = mapper;
            _contactUsHandler = ContactUsHandler;
        }

        public async Task<IQueryable<ContactUsDTO>> GetAllAsync()
        {
            var query = await _contactUsRepository.GetAllAsync(x => x.IsDeleted == false);

            return query.Select(e => new ContactUsDTO
            {
                Id = e.Id,
                FullName = e.FullName,
                Email = e.Email,
                Message = e.Message,
                ImageUrl = e.ImageUrl
            }).AsQueryable();
        }

        public async Task<ContactUsDTO> GetByIdAsync(GetByIdContactUsDTO dto)
        {
            var entity = _contactUsHandler.HandleEntityAsync(
                await _contactUsRepository.GetByIdAsync(x => x.Id == dto.Id));

            return new ContactUsDTO
            {
                Id = entity.Id,
                FullName = entity.FullName,
                Email = entity.Email,
                Message = entity.Message,
                ImageUrl = entity.ImageUrl
            };
        }

        public async Task<ContactUsDTO> AddAsync(CreateContactUsDTO dto)
        {
            var map = _mapper.Map<ContactUs>(dto);
            var entity = await _contactUsRepository.AddAsync(map);

            return new ContactUsDTO
            {
                Id = entity.Id,
                Email = entity.Email,
                Subject = entity.Subject,
                Message = entity.Message,
                ImageUrl = entity.ImageUrl,
                FullName = entity.FullName,
            };
        }

        public async Task<ContactUsDTO> DeleteAsync(DeleteContactUsDTO dto)
        {
            var entity = await _contactUsRepository.DeleteAsync(
                _contactUsHandler.HandleEntityAsync(
                await _contactUsRepository.GetByIdAsync(x => x.Id == dto.Id)));

            return new ContactUsDTO
            {
                Id = entity.Id,
                FullName = entity.FullName,
                Email = entity.Email,
                Message = entity.Message,
                ImageUrl = entity.ImageUrl
            };
        }
    }
}
