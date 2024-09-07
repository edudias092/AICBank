using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AICBank.Core.Entities;
using AICBank.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace AICBank.Data.Repositories
{
    public class Repository<T> : IRepository<T> where T : Entity, new()
    {
        protected readonly AICBankDbContext _db;
        protected readonly DbSet<T> _set;
        public Repository(AICBankDbContext db)
        {
            _db = db;
            _set = _db.Set<T>();
        }

        public async Task<List<T>> Get(Expression<Func<T, bool>> expression)
        {
            var result = await _set.Where(expression).ToListAsync();

            return result;
        }

        public async Task<List<T>> GetAll()
        {
            return await _set.AsNoTracking().ToListAsync();
        }

        public async Task<T> GetById(int id)
        {
            return await _set.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task Add(T entity)
        {
            _set.Add(entity);

            await Commit();
        }

        public async Task Update(T entity)
        {
            _set.Update(entity);

            await Commit();
        }

        public async Task Remove(int id)
        {
            var entity = new T {Id = id};

            _set.Remove(entity);

            await Commit();
        }

        public async Task Commit()
        {
            await _db.SaveChangesAsync();
        }

        public void Dispose()
        {
            _db.Dispose();
        }
    }
}