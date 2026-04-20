using System.Linq.Expressions;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.Filpride.IRepository;
using IBS.Models.MasterFile;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace IBS.DataAccess.Repository.Filpride
{
    public class PickUpPointRepository : Repository<PickUpPoint>, IPickUpPointRepository
    {
        private readonly ApplicationDbContext _db;

        public PickUpPointRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }

        public override async Task<IEnumerable<PickUpPoint>> GetAllAsync(Expression<Func<PickUpPoint, bool>>? filter, CancellationToken cancellationToken = default)
        {
            IQueryable<PickUpPoint> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query
                .Include(p => p.Supplier)
                .ToListAsync(cancellationToken);
        }

        public override IQueryable<PickUpPoint> GetAllQuery(Expression<Func<PickUpPoint, bool>>? filter = null)
        {
            IQueryable<PickUpPoint> query = dbSet
                .Include(p => p.Supplier)
                .AsSplitQuery()
                .AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query;
        }

        public async Task<List<SelectListItem>> GetPickUpPointListBasedOnSupplier(string companyClaims, int supplierId, CancellationToken cancellationToken = default)
        {
            return await _db.PickUpPoints
                .OrderBy(p => p.Depot)
                .Where(p => p.SupplierId == supplierId)
                .Select(po => new SelectListItem
                {
                    Value = po.PickUpPointId.ToString(),
                    Text = po.Depot
                })
                .ToListAsync(cancellationToken);
        }
    }
}
