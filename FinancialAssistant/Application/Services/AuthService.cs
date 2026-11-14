using System;
using System.Threading.Tasks;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;
using FinancialAssistant.Infrastructure.Services;
using Microsoft.Maui.Storage;

namespace FinancialAssistant.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepo;
        private User? _currentUser;

        public AuthService(IRepository<User> userRepo)
        {
            _userRepo = userRepo;
        }

        public bool IsAuthenticated => _currentUser != null;

        // Task 3.1 — SecureStorage-based session restore
        public async Task<User?> GetCurrentUserAsync()
        {
            if (_currentUser != null) return _currentUser;

            try
            {
                var userIdStr = await SecureStorage.GetAsync("UserId");
                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId) && userId > 0)
                {
                    _currentUser = await _userRepo.GetByIdAsync(userId);
                    // If user not found in DB (e.g. DB was reset), clear the stale session
                    if (_currentUser == null)
                        SecureStorage.Remove("UserId");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthService] GetCurrentUserAsync error: {ex.Message}");
                // Clear potentially corrupted session data
                try { SecureStorage.Remove("UserId"); } catch { }
            }

            return _currentUser;
        }

        // Task 3.1 — SecureStorage-based logout
        public async Task LogoutAsync()
        {
            _currentUser = null;
            SecureStorage.Remove("UserId");
            await Task.CompletedTask;
        }

        // Task 3.2 — Login by email
        public async Task<(User? User, string? Error)> LoginByEmailAsync(string email, string password)
        {
            var users = await _userRepo.GetByConditionAsync(u => u.Email == email);
            if (users.Count == 0)
                return (null, "Неверный email или пароль");

            var user = users[0];
            if (!PasswordHasher.VerifyPassword(password, user.PasswordHash))
                return (null, "Неверный email или пароль");

            _currentUser = user;
            await SecureStorage.SetAsync("UserId", user.Id.ToString());
            return (user, null);
        }

        // Task 3.3 — Register with email + displayName
        public async Task<(bool Success, string? Error)> RegisterAsync(string email, string password, string displayName)
        {
            var existing = await _userRepo.GetByConditionAsync(u => u.Email == email);
            if (existing.Count > 0)
                return (false, "Пользователь с таким email уже существует");

            var hashedPassword = PasswordHasher.HashPassword(password);
            var newUser = new User
            {
                Username = email.Split('@')[0],
                DisplayName = displayName,
                Email = email,
                PasswordHash = hashedPassword,
                IsPremium = false,
                Currency = "BYN",
                Language = "ru"
            };

            await _userRepo.SaveAsync(newUser);
            await LoginByEmailAsync(email, password);
            return (true, null);
        }

        // Backward-compat login by username (task 3.1 / kept for legacy)
        [Obsolete("Use LoginByEmailAsync instead")]
        public async Task<User?> LoginAsync(string username, string password)
        {
            var users = await _userRepo.GetByConditionAsync(u => u.Username == username);
            if (users.Count == 0) return null;

            var user = users[0];
            
            // Try hashed verification first; fall back to plain-text comparison for old accounts
            bool valid = false;
            try
            {
                valid = PasswordHasher.VerifyPassword(password, user.PasswordHash);
            }
            catch
            {
                // Old password format — try plain text comparison
                valid = user.PasswordHash == password;
            }

            if (!valid) return null;

            _currentUser = user;
            await SecureStorage.SetAsync("UserId", user.Id.ToString());
            return user;
        }

        // Task 5.2 — removed (Google OAuth removed)

        public async Task UpdateUserAsync(User user)
        {
            await _userRepo.SaveAsync(user);
            if (_currentUser?.Id == user.Id)
                _currentUser = user;
        }
    }
}
