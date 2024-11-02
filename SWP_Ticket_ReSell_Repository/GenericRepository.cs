using SWP_Ticket_ReSell_DAO.Models;
using Mapster;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Repository;
public class GenericRepository<T> where T : class
{
    private readonly swp1Context _context;
    private readonly DbSet<T> dbSet;

    public GenericRepository(swp1Context context)
    {
        _context = context;
        dbSet = context.Set<T>();
    }
    public async Task DeleteRangeAsync(IList<T> entities)
    {
        _context.Set<T>().RemoveRange(entities);
        await _context.SaveChangesAsync();
    }

    public async Task CreateAsync(T entity)
    {
        await dbSet.AddAsync(entity);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(T entity)
    {
        dbSet.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(T entity)
    {
        dbSet.Remove(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<T?> FindByAsync(
        Expression<Func<T, bool>> expression,
        Func<IQueryable<T>, IQueryable<T>>? includeFunc = null)
    {
        IQueryable<T> query = dbSet;

        if (includeFunc != null)
        {
            query = includeFunc(query);
        }

        return await query.FirstOrDefaultAsync(expression);
    }

    public async Task<IList<TDTO>> FindListAsync<TDTO>(
        Expression<Func<T, bool>>? expression = null,
        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
        Func<IQueryable<T>, IQueryable<T>>? includeFunc = null) where TDTO : class
    {
        IQueryable<T> query = dbSet;
        if (expression != null)
        {
            query = query.Where(expression);
        }
        if (includeFunc != null)
        {
            query = includeFunc(query);
        }
        if (orderBy != null)
        {
            query = orderBy(query);
        }
        return await query.ProjectToType<TDTO>().ToListAsync();
    }

    public async Task<bool> ExistsByAsync(
        Expression<Func<T, bool>>? expression = null)
    {
        IQueryable<T> query = dbSet;

        if (expression != null)
        {
            query = query.Where(expression);
        }

        return await query.AnyAsync();
    }
}