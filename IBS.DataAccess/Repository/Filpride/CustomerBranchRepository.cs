using System.Linq.Expressions;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.Filpride.IRepository;
using IBS.Models.MasterFile;
using Microsoft.EntityFrameworkCore;

namespace IBS.DataAccess.Repository.Filpride
{
    public class CustomerBranchRepository: Repository<CustomerBranch>, ICustomerBranchRepository
    {
        private readonly ApplicationDbContext _db;

        public CustomerBranchRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
        }

        public async Task UpdateAsync(CustomerBranch model, CancellationToken cancellationToken)
        {
            var currentModel = await _db
                .CustomerBranches.FirstOrDefaultAsync(x => x.Id == model.Id, cancellationToken);

            if (currentModel == null)
            {
                throw new NullReferenceException("Customer branch not found");
            }

            currentModel.CustomerId = model.CustomerId;
            currentModel.BranchName = model.BranchName;
            currentModel.BranchAddress = model.BranchAddress;
            currentModel.BranchTin = model.BranchTin;

            await _db.SaveChangesAsync(cancellationToken);
        }

        public override async Task<IEnumerable<CustomerBranch>> GetAllAsync(
            Expression<Func<CustomerBranch, bool>>? filter, CancellationToken cancellationToken = default)
        {
            IQueryable<CustomerBranch> query = dbSet;
            if (filter != null)
            {
                query = query.Where(filter);
            }

            return await query
                .Include(b => b.Customer)
                .ToListAsync(cancellationToken);
        }

        public override IQueryable<CustomerBranch> GetAllQuery(Expression<Func<CustomerBranch, bool>>? filter = null)
        {
            IQueryable<CustomerBranch> query = dbSet
                .Include(b => b.Customer)
                .AsSplitQuery()
                .AsNoTracking();

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query;
        }
    }
}
