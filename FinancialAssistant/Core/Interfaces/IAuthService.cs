using System;
using System.Threading.Tasks;
using FinancialAssistant.Core.Models;

namespace FinancialAssistant.Core.Interfaces
{
    public interface IAuthService
    {
        [Obsolete("Use LoginByEmailAsync instead")]
        Task<User?> LoginAsync(string username, string password);

        Task<(User? User, string? Error)> LoginByEmailAsync(string email, string password);
        Task<(bool Success, string? Error)> RegisterAsync(string email, string password, string displayName);
        Task LogoutAsync();
        Task<User?> GetCurrentUserAsync();
        Task UpdateUserAsync(User user);
        bool IsAuthenticated { get; }
    }
}
