using Microsoft.AspNetCore.Identity;
using PieShop.Models.User;

namespace PieShop.BusinessLogic
{
    public interface IAccountService
    {
        Task<List<IdentityUser>> GetUsersAsync();
        Task<IdentityUser?> FindUserByIdAsync(string userId);
        Task<IdentityResult> AddUserToRoleAsync(string userId, string roleName);
        Task<IdentityResult> RemoveUserFromRoleAsync(string userId, string roleName);
        Task<bool> CheckUserInRoleAsync(string userId, string roleName);
        Task<IdentityResult> CreateRoleAsync(string roleName);
        Task<bool> RoleExistsAsync(string roleName);
        Task<List<IdentityRole>> GetRolesAsync();
        Task<List<UserWithRoles>> GetUsersWithRolesAsync();
        Task<List<string>> GetRolesByUserAsync(string userId);
    }
}
