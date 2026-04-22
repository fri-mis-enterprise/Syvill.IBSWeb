using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.Filpride.IRepository;
using IBS.Models.MasterFile;

namespace IBS.DataAccess.Repository.Filpride
{
    public class EmployeeRepository: Repository<Employee>, IEmployeeRepository
    {
        private readonly ApplicationDbContext _db;

        public EmployeeRepository(ApplicationDbContext db): base(db)
        {
            _db = db;
        }
    }
}
