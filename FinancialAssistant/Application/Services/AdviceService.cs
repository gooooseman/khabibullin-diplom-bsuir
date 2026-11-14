using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FinancialAssistant.Core.Interfaces;

namespace FinancialAssistant.Application.Services
{
    public class AdviceService : IAdviceService
    {
        private readonly IFinancialService _financialService;

        public AdviceService(IFinancialService financialService)
        {
            _financialService = financialService;
        }

        public async Task<List<FinancialAdvice>> GenerateAdviceAsync()
        {
            var adviceList = new List<FinancialAdvice>();
            var now = DateTime.Now;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            var balance = await _financialService.GetTotalBalanceAsync();
            if (balance < 0)
            {
                adviceList.Add(new FinancialAdvice
                {
                    Title = "Отрицательный баланс",
                    Description = "Ваш общий баланс отрицательный. Рекомендуем сократить расходы.",
                    Severity = "Critical",
                    Action = "Просмотреть расходы"
                });
            }

            var goals = await _financialService.GetGoalsAsync();
            foreach (var goal in goals)
            {
                if (goal.Deadline < now && goal.CurrentAmount < goal.TargetAmount)
                {
                    adviceList.Add(new FinancialAdvice
                    {
                        Title = $"Цель не достигнута: {goal.Name}",
                        Description = $"Срок по цели «{goal.Name}» истёк. Обновите срок или целевую сумму.",
                        Severity = "Warning",
                        Action = "Обновить цель"
                    });
                }
                else if (goal.TargetAmount > 0)
                {
                    var monthsLeft = ((goal.Deadline.Year - now.Year) * 12) + goal.Deadline.Month - now.Month;
                    if (monthsLeft > 0)
                    {
                        var requiredMonthly = (goal.TargetAmount - goal.CurrentAmount) / monthsLeft;
                        adviceList.Add(new FinancialAdvice
                        {
                            Title = $"Совет по цели: {goal.Name}",
                            Description = $"Для достижения цели откладывайте {requiredMonthly:N0} в месяц.",
                            Severity = "Info",
                            Action = "Посмотреть цель"
                        });
                    }
                }
            }

            var expenses = await _financialService.GetExpensesByCategoryAsync(startOfMonth, now);
            if (expenses.Any())
            {
                var topCategory = expenses.OrderByDescending(x => x.Value).First();
                adviceList.Add(new FinancialAdvice
                {
                    Title = "Топ категория расходов",
                    Description = $"Больше всего потрачено на «{topCategory.Key}» — {topCategory.Value:N0} за этот месяц.",
                    Severity = "Info",
                    Action = "Анализировать"
                });
            }

            if (!adviceList.Any())
            {
                adviceList.Add(new FinancialAdvice
                {
                    Title = "Всё под контролем",
                    Description = "Ваши финансы в порядке. Продолжайте в том же духе!",
                    Severity = "Info",
                    Action = ""
                });
            }

            return adviceList;
        }
    }
}
