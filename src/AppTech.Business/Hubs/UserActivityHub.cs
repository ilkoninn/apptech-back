using AppTech.Core.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;

public class UserActivityHub : Hub
{
    private readonly UserManager<User> _userManager;

    public UserActivityHub(UserManager<User> userManager)
    {
        _userManager = userManager;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst("id")?.Value;

        if (!string.IsNullOrEmpty(userId))
        {
            await UpdateUserStatusAsync(userId, true);
            await Clients.All.SendAsync("UserStatusChanged", true);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.User?.FindFirst("id")?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await UpdateUserStatusAsync(userId, false);
            await Clients.All.SendAsync("UserStatusChanged", false);
        }

        await base.OnDisconnectedAsync(exception);
    }

    private async Task UpdateUserStatusAsync(string userId, bool isOnline)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            user.IsOnline = isOnline;
            await _userManager.UpdateAsync(user);
        }
    }
}
