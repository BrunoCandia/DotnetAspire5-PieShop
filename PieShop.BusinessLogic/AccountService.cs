using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PieShop.Models.User;

namespace PieShop.BusinessLogic
{
    public class AccountService : IAccountService
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public AccountService(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<IdentityResult> AddUserToRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            return await _userManager.AddToRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> RemoveUserFromRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return IdentityResult.Failed(new IdentityError { Description = "User not found" });
            }

            return await _userManager.RemoveFromRoleAsync(user, roleName);
        }

        public async Task<bool> CheckUserInRoleAsync(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                return false;
            }

            return await _userManager.IsInRoleAsync(user, roleName);
        }

        public async Task<IdentityResult> CreateRoleAsync(string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
            {
                return IdentityResult.Failed(new IdentityError { Description = "Role already exists" });
            }

            return await _roleManager.CreateAsync(new IdentityRole(roleName));
        }

        public async Task<bool> RoleExistsAsync(string roleName)
        {
            return await _roleManager.RoleExistsAsync(roleName);
        }

        public async Task<List<IdentityRole>> GetRolesAsync()
        {
            var roles = await _roleManager.Roles.AsNoTracking().ToListAsync();

            return roles;
        }

        public async Task<List<IdentityUser>> GetUsersAsync()
        {
            var users = await _userManager.Users.AsNoTracking().ToListAsync();

            return users;
        }

        public async Task<IdentityUser?> FindUserByIdAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            return user;
        }

        public async Task<List<UserWithRoles>> GetUsersWithRolesAsync()
        {
            var users = await GetUsersAsync();

            var userRoles = new List<UserWithRoles>();

            foreach (var user in users)
            {
                var roles = await GetRolesByUserAsync(user.Id);

                userRoles.Add(new UserWithRoles
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    Roles = roles
                });
            }

            return userRoles;
        }

        public async Task<List<string>> GetRolesByUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
            {
                throw new Exception("User not found");
            }

            var rolesUser = await _userManager.GetRolesAsync(user);

            return rolesUser.ToList();
        }
    }
}
