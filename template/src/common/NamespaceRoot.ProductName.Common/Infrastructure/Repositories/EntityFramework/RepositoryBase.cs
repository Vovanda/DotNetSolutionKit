using Microsoft.EntityFrameworkCore;

namespace NamespaceRoot.ProductName.Common.Infrastructure.Repositories.EntityFramework;

public abstract class RepositoryBase
{
    private DbContext? _context;
    protected DbContext Context => _context ?? throw new InvalidOperationException("Context не установлен");
    
    public void SetContext(DbContext context)
    {
        _context = context;
    }
}