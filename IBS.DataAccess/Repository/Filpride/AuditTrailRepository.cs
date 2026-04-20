using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.Filpride.IRepository;
using IBS.Models.Common;

namespace IBS.DataAccess.Repository.Filpride
{
    public class AuditTrailRepository : Repository<AuditTrail>, IAuditTrailRepository
    {
        private readonly ApplicationDbContext _db;

        public AuditTrailRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
    }
}
