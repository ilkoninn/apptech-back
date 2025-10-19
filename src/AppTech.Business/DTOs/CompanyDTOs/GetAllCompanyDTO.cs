namespace AppTech.Business.DTOs.CompanyDTOs
{
    public class GetAllCompanyDTO
    {
        public bool isTop { get; set; }
    }

    public class GetAllCompanyByPageDTO
    {
        public int pageIndex { get; set; }
        public int pageSize { get; set; } = 16;
    }

}
