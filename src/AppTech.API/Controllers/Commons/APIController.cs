using Microsoft.AspNetCore.Authorization;

namespace AppTech.API.Controllers.Commons
{
    [Authorize(Roles = "Admin, Student, Moderator, Teacher")]
    public class APIController : BaseAPIController { }
}
