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
    // Cách sử dụng : Nó Sẽ Tìm object Entity Theo điều kiện ví dụ :
    // p => p.customerID == id Nó sẽ lấy id của object trong db sau đó so sánh với id của người dùng và trả về object
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

    // Cái này cũng giống như cái trên nhưng trả về 1 List Object 
    // expression là điều kiện lọc : ví dụ p => p.Name == name 
    // orderBy cũng là điệu kiện lọc nếu sử dụng cái này  _ => _.OrderByDescending(p => p.CreatedDate) Thì nó sẽ xắp xếp giảm dần (còn nếu k sài thì nó sẽ bỏ qua)
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
    // Cái này sẽ kiểm tra xem điệu kiện đúng hay không trả về True False
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