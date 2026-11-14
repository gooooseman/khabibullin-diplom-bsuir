using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;

namespace FinancialAssistant.Application.Services
{
    public class FinancialService : IFinancialService
    {
        private readonly IRepository<Income> _incomeRepo;
        private readonly IRepository<Expense> _expenseRepo;
        private readonly IRepository<SavingsGoal> _goalsRepo;
        private readonly IRepository<Category> _categoryRepo;
        private readonly IAuthService _authService;

        public FinancialService(
            IRepository<Income> incomeRepo,
            IRepository<Expense> expenseRepo,
            IRepository<SavingsGoal> goalsRepo,
            IRepository<Category> categoryRepo,
            IAuthService authService)
        {
            _incomeRepo = incomeRepo;
            _expenseRepo = expenseRepo;
            _goalsRepo = goalsRepo;
            _categoryRepo = categoryRepo;
            _authService = authService;
        }

        private async Task<int> GetCurrentUserIdAsync()
        {
            var user = await _authService.GetCurrentUserAsync();
            return user?.Id ?? 0;
        }

        public async Task<decimal> GetTotalBalanceAsync()
        {
            int userId = await GetCurrentUserIdAsync();
            var incomes = await _incomeRepo.GetByConditionAsync(i => i.UserId == userId);
            var expenses = await _expenseRepo.GetByConditionAsync(e => e.UserId == userId);
            return incomes.Sum(i => i.Amount) - expenses.Sum(e => e.Amount);
        }

        public async Task<List<Income>> GetIncomesAsync(DateTime start, DateTime end)
        {
            int userId = await GetCurrentUserIdAsync();
            return await _incomeRepo.GetByConditionAsync(
                i => i.UserId == userId && i.Date >= start && i.Date <= end);
        }

        public async Task<List<Expense>> GetExpensesAsync(DateTime start, DateTime end)
        {
            int userId = await GetCurrentUserIdAsync();
            var expenses = await _expenseRepo.GetByConditionAsync(
                e => e.UserId == userId && e.Date >= start && e.Date <= end);
            var categories = await _categoryRepo.GetAllAsync();
            foreach (var expense in expenses)
                expense.Category = categories.FirstOrDefault(c => c.Id == expense.CategoryId);
            return expenses;
        }

        public async Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(DateTime start, DateTime end)
        {
            var expenses = await GetExpensesAsync(start, end);
            return expenses
                .GroupBy(e => e.Category?.Name ?? "Без категории")
                .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        }

        public async Task<List<SavingsGoal>> GetGoalsAsync()
        {
            int userId = await GetCurrentUserIdAsync();
            return await _goalsRepo.GetByConditionAsync(g => g.UserId == userId);
        }

        public async Task AddIncomeAsync(Income income)
        {
            if (income.Amount <= 0) throw new ArgumentException("Amount must be positive");
            if (income.UserId == 0)
                income.UserId = await GetCurrentUserIdAsync();
            await _incomeRepo.SaveAsync(income);
        }

        public async Task AddExpenseAsync(Expense expense)
        {
            if (expense.Amount <= 0) throw new ArgumentException("Amount must be positive");
            if (expense.UserId == 0)
                expense.UserId = await GetCurrentUserIdAsync();
            await _expenseRepo.SaveAsync(expense);
        }

        public async Task AddGoalAsync(SavingsGoal goal)
        {
            if (goal.UserId == 0)
                goal.UserId = await GetCurrentUserIdAsync();
            await _goalsRepo.SaveAsync(goal);
        }

        public async Task UpdateGoalAsync(SavingsGoal goal)
        {
            await _goalsRepo.SaveAsync(goal);
        }

        public async Task DeleteGoalAsync(SavingsGoal goal)
        {
            await _goalsRepo.DeleteAsync(goal);
        }
    }
}
