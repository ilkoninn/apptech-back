using AppTech.Core.Entities;
using AppTech.Core.Entities.Identity;

namespace AppTech.Business.Services.ExternalServices.Interfaces
{
    public interface IEmailService
    {
        Task SendConfirmationCodeMessageAsync(User currentUser, int number = 0, string toUser = "",
             bool numberOrLink = true, string token = "");
        Task SendUnbanMailAsync(string toUser, User user);
        void SendPruchaseMail(Transaction transaction, User user);
        void SendRefundMail(Transaction transaction, User user);
    }
}
