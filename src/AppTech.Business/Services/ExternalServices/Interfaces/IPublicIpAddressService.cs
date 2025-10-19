namespace AppTech.Business.Services.ExternalServices.Interfaces
{
    public interface IPublicIpAddressService
    {
        Task CheckUserPublicIpAddressAsync(string publicIpAddress, string userId);
    }
}
