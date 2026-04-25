using System.Linq.Expressions;
using IBS.DataAccess.Repository.IRepository;
using IBS.DTOs;
using IBS.Models.MasterFile;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace IBS.DataAccess.Repository.Filpride.IRepository
{
    public interface IChartOfAccountRepository: IRepository<ChartOfAccount>
    {
        Task<List<SelectListItem>> GetMainAccount(CancellationToken cancellationToken = default);

        Task<List<SelectListItem>> GetMemberAccount(string parentAcc, CancellationToken cancellationToken = default);

        Task<ChartOfAccount> GenerateAccount(ChartOfAccount model, string thirdLevel,
            CancellationToken cancellationToken = default);

        Task UpdateAsync(ChartOfAccount model, CancellationToken cancellationToken = default);

        IEnumerable<ChartOfAccountDto> GetSummaryReportView(CancellationToken cancellationToken = default);

        Task<ChartOfAccount?> GetAsyncIgnoreQueryFilters(Expression<Func<ChartOfAccount, bool>> filter, CancellationToken cancellationToken = default);

        Task<IEnumerable<ChartOfAccount>> GetAllAsyncIgnoreQueryFilters(Expression<Func<ChartOfAccount, bool>>? filter = null, CancellationToken cancellationToken = default);

        IQueryable<ChartOfAccount> GetAllQueryIgnoreQueryFilters(Expression<Func<ChartOfAccount, bool>>? filter = null);
    }
}
