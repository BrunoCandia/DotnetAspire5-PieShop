using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PieShop.BusinessLogic;

namespace PieShop.UI.Controllers
{
    [Authorize(Policy = "IsAdministrator")]
    public class UserController : Controller
    {
        private readonly IAccountService _accountService;

        public UserController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<IActionResult> Index()
        {
            var userRoles = await _accountService.GetUsersWithRolesAsync();

            return View(userRoles);
        }
    }
}