using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinancialAssistant.Core.Models;

namespace FinancialAssistant.Core.Interfaces
{
    public interface IRepository<T> where T : BaseEntity, new()
    {
        Task<List<T>> GetAllAsync();
        Task<T> GetByIdAsync(int id);
        Task<int> SaveAsync(T item);
        Task<int> DeleteAsync(T item);
        Task<List<T>> GetByConditionAsync(Func<T, bool> predicate);
    }
}
