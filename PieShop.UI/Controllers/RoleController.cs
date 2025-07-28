using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PieShop.BusinessLogic;

namespace PieShop.UI.Controllers
{
    [Authorize(Policy = "IsAdministrator")]
    public class RoleController : Controller
    {
        private readonly IAccountService _accountService;

        public RoleController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _accountService.GetRolesAsync();

            return View(roles);
        }

        public IActionResult Create()
        {
            Console.WriteLine("Create role action called");
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(string roleName)
        {
            if (!string.IsNullOrWhiteSpace(roleName))
            {
                var result = await _accountService.CreateRoleAsync(roleName);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View();
        }

        public async Task<IActionResult> Assign()
        {
            ViewBag.Users = await _accountService.GetUsersAsync();
            ViewBag.Roles = await _accountService.GetRolesAsync();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Assign(string userId, string roleName)
        {
            var user = await _accountService.FindUserByIdAsync(userId);

            if (user != null && !string.IsNullOrWhiteSpace(roleName))
            {
                var result = await _accountService.AddUserToRoleAsync(user.Id, roleName);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            var user = await _accountService.FindUserByIdAsync(userId);

            if (user != null && !string.IsNullOrWhiteSpace(roleName))
            {
                var result = await _accountService.RemoveUserFromRoleAsync(user.Id, roleName);

                if (result.Succeeded)
                {
                    return RedirectToAction(nameof(Index));
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return RedirectToAction(nameof(Index));
        }
    }
}