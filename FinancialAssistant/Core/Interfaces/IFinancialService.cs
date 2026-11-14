using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinancialAssistant.Core.Models;

namespace FinancialAssistant.Core.Interfaces
{
    public interface IFinancialService
    {
        Task<decimal> GetTotalBalanceAsync();
        Task<List<Income>> GetIncomesAsync(DateTime start, DateTime end);
        Task<List<Expense>> GetExpensesAsync(DateTime start, DateTime end);
        Task<Dictionary<string, decimal>> GetExpensesByCategoryAsync(DateTime start, DateTime end);
        Task<List<SavingsGoal>> GetGoalsAsync();
        Task AddIncomeAsync(Income income);
        Task AddExpenseAsync(Expense expense);
        Task AddGoalAsync(SavingsGoal goal);
        Task UpdateGoalAsync(SavingsGoal goal);
        Task DeleteGoalAsync(SavingsGoal goal);
    }
}
