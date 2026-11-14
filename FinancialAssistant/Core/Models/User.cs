using System;
using SQLite;

namespace FinancialAssistant.Core.Models
{
    public class User : BaseEntity
    {
        [Unique]
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty; // Simplified for diploma
        public string Email { get; set; } = string.Empty;
        public bool IsPremium { get; set; }
        public string Currency { get; set; } = "BYN";
        public string Language { get; set; } = "ru";
        public string DisplayName { get; set; } = string.Empty;
        public string? GoogleId { get; set; }
    }
}
