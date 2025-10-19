using AppTech.Business.DTOs.CommentDTOs;
using AppTech.Core.Entities;
using AutoMapper;

namespace AppTech.Business.MappingProfiles
{
    public class CommentMP : Profile
    {
        public CommentMP()
        {
            CreateMap<CreateCommentDTO, Comment>(); // DTO-dan Entity-ə
            CreateMap<Comment, CreateCommentDTO>().ReverseMap(); // Entity-dən DTO-ya
            CreateMap<UpdateCommentDTO, Comment>();
            CreateMap<Comment, UpdateCommentDTO>().ReverseMap();
        }
    }
}
