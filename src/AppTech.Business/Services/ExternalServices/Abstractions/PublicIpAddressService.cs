using AppTech.Business.Services.ExternalServices.Interfaces;
using AppTech.Core.Entities.Identity;
using AppTech.Core.Enums;
using AppTech.Core.Exceptions.UserExceptions;
using AppTech.DAL.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppTech.Business.Services.ExternalServices.Abstractions
{
    public class PublicIpAddressService : IPublicIpAddressService
    {
        private readonly IPublicIpAddressRepository _publicIpAddressRepository;
        private readonly UserManager<User> _userManager;

        public PublicIpAddressService(IPublicIpAddressRepository publicIpAddressRepository, UserManager<User> userManager)
        {
            _publicIpAddressRepository = publicIpAddressRepository;
            _userManager = userManager;
        }

        public async Task CheckUserPublicIpAddressAsync(string publicIpAddress, string usernameOrEmail)
        {
            var oldUser = await _userManager.Users
                .FirstOrDefaultAsync(x => x.UserName.ToLower() == usernameOrEmail.ToLower()
                                        || x.Email.ToLower() == usernameOrEmail.ToLower())
                ?? throw new UsernameOrEmailAddressNotFoundException();

            var oldUserRole = (await _userManager.GetRolesAsync(oldUser)).FirstOrDefault();


            if(oldUserRole == EUserRole.Student.ToString())
            {
                var isUserHasThisPublicIpAddress = (await _publicIpAddressRepository.GetAllAsync(
              x => !x.IsDeleted && x.UserId == oldUser.Id && x.PublicIpAddress == publicIpAddress)).Any();

                if (!isUserHasThisPublicIpAddress)
                {
                    var publicIpAddressCount = (await _publicIpAddressRepository.GetAllAsync(
                        x => !x.IsDeleted && x.UserId == oldUser.Id)).Select(x => x.PublicIpAddress).Distinct().Count();

                    if (publicIpAddressCount >= 3)
                    {
                        if (oldUser.PublicIpAddressAccessFailed < 3)
                        {
                            oldUser.PublicIpAddressAccessFailed += 1;

                            var resultPublic = await _userManager.UpdateAsync(oldUser);

                            if (!resultPublic.Succeeded)
                                throw new UserIdentityResultException($"{resultPublic.Errors.FirstOrDefault()?.Description}");

                            throw new UserCanBeBlockedByToManyPublicIpAddress();
                        }

                        oldUser.IsBanned = true;
                        oldUser.PublicIpAddressAccessFailed = 0;

                        var resultBan = await _userManager.UpdateAsync(oldUser);

                        if (!resultBan.Succeeded)
                            throw new UserIdentityResultException($"{resultBan.Errors.FirstOrDefault()?.Description}");

                        throw new UserIsBlockedByServer();
                    }

                    var newPublicIpAddress = new UserPublicIpAddress()
                    {
                        UserId = oldUser.Id,
                        PublicIpAddress = publicIpAddress,
                        ExpiredOn = DateTime.UtcNow.AddDays(1),
                    };

                    await _publicIpAddressRepository.AddAsync(newPublicIpAddress);
                }
            }           
        }
    }
}
