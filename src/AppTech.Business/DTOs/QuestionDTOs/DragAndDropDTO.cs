using AppTech.Business.DTOs.Commons;
using Microsoft.AspNetCore.Http;

namespace AppTech.Business.DTOs.QuestionDTOs
{
    // FRONT
    public class DragDropQuestionResponseDTO : BaseEntityDTO
    {
        public string ImageUrl { get; set; }
        public ICollection<DropZoneResponseDTO> dropZones { get; set; }
        public ICollection<DragVariantResponseDTO> dragVariants { get; set; }
    }

    public class DropZoneResponseDTO : BaseEntityDTO
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class DragVariantResponseDTO : BaseEntityDTO
    {
        public string ImageUrl { get; set; }
    }

    // DATABASE
    public class DragDropQuestionDTO
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; }
        public int Point { get; set; }
        public IFormFile Image { get; set; }
        public List<DropZoneDTO> DropZones { get; set; }
        public int CertificationId { get; set; }
    }

    public class DropZoneDTO
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public IFormFile Image { get; set; } 
    }
}
