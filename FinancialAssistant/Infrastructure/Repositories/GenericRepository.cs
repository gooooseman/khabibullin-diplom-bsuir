using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FinancialAssistant.Core.Interfaces;
using FinancialAssistant.Core.Models;
using FinancialAssistant.Infrastructure.Data;

namespace FinancialAssistant.Infrastructure.Repositories
{
    public class GenericRepository<T> : IRepository<T> where T : BaseEntity, new()
    {
        private readonly DatabaseService _databaseService;

        public GenericRepository(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        private async Task Init()
        {
            await _databaseService.Init();
        }

        public async Task<List<T>> GetAllAsync()
        {
            await Init();
            return await _databaseService.Connection.Table<T>().ToListAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            await Init();
            return await _databaseService.Connection.Table<T>().Where(i => i.Id == id).FirstOrDefaultAsync();
        }

        public async Task<int> SaveAsync(T item)
        {
            await Init();
            if (item.Id != 0)
                return await _databaseService.Connection.UpdateAsync(item);
            else
                return await _databaseService.Connection.InsertAsync(item);
        }

        public async Task<int> DeleteAsync(T item)
        {
            await Init();
            return await _databaseService.Connection.DeleteAsync(item);
        }

        public async Task<List<T>> GetByConditionAsync(Func<T, bool> predicate)
        {
            await Init();
            // Note: This loads all into memory first which is not ideal for large datasets but ok for local SQLite
            // To do this efficiently with SQLite-net, we need Expressions which Func<> is not directly.
            // For now, load all and filter in memory as predicate is Func.
            var all = await _databaseService.Connection.Table<T>().ToListAsync();
            return new List<T>(System.Linq.Enumerable.Where(all, predicate));
        }
    }
}
