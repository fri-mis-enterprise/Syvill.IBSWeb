using System.Linq.Expressions;
using System.Security.Claims;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
using IBS.Models.AccountsReceivable;
using IBS.Models.Common;
using IBS.Models.Enums;
using IBS.Models.MasterFile;
using IBS.Models.ViewModels;
using IBS.Utility.Constants;
using IBS.Utility.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Color = System.Drawing.Color;

namespace IBSWeb.Areas.User.Controllers
{
    [Area(nameof(User))]
    public class AccountsReceivableReport: Controller
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly ILogger<AccountsReceivableReport> _logger;

        public AccountsReceivableReport(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment webHostEnvironment,
            ILogger<AccountsReceivableReport> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _webHostEnvironment = webHostEnvironment;
            _logger = logger;
        }

        private string GetUserFullName()
        {
            return User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.GivenName)?.Value
                   ?? User.Identity?.Name!;
        }

        private static string NormalizeStatusFilter(string? statusFilter) => statusFilter switch
        {
            "All" => "All",
            "InvalidOnly" => "InvalidOnly",
            _ => "ValidOnly"
        };

        private static string GetStatusFilterLabel(string statusFilter) => statusFilter switch
        {
            "All" => "All (Include Voided)",
            "InvalidOnly" => "Voided Only",
            _ => "Valid Only (Exclude Voided)"
        };

        [HttpGet]
        public IActionResult PostedCollection()
        {
            return View();
        }

        #region -- Generate Posted Collection Excel File --

        public async Task<IActionResult> GeneratePostedCollectionExcelFile(ViewModelBook model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["warning"] = "Please input date range";
                return RedirectToAction(nameof(PostedCollection));
            }

            try
            {
                var dateFrom = model.DateFrom;
                var dateTo = model.DateTo;
                var extractedBy = GetUserFullName();
                var statusFilter = NormalizeStatusFilter(model.StatusFilter);

                var collectionReceiptReport = await _unitOfWork.Report
                    .GetCollectionReceiptReport(model.DateFrom, model.DateTo, statusFilter, cancellationToken);

                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("COLLECTION");

                var mergedCells = worksheet.Cells["A1:C1"];
                mergedCells.Merge = true;
                mergedCells.Value = "COLLECTION";
                mergedCells.Style.Font.Size = 16;

                worksheet.Cells["A2"].Value = "Date Range:";
                worksheet.Cells["A3"].Value = "Extracted By:";
                worksheet.Cells["A4"].Value = "Company:";
                worksheet.Cells["A5"].Value = "Status Filter:";

                worksheet.Cells["B2"].Value = $"{dateFrom} - {dateTo}";
                worksheet.Cells["B3"].Value = $"{extractedBy}";
                worksheet.Cells["B4"].Value = $"Syvill";
                worksheet.Cells["B5"].Value = GetStatusFilterLabel(statusFilter);

                bool showVoidCancelColumns = statusFilter != "ValidOnly";

                worksheet.Cells["A7"].Value = "CUSTOMER No.";
                worksheet.Cells["B7"].Value = "CUSTOMER NAME";
                worksheet.Cells["C7"].Value = "ACCT. TYPE";
                worksheet.Cells["D7"].Value = "TRAN. DATE (INV)";
                worksheet.Cells["E7"].Value = "CR No.";
                worksheet.Cells["F7"].Value = "INVOICE No.";
                worksheet.Cells["G7"].Value = "DUE DATE";
                worksheet.Cells["H7"].Value = "DATE OF CHECK";
                worksheet.Cells["I7"].Value = "BANK";
                worksheet.Cells["J7"].Value = "CHECK No.";
                worksheet.Cells["K7"].Value = "AMOUNT";

                if (showVoidCancelColumns)
                {
                    worksheet.Cells["L7"].Value = "VOIDED BY";
                    worksheet.Cells["M7"].Value = "VOIDED DATE";
                }

                string headerEndColumn = showVoidCancelColumns ? "M7" : "K7";
                var headerCells = worksheet.Cells[$"A7:{headerEndColumn}"];
                headerCells.Style.Font.Size = 11;
                headerCells.Style.Fill.PatternType = ExcelFillStyle.Solid;
                headerCells.Style.Fill.BackgroundColor.SetColor(Color.DarkGray);
                headerCells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                headerCells.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                headerCells.Style.Font.Bold = true;

                var row = 8;
                var startingRow = row - 1;
                var currencyFormat = "#,##0.00";
                decimal totalAmount = 0;

                foreach (var cr in collectionReceiptReport)
                {
                    var currentAmount = cr.CashAmount + cr.CheckAmount;
                    worksheet.Cells[row, 1].Value = cr.Customer?.CustomerCode;
                    worksheet.Cells[row, 2].Value = cr.ServiceInvoice?.CustomerName;
                    worksheet.Cells[row, 3].Value = cr.ServiceInvoice?.CreatedDate;
                    worksheet.Cells[row, 4].Value = cr.CollectionReceiptNo;
                    worksheet.Cells[row, 5].Value = cr.ServiceInvoice?.ServiceInvoiceNo;
                    worksheet.Cells[row, 6].Value = cr.ServiceInvoice?.DueDate;
                    worksheet.Cells[row, 7].Value = cr.CheckDate;
                    worksheet.Cells[row, 8].Value = $"{cr.BankAccount?.Bank} {cr.BankAccountNumber}";
                    worksheet.Cells[row, 9].Value = cr.CheckNo;
                    worksheet.Cells[row, 10].Value = currentAmount;

                    worksheet.Cells[row, 3].Style.Numberformat.Format = "MMM/dd/yyyy";
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "MMM/dd/yyyy";
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "MMM/dd/yyyy";
                    worksheet.Cells[row, 10].Style.Numberformat.Format = currencyFormat;

                    if (showVoidCancelColumns)
                    {
                        worksheet.Cells[row, 11].Value = cr.VoidedBy;
                        worksheet.Cells[row, 12].Value = cr.VoidedDate;
                        if (cr.VoidedDate.HasValue)
                        {
                            worksheet.Cells[row, 12].Style.Numberformat.Format = "MMM/dd/yyyy";
                        }
                    }

                    totalAmount += currentAmount;
                    row++;
                }

                int lastColumn = showVoidCancelColumns ? 12 : 10;

                worksheet.Cells[row, 9].Value = "Total:";
                worksheet.Cells[row, 10].Value = totalAmount;
                worksheet.Cells[row, 10].Style.Numberformat.Format = currencyFormat;

                using (var range = worksheet.Cells[row, 1, row, lastColumn])
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.Yellow);
                    range.Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }

                using (var range = worksheet.Cells[row, 10, row, 11])
                {
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Double;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Font.Bold = true;
                }

                int lastRow = row - 1;
                using (var range = worksheet.Cells[startingRow - 1, 11, lastRow, 11])
                {
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                }

                worksheet.Cells.AutoFitColumns();
                worksheet.View.FreezePanes(8, 1);

                #region -- Audit Trail --

                AuditTrail auditTrailBook = new(GetUserFullName(), "Generate posted collection report excel file",
                    "Accounts Receivable Report");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion

                var fileName = $"Collection_Report_{DateTimeHelper.GetCurrentPhilippineTime():yyyyddMMHHmmss}.xlsx";
                var stream = new MemoryStream();
                await package.SaveAsAsync(stream, cancellationToken);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                ViewData["error"] = ex.Message;
                _logger.LogError(ex,
                    "Failed to generate posted collection report excel file. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(PostedCollection));
            }
        }

        #endregion -- Generate Posted Collection Excel File --

        [HttpGet]
        public IActionResult AgingReport()
        {
            return View();
        }

        #region -- Generate Aging Report Excel File --

        public async Task<IActionResult> GenerateAgingReportExcelFile(ViewModelBook model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["warning"] = "Please input date range";
                return RedirectToAction(nameof(AgingReport));
            }

            try
            {
                var dateFrom = model.DateFrom;
                var dateTo = model.DateTo;
                var extractedBy = GetUserFullName();

                var serviceInvoices = await _unitOfWork.ServiceInvoice
                    .GetAllAsync(si => si.PostedBy != null
                                       && si.AmountPaid == 0 && !si.IsPaid, cancellationToken);

                if (!serviceInvoices.Any())
                {
                    TempData["info"] = "No Record Found";
                    return RedirectToAction(nameof(AgingReport));
                }

                // Create the Excel package
                using var package = new ExcelPackage();
                // Add a new worksheet to the Excel package
                var worksheet = package.Workbook.Worksheets.Add("AgingReport");

                // Set the column headers
                var mergedCells = worksheet.Cells["A1:C1"];
                mergedCells.Merge = true;
                mergedCells.Value = "AGING REPORT";
                mergedCells.Style.Font.Size = 13;

                worksheet.Cells["A2"].Value = "Date Range:";
                worksheet.Cells["A3"].Value = "Extracted By:";
                worksheet.Cells["A4"].Value = "Company:";

                worksheet.Cells["B2"].Value = $"{dateFrom} - {dateTo}";
                worksheet.Cells["B3"].Value = $"{extractedBy}";
                worksheet.Cells["B4"].Value = $"Syvill";

                worksheet.Cells["A7"].Value = "MONTH";
                worksheet.Cells["B7"].Value = "CUSTOMER NAME";
                worksheet.Cells["C7"].Value = "ACCT. TYPE";
                worksheet.Cells["D7"].Value = "TERMS";
                worksheet.Cells["E7"].Value = "EWT %";
                worksheet.Cells["F7"].Value = "SALES DATE";
                worksheet.Cells["G7"].Value = "DUE DATE";
                worksheet.Cells["H7"].Value = "INVOICE No.";
                worksheet.Cells["I7"].Value = "DR";
                worksheet.Cells["J7"].Value = "GROSS";
                worksheet.Cells["K7"].Value = "PARTIAL COLLECTIONS";
                worksheet.Cells["L7"].Value = "ADJUSTED GROSS";
                worksheet.Cells["M7"].Value = "EWT";
                worksheet.Cells["N7"].Value = "NET OF VAT";
                worksheet.Cells["O7"].Value = "VCF";
                worksheet.Cells["P7"].Value = "RETENTION AMOUNT";
                worksheet.Cells["Q7"].Value = "ADJUSTED NET";
                worksheet.Cells["R7"].Value = "DAYS DUE";
                worksheet.Cells["S7"].Value = "CURRENT";
                worksheet.Cells["T7"].Value = "1-30 DAYS";
                worksheet.Cells["U7"].Value = "31-60 DAYS";
                worksheet.Cells["V7"].Value = "61-90 DAYS";
                worksheet.Cells["W7"].Value = "OVER 90 DAYS";

                // Apply styling to the header row
                using (var range = worksheet.Cells["A7:W7"])
                {
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                // Populate the data rows
                int row = 8;
                string currencyFormat = "#,##0.00";

                var totalGrossAmount = 0m;
                var totalAmountPaid = 0m;
                var totalAdjustedGross = 0m;
                var totalWithHoldingTaxAmount = 0m;
                var totalNetOfVatAmount = 0m;
                var totalVcfAmount = 0m;
                var totalRetentionAmount = 0m;
                var totalAdjustedNet = 0m;
                var totalCurrent = 0m;
                var totalOneToThirtyDays = 0m;
                var totalThirtyOneToSixtyDays = 0m;
                var totalSixtyOneToNinetyDays = 0m;
                var totalOverNinetyDays = 0m;
                var repoCalculator = _unitOfWork.ServiceInvoice;

                foreach (var sv in serviceInvoices)
                {
                    var gross = sv.Total;
                    var netDiscount = sv.Total - sv.Discount;
                    var netOfVatAmount = sv.VatType == SD.VatType_Vatable
                        ? repoCalculator.ComputeNetOfVat(netDiscount)
                        : netDiscount;
                    var withHoldingTaxAmount = sv.HasEwt
                        ? repoCalculator.ComputeEwtAmount(netDiscount, 0.01m)
                        : 0;
                    var retentionAmount = (sv.Customer?.RetentionRate ?? 0.0000m) * netOfVatAmount;
                    var vcfAmount = 0.0000m;
                    var adjustedGross = gross - vcfAmount;
                    var adjustedNet = gross - vcfAmount - retentionAmount;

                    var today = DateOnly.FromDateTime(DateTime.Today);
                    var daysDue = (today > sv.DueDate) ? (today.DayNumber - sv.DueDate.DayNumber) : 0;
                    var current = (sv.DueDate >= today) ? gross : 0.0000m;
                    var oneToThirtyDays = (daysDue >= 1 && daysDue <= 30) ? gross : 0.0000m;
                    var thirtyOneToSixtyDays = (daysDue >= 31 && daysDue <= 60) ? gross : 0.0000m;
                    var sixtyOneToNinetyDays = (daysDue >= 61 && daysDue <= 90) ? gross : 0.0000m;
                    var overNinetyDays = (daysDue > 90) ? gross : 0.0000m;

                    worksheet.Cells[row, 1].Value = sv.Period.ToString("MMMM yyyy");
                    worksheet.Cells[row, 2].Value = sv.CustomerName;
                    worksheet.Cells[row, 3].Value = "NA";
                    worksheet.Cells[row, 4].Value = sv.HasEwt ? "1" : "0";
                    worksheet.Cells[row, 5].Value = sv.Period.ToString("MMMM yyyy");
                    worksheet.Cells[row, 6].Value = sv.DueDate.ToString("MMMM yyyy");
                    worksheet.Cells[row, 7].Value = sv.ServiceInvoiceNo;
                    worksheet.Cells[row, 8].Value = gross;
                    worksheet.Cells[row, 9].Value = sv.AmountPaid;
                    worksheet.Cells[row, 10].Value = adjustedGross;
                    worksheet.Cells[row, 11].Value = withHoldingTaxAmount;
                    worksheet.Cells[row, 12].Value = netOfVatAmount;
                    worksheet.Cells[row, 13].Value = vcfAmount;
                    worksheet.Cells[row, 14].Value = retentionAmount;
                    worksheet.Cells[row, 15].Value = adjustedNet;
                    worksheet.Cells[row, 16].Value = daysDue;
                    worksheet.Cells[row, 17].Value = current;
                    worksheet.Cells[row, 18].Value = oneToThirtyDays;
                    worksheet.Cells[row, 19].Value = thirtyOneToSixtyDays;
                    worksheet.Cells[row, 20].Value = sixtyOneToNinetyDays;
                    worksheet.Cells[row, 21].Value = overNinetyDays;

                    worksheet.Cells[row, 9].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 10].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 11].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 12].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 13].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 14].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 15].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 16].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 17].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 18].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 19].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 20].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 21].Style.Numberformat.Format = currencyFormat;

                    row++;

                    totalGrossAmount += sv.Total;
                    totalAmountPaid += sv.AmountPaid;
                    totalAdjustedGross += adjustedGross;
                    totalWithHoldingTaxAmount += withHoldingTaxAmount;
                    totalNetOfVatAmount += netOfVatAmount;
                    totalVcfAmount += vcfAmount;
                    totalRetentionAmount += retentionAmount;
                    totalAdjustedNet += adjustedNet;
                    totalCurrent += current;
                    totalOneToThirtyDays += oneToThirtyDays;
                    totalThirtyOneToSixtyDays += thirtyOneToSixtyDays;
                    totalSixtyOneToNinetyDays += sixtyOneToNinetyDays;
                    totalOverNinetyDays += overNinetyDays;
                }

                worksheet.Cells[row, 8].Value = "Total ";
                worksheet.Cells[row, 9].Value = totalGrossAmount;
                worksheet.Cells[row, 10].Value = totalAmountPaid;
                worksheet.Cells[row, 11].Value = totalAdjustedGross;
                worksheet.Cells[row, 12].Value = totalWithHoldingTaxAmount;
                worksheet.Cells[row, 13].Value = totalNetOfVatAmount;
                worksheet.Cells[row, 14].Value = totalVcfAmount;
                worksheet.Cells[row, 15].Value = totalRetentionAmount;
                worksheet.Cells[row, 16].Value = totalAdjustedNet;
                worksheet.Cells[row, 17].Value = totalCurrent;
                worksheet.Cells[row, 18].Value = totalOneToThirtyDays;
                worksheet.Cells[row, 19].Value = totalThirtyOneToSixtyDays;
                worksheet.Cells[row, 20].Value = totalSixtyOneToNinetyDays;
                worksheet.Cells[row, 21].Value = totalOverNinetyDays;

                worksheet.Cells[row, 9].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 10].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 11].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 12].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 13].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 14].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 15].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 16].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 17].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 18].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 19].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 20].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 21].Style.Numberformat.Format = currencyFormat;

                // Apply style to subtotal row
                using (var range = worksheet.Cells[row, 1, row, 21])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(172, 185, 202));
                }

                using (var range = worksheet.Cells[row, 8, row, 21])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin; // Single top border
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Double; // Double bottom border
                }

                // Auto-fit columns for better readability
                worksheet.Cells.AutoFitColumns();
                worksheet.View.FreezePanes(8, 1);

                #region -- Audit Trail --

                AuditTrail auditTrailBook = new(GetUserFullName(), "Generate aging report excel file",
                    "Accounts Receivable Report");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion

                var fileName = $"Aging_Report_{DateTimeHelper.GetCurrentPhilippineTime():yyyyddMMHHmmss}.xlsx";
                var stream = new MemoryStream();
                await package.SaveAsAsync(stream, cancellationToken);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex,
                    "Failed to generate aging report excel file. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(AgingReport));
            }
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> ArPerCustomer()
        {
            ViewModelBook viewmodel = new() { CustomerList = await _unitOfWork.GetCustomerListAsyncById() };

            return View(viewmodel);
        }

        #region -- Generate AR Per Customer Excel File --

        public async Task<IActionResult> GenerateArPerCustomerExcelFile(ViewModelBook model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["warning"] = "Please input date range";
                return RedirectToAction(nameof(ArPerCustomer));
            }

            try
            {
                var dateFrom = model.DateFrom;
                var dateTo = model.DateTo;
                var extractedBy = GetUserFullName();

                var statusFilter = NormalizeStatusFilter(model.StatusFilter);

                var salesInvoice = await _unitOfWork.Report
                    .GetARPerCustomerReport(model.DateFrom, model.DateTo, model.Customers, statusFilter,
                        cancellationToken);

                if (!salesInvoice.Any())
                {
                    TempData["info"] = "No Record Found";
                    return RedirectToAction(nameof(ArPerCustomer));
                }

                // Create the Excel package
                using var package = new ExcelPackage();

                // Audit info columns — only for All or InvalidOnly
                bool showVoidCancelColumns = statusFilter != "ValidOnly";

                // Add a new worksheet to the Excel package
                var worksheet = package.Workbook.Worksheets.Add("ARPerCustomer");

                // Set the column headers
                var mergedCells = worksheet.Cells["A1:C1"];
                mergedCells.Merge = true;
                mergedCells.Value = "AR PER CUSTOMER";
                mergedCells.Style.Font.Size = 13;

                worksheet.Cells["A2"].Value = "Date Range:";
                worksheet.Cells["A3"].Value = "Extracted By:";
                worksheet.Cells["A4"].Value = "Company:";
                worksheet.Cells["A5"].Value = "Status Filter:";

                worksheet.Cells["B2"].Value =
                    $"{dateFrom.ToString(SD.Date_Format)} - {dateTo.ToString(SD.Date_Format)}";
                worksheet.Cells["B3"].Value = $"{extractedBy}";
                worksheet.Cells["B4"].Value = $"Syvill";
                worksheet.Cells["B5"].Value = GetStatusFilterLabel(statusFilter);

                worksheet.Cells["A7"].Value = "CUSTOMER No.";
                worksheet.Cells["B7"].Value = "CUSTOMER NAME";
                worksheet.Cells["C7"].Value = "ACCT. TYPE";
                worksheet.Cells["D7"].Value = "TERMS";
                worksheet.Cells["E7"].Value = "TRAN. DATE";
                worksheet.Cells["F7"].Value = "DUE DATE";
                worksheet.Cells["G7"].Value = "INVOICE No.";
                worksheet.Cells["H7"].Value = "DR No.";
                worksheet.Cells["I7"].Value = "PO No.";
                worksheet.Cells["J7"].Value = "COS No.";
                worksheet.Cells["K7"].Value = "REMARKS";
                worksheet.Cells["L7"].Value = "PRODUCT";
                worksheet.Cells["M7"].Value = "QTY";
                worksheet.Cells["N7"].Value = "UNIT";
                worksheet.Cells["O7"].Value = "UNIT PRICE";
                worksheet.Cells["P7"].Value = "FREIGHT";
                worksheet.Cells["Q7"].Value = "FREIGHT/LTR";
                worksheet.Cells["R7"].Value = "VAT/LTR";
                worksheet.Cells["S7"].Value = "VAT AMT.";
                worksheet.Cells["T7"].Value = "TOTAL AMT. (G. VAT)";
                worksheet.Cells["U7"].Value = "AMT. PAID";
                worksheet.Cells["V7"].Value = "SI BALANCE";
                worksheet.Cells["W7"].Value = "EWT AMT.";
                worksheet.Cells["X7"].Value = "EWT PAID";
                worksheet.Cells["Y7"].Value = "CWT BALANCE";

                // Add void/cancel columns — only for All or InvalidOnly
                if (showVoidCancelColumns)
                {
                    worksheet.Cells["Z7"].Value = "VOIDED BY";
                    worksheet.Cells["AA7"].Value = "VOIDED DATE";
                }

                // Apply styling to the header row
                string headerEndColumn = showVoidCancelColumns ? "AA7" : "Y7";
                using (var range = worksheet.Cells[$"A7:{headerEndColumn}"])
                {
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                // Populate the data rows
                int row = 8;
                string currencyFormat = "#,##0.0000";
                string currencyFormatTwoDecimal = "#,##0.00";

                var totalQuantity = 0m;
                var totalFreight = 0m;
                var totalFreightPerLiter = 0m;
                var totalVatPerLiter = 0m;
                var totalVatAmount = 0m;
                var totalGrossAmount = 0m;
                var totalAmountPaid = 0m;
                var totalBalance = 0m;
                var totalEwtAmount = 0m;
                var totalEwtAmountPaid = 0m;
                var totalEwtBalance = 0m;
                var repoCalculator = _unitOfWork.ServiceInvoice;

                foreach (var groupByCustomer in salesInvoice.GroupBy(x => x.Customer))
                {
                    foreach (var sv in groupByCustomer)
                    {
                        var isVatable = sv.VatType == SD.VatType_Vatable;
                        var isTaxable = sv.HasEwt;
                        var grossAmount = sv.Total;
                        var netOfVat = isVatable
                            ? repoCalculator.ComputeNetOfVat(grossAmount)
                            : grossAmount;
                        var vatAmount = isVatable ? repoCalculator.ComputeVatAmount(netOfVat) : 0m;
                        //var vatPerLiter = vatAmount / sv.Quantity;
                        var ewtAmount = isTaxable ? repoCalculator.ComputeEwtAmount(netOfVat, 0.01m) : 0m;
                        //var isEwtAmountPaid = sv.IsTaxAndVatPaid ? ewtAmount : 0m;
                        // var ewtBalance = ewtAmount - isEwtAmountPaid;

                        worksheet.Cells[row, 1].Value = sv.Customer?.CustomerCode;
                        // worksheet.Cells[row, 2].Value = sv.CustomerOrderSlip?.CustomerName ?? sv.Customer?.CustomerName;
                        // worksheet.Cells[row, 3].Value = sv.CustomerOrderSlip?.CustomerType ?? sv.Customer?.CustomerType;
                        // worksheet.Cells[row, 4].Value = sv.Terms;
                        // worksheet.Cells[row, 5].Value = sv.TransactionDate;
                        // worksheet.Cells[row, 6].Value = sv.DueDate;
                        // worksheet.Cells[row, 7].Value = sv.SalesInvoiceNo;
                        // worksheet.Cells[row, 8].Value = sv.DeliveryReceipt?.DeliveryReceiptNo;
                        // worksheet.Cells[row, 9].Value = sv.CustomerOrderSlip?.CustomerPoNo;
                        // worksheet.Cells[row, 10].Value = sv.CustomerOrderSlip?.CustomerOrderSlipNo;
                        // worksheet.Cells[row, 11].Value = sv.Remarks;
                        // worksheet.Cells[row, 12].Value = sv.CustomerOrderSlip?.ProductName;
                        // worksheet.Cells[row, 13].Value = sv.Quantity;
                        // worksheet.Cells[row, 14].Value = sv.Product?.ProductUnit;
                        // worksheet.Cells[row, 15].Value = sv.UnitPrice;
                        // worksheet.Cells[row, 16].Value = freight;
                        // worksheet.Cells[row, 17].Value = sv.DeliveryReceipt?.Freight;
                        // worksheet.Cells[row, 18].Value = vatPerLiter;
                        // worksheet.Cells[row, 19].Value = vatAmount;
                        // worksheet.Cells[row, 20].Value = grossAmount;
                        // worksheet.Cells[row, 21].Value = sv.AmountPaid;
                        // worksheet.Cells[row, 22].Value = sv.Balance;
                        // worksheet.Cells[row, 23].Value = ewtAmount;
                        // worksheet.Cells[row, 24].Value = isEwtAmountPaid;
                        //worksheet.Cells[row, 25].Value = ewtBalance;

                        // Add void/cancel data — only for All or InvalidOnly
                        if (showVoidCancelColumns)
                        {
                            worksheet.Cells[row, 26].Value = sv.VoidedBy;
                            worksheet.Cells[row, 27].Value = sv.VoidedDate;
                            if (sv.VoidedDate.HasValue)
                            {
                                worksheet.Cells[row, 27].Style.Numberformat.Format = "MMM/dd/yyyy";
                            }
                        }

                        worksheet.Cells[row, 5].Style.Numberformat.Format = "MMM/dd/yyyy";
                        worksheet.Cells[row, 6].Style.Numberformat.Format = "MMM/dd/yyyy";
                        worksheet.Cells[row, 13].Style.Numberformat.Format = currencyFormatTwoDecimal;
                        worksheet.Cells[row, 15].Style.Numberformat.Format = currencyFormat;
                        worksheet.Cells[row, 16].Style.Numberformat.Format = currencyFormatTwoDecimal;
                        worksheet.Cells[row, 17].Style.Numberformat.Format = currencyFormat;
                        worksheet.Cells[row, 18].Style.Numberformat.Format = currencyFormatTwoDecimal;
                        worksheet.Cells[row, 19].Style.Numberformat.Format = currencyFormatTwoDecimal;
                        worksheet.Cells[row, 20].Style.Numberformat.Format = currencyFormatTwoDecimal;
                        worksheet.Cells[row, 21].Style.Numberformat.Format = currencyFormatTwoDecimal;
                        worksheet.Cells[row, 22].Style.Numberformat.Format = currencyFormatTwoDecimal;
                        worksheet.Cells[row, 23].Style.Numberformat.Format = currencyFormatTwoDecimal;
                        worksheet.Cells[row, 24].Style.Numberformat.Format = currencyFormatTwoDecimal;
                        worksheet.Cells[row, 25].Style.Numberformat.Format = currencyFormatTwoDecimal;

                        row++;

                        // totalQuantity += sv.Quantity;
                        // totalFreight += freight ?? 0m;
                        // totalFreightPerLiter += sv.DeliveryReceipt?.Freight ?? 0m;
                        // totalVatPerLiter += vatPerLiter;
                        // totalVatAmount += vatAmount;
                        // totalGrossAmount += grossAmount;
                        // totalAmountPaid += sv.AmountPaid;
                        // totalBalance += sv.Balance;
                        // totalEwtAmount += ewtAmount;
                        // totalEwtAmountPaid += isEwtAmountPaid;
                        // totalEwtBalance += ewtBalance;
                    }
                    // var subTotalQuantity = groupByCustomer.Sum(x => x.Quantity);
                    //
                    // var isVatableSub = groupByCustomer.Select(x => x.CustomerOrderSlip?.VatType).FirstOrDefault();
                    // var isTaxableSub = groupByCustomer.Select(x => x.CustomerOrderSlip?.HasEWT).FirstOrDefault();
                    // var subTotalFreight = groupByCustomer.Sum(x => x.DeliveryReceipt?.FreightAmount) ?? 0m;
                    // var subTotalFreightPerLiter = subTotalFreight != 0m && subTotalQuantity != 0m ? subTotalFreight / subTotalQuantity : 0m;
                    // var subTotalGrossAmount = groupByCustomer.Sum(x => x.Amount);
                    // var subTotalNetOfVat = isVatableSub == SD.VatType_Vatable
                    //     ? repoCalculator.ComputeNetOfVat(subTotalGrossAmount)
                    //     : subTotalGrossAmount;
                    // var subTotalVatAmount = isVatableSub == SD.VatType_Vatable
                    //     ? repoCalculator.ComputeVatAmount(subTotalNetOfVat)
                    //     : 0m;
                    // var subTotalAmountPaid = groupByCustomer.Sum(x => x.AmountPaid);
                    // var subTotalVatPerLiter = subTotalVatAmount / subTotalQuantity;
                    // var subTotalEwtAmount = isTaxableSub == true
                    //     ? repoCalculator.ComputeEwtAmount(subTotalNetOfVat, 0.01m)
                    //     : 0m;
                    // var isEwtAmountPaidSub = groupByCustomer.Select(x => x.IsTaxAndVatPaid).FirstOrDefault() ? subTotalEwtAmount : 0m;
                    // var subTotalEwtBalance = subTotalEwtAmount - isEwtAmountPaidSub;
                    // var subTotalUnitPrice = subTotalGrossAmount / subTotalQuantity;
                    // var subTotalBalance = groupByCustomer.Sum(x => x.Balance);
                    // var subTotalEwtAmountPaid = isEwtAmountPaidSub;

                    worksheet.Cells[row, 12].Value = "SUB TOTAL ";

                    // worksheet.Cells[row, 13].Value = subTotalQuantity;
                    // worksheet.Cells[row, 15].Value = subTotalUnitPrice;
                    // worksheet.Cells[row, 16].Value = subTotalFreight;
                    // worksheet.Cells[row, 17].Value = subTotalFreightPerLiter;
                    // worksheet.Cells[row, 18].Value = subTotalVatPerLiter;
                    // worksheet.Cells[row, 19].Value = subTotalVatAmount;
                    // worksheet.Cells[row, 20].Value = subTotalGrossAmount;
                    // worksheet.Cells[row, 21].Value = subTotalAmountPaid;
                    // worksheet.Cells[row, 22].Value = subTotalBalance;
                    // worksheet.Cells[row, 23].Value = subTotalEwtAmount;
                    // worksheet.Cells[row, 24].Value = subTotalEwtAmountPaid;
                    // worksheet.Cells[row, 25].Value = subTotalEwtBalance;

                    worksheet.Cells[row, 13].Style.Numberformat.Format = currencyFormatTwoDecimal;
                    worksheet.Cells[row, 15].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 16].Style.Numberformat.Format = currencyFormatTwoDecimal;
                    worksheet.Cells[row, 17].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 18].Style.Numberformat.Format = currencyFormatTwoDecimal;
                    worksheet.Cells[row, 19].Style.Numberformat.Format = currencyFormatTwoDecimal;
                    worksheet.Cells[row, 20].Style.Numberformat.Format = currencyFormatTwoDecimal;
                    worksheet.Cells[row, 21].Style.Numberformat.Format = currencyFormatTwoDecimal;
                    worksheet.Cells[row, 22].Style.Numberformat.Format = currencyFormatTwoDecimal;
                    worksheet.Cells[row, 23].Style.Numberformat.Format = currencyFormatTwoDecimal;
                    worksheet.Cells[row, 24].Style.Numberformat.Format = currencyFormatTwoDecimal;
                    worksheet.Cells[row, 25].Style.Numberformat.Format = currencyFormatTwoDecimal;

                    // Apply style to sub total row
                    int lastColumn = showVoidCancelColumns ? 27 : 25;
                    using (var range = worksheet.Cells[row, 1, row, lastColumn])
                    {
                        range.Style.Font.Bold = true;
                        range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                        range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(172, 185, 202));
                    }

                    row++;
                }

                totalFreightPerLiter = totalFreight != 0 && totalQuantity != 0 ? totalFreight / totalQuantity : 0m;

                worksheet.Cells[row, 12].Value = "GRAND TOTAL ";

                worksheet.Cells[row, 13].Value = totalQuantity;
                worksheet.Cells[row, 15].Value = totalGrossAmount / totalQuantity;
                worksheet.Cells[row, 16].Value = totalFreight;
                worksheet.Cells[row, 17].Value = totalFreightPerLiter;
                worksheet.Cells[row, 18].Value = totalVatPerLiter;
                worksheet.Cells[row, 19].Value = totalVatAmount;
                worksheet.Cells[row, 20].Value = totalGrossAmount;
                worksheet.Cells[row, 21].Value = totalAmountPaid;
                worksheet.Cells[row, 22].Value = totalBalance;
                worksheet.Cells[row, 23].Value = totalEwtAmount;
                worksheet.Cells[row, 24].Value = totalEwtAmountPaid;
                worksheet.Cells[row, 25].Value = totalEwtBalance;

                worksheet.Cells[row, 13].Style.Numberformat.Format = currencyFormatTwoDecimal;
                worksheet.Cells[row, 15].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 16].Style.Numberformat.Format = currencyFormatTwoDecimal;
                worksheet.Cells[row, 17].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 18].Style.Numberformat.Format = currencyFormatTwoDecimal;
                worksheet.Cells[row, 19].Style.Numberformat.Format = currencyFormatTwoDecimal;
                worksheet.Cells[row, 20].Style.Numberformat.Format = currencyFormatTwoDecimal;
                worksheet.Cells[row, 21].Style.Numberformat.Format = currencyFormatTwoDecimal;
                worksheet.Cells[row, 22].Style.Numberformat.Format = currencyFormatTwoDecimal;
                worksheet.Cells[row, 23].Style.Numberformat.Format = currencyFormatTwoDecimal;
                worksheet.Cells[row, 24].Style.Numberformat.Format = currencyFormatTwoDecimal;
                worksheet.Cells[row, 25].Style.Numberformat.Format = currencyFormatTwoDecimal;

                // Apply style to grand total row
                int grandTotalLastColumn = showVoidCancelColumns ? 27 : 25;
                using (var range = worksheet.Cells[row, 1, row, grandTotalLastColumn])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(172, 185, 202));
                }

                using (var range = worksheet.Cells[row, 12, row, grandTotalLastColumn])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Double;
                }

                // Auto-fit columns for better readability
                worksheet.Cells.AutoFitColumns();
                worksheet.View.FreezePanes(8, 1);

                #region -- Audit Trail --

                AuditTrail auditTrailBook = new(GetUserFullName(), "Generate ar per customer report excel file",
                    "Accounts Receivable Report");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion

                var fileName =
                    $"AR_Per_Customer_Report_{DateTimeHelper.GetCurrentPhilippineTime():yyyyddMMHHmmss}.xlsx";
                var stream = new MemoryStream();
                await package.SaveAsAsync(stream, cancellationToken);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex,
                    "Failed to generate ar per customer report excel file. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(ArPerCustomer));
            }
        }

        #endregion

        [HttpGet]
        public IActionResult ServiceInvoiceReport()
        {
            return View();
        }

        #region -- Generated Service Invoice Report as Quest PDF

        public async Task<IActionResult> GeneratedServiceInvoiceReport(ViewModelBook model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The submitted information is invalid.";
                return RedirectToAction(nameof(ServiceInvoiceReport));
            }

            var statusFilter = NormalizeStatusFilter(model.StatusFilter);

            try
            {
                var serviceInvoice = await _unitOfWork.Report
                    .GetServiceInvoiceReport(model.DateFrom, model.DateTo, statusFilter, cancellationToken);

                if (!serviceInvoice.Any())
                {
                    TempData["info"] = "No records found!";
                    return RedirectToAction(nameof(ServiceInvoiceReport));
                }

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        #region -- Page Setup

                        page.Size(PageSizes.Legal.Landscape());
                        page.Margin(20);
                        page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Times New Roman"));

                        #endregion

                        #region -- Header

                        var imgFilprideLogoPath =
                            Path.Combine(_webHostEnvironment.WebRootPath, "img", "Filpride-logo.png");

                        page.Header().Height(50).Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item()
                                    .Text("SERVICE REPORT")
                                    .FontSize(20).SemiBold();

                                column.Item().Text(text =>
                                {
                                    text.Span("Date From: ").SemiBold();
                                    text.Span(model.DateFrom.ToString(SD.Date_Format));
                                });

                                column.Item().Text(text =>
                                {
                                    text.Span("Date To: ").SemiBold();
                                    text.Span(model.DateTo.ToString(SD.Date_Format));
                                });
                            });

                            row.ConstantItem(size: 100)
                                .Height(50)
                                .Image(Image.FromFile(imgFilprideLogoPath)).FitWidth();
                        });

                        #endregion

                        #region -- Content

                        page.Content().PaddingTop(10).Table(table =>
                        {
                            #region -- Columns Definition

                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });

                            #endregion

                            #region -- Table Header

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter()
                                    .AlignMiddle().Text("Transaction Date").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter()
                                    .AlignMiddle().Text("Customer Name").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter()
                                    .AlignMiddle().Text("Customer Address").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter()
                                    .AlignMiddle().Text("Customer TIN").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter()
                                    .AlignMiddle().Text("Service Invoice#").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter()
                                    .AlignMiddle().Text("Service").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter()
                                    .AlignMiddle().Text("Period").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter()
                                    .AlignMiddle().Text("Due Date").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter()
                                    .AlignMiddle().Text("G. Amount").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter()
                                    .AlignMiddle().Text("Amount Paid").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter()
                                    .AlignMiddle().Text("Payment Status").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter()
                                    .AlignMiddle().Text("Instructions").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter()
                                    .AlignMiddle().Text("Type").SemiBold();
                            });

                            #endregion

                            #region -- Loop to Show Records

                            var totalAmount = 0m;
                            var totalAmountPaid = 0m;

                            foreach (var record in serviceInvoice)
                            {
                                table.Cell().Border(0.5f).Padding(3).Text(record.CreatedDate.ToString(SD.Date_Format));
                                table.Cell().Border(0.5f).Padding(3).Text(record.CustomerName);
                                table.Cell().Border(0.5f).Padding(3).Text(record.CustomerAddress);
                                table.Cell().Border(0.5f).Padding(3).Text(record.CustomerTin);
                                table.Cell().Border(0.5f).Padding(3).Text(record.ServiceInvoiceNo);
                                table.Cell().Border(0.5f).Padding(3).Text(record.ServiceName);
                                table.Cell().Border(0.5f).Padding(3).Text(record.Period.ToString(SD.Date_Format));
                                table.Cell().Border(0.5f).Padding(3).Text(record.DueDate.ToString(SD.Date_Format));
                                table.Cell().Border(0.5f).Padding(3).AlignRight()
                                    .Text(record.Total != 0
                                        ? record.Total < 0
                                            ? $"({Math.Abs(record.Total).ToString(SD.Two_Decimal_Format)})"
                                            : record.Total.ToString(SD.Two_Decimal_Format)
                                        : null).FontColor(record.Total < 0 ? Colors.Red.Medium : Colors.Black);
                                table.Cell().Border(0.5f).Padding(3).AlignRight().Text(record.AmountPaid != 0
                                    ? record.AmountPaid < 0
                                        ? $"({Math.Abs(record.AmountPaid).ToString(SD.Two_Decimal_Format)})"
                                        : record.AmountPaid.ToString(SD.Two_Decimal_Format)
                                    : null).FontColor(record.AmountPaid < 0 ? Colors.Red.Medium : Colors.Black);
                                table.Cell().Border(0.5f).Padding(3).Text(record.PaymentStatus);
                                table.Cell().Border(0.5f).Padding(3).Text(record.Instructions);
                                table.Cell().Border(0.5f).Padding(3).Text(record.Type);

                                totalAmount += record.Total;
                                totalAmountPaid += record.AmountPaid;
                            }

                            #endregion

                            #region -- Create Table Cell for Totals

                            table.Cell().ColumnSpan(8).Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3)
                                .AlignRight().Text("TOTAL:").SemiBold();
                            table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight()
                                .Text(totalAmount != 0
                                    ? totalAmount < 0
                                        ? $"({Math.Abs(totalAmount).ToString(SD.Two_Decimal_Format)})"
                                        : totalAmount.ToString(SD.Two_Decimal_Format)
                                    : null).FontColor(totalAmount < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                            table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight()
                                .Text(totalAmountPaid != 0
                                    ? totalAmountPaid < 0
                                        ? $"({Math.Abs(totalAmountPaid).ToString(SD.Two_Decimal_Format)})"
                                        : totalAmountPaid.ToString(SD.Two_Decimal_Format)
                                    : null).FontColor(totalAmountPaid < 0 ? Colors.Red.Medium : Colors.Black)
                                .SemiBold();
                            table.Cell().ColumnSpan(3).Background(Colors.Grey.Lighten1).Border(0.5f);

                            #endregion
                        });

                        #endregion

                        #region -- Footer

                        page.Footer().AlignRight().Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });

                        #endregion
                    });
                });

                #region -- Audit Trail --

                AuditTrail auditTrailBook = new(GetUserFullName(), "Generate service invoice report quest pdf",
                    "Accounts Receivable Report");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion

                var pdfBytes = document.GeneratePdf();
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex,
                    "Failed to generate service invoice report quest pdf. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(ServiceInvoiceReport));
            }
        }

        #endregion

        #region -- Generate Service Invoice Report Excel File --

        public async Task<IActionResult> GenerateServiceInvoiceReportExcelFile(ViewModelBook model,
            CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["warning"] = "Please input date range";
                return RedirectToAction(nameof(ServiceInvoiceReport));
            }

            try
            {
                var dateFrom = model.DateFrom;
                var dateTo = model.DateTo;
                var extractedBy = GetUserFullName();
                var statusFilter = NormalizeStatusFilter(model.StatusFilter);

                var serviceReport = await _unitOfWork.Report.GetServiceInvoiceReport(model.DateFrom, model.DateTo,
                    statusFilter, cancellationToken);

                if (serviceReport.Count == 0)
                {
                    TempData["info"] = "No Record Found";
                    return RedirectToAction(nameof(ServiceInvoiceReport));
                }

                // Create the Excel package
                using var package = new ExcelPackage();

                // Audit info columns — only for All or InvalidOnly
                bool showVoidCancelColumns = statusFilter != "ValidOnly";

                // Add a new worksheet to the Excel package
                var worksheet = package.Workbook.Worksheets.Add("ServiceReport");

                // Set the column headers
                var mergedCells = worksheet.Cells["A1:C1"];
                mergedCells.Merge = true;
                mergedCells.Value = "SERVICE REPORT";
                mergedCells.Style.Font.Size = 13;

                worksheet.Cells["A2"].Value = "Date Range:";
                worksheet.Cells["A3"].Value = "Extracted By:";
                worksheet.Cells["A4"].Value = "Company:";
                worksheet.Cells["A5"].Value = "Status Filter:";

                worksheet.Cells["B2"].Value = $"{dateFrom} - {dateTo}";
                worksheet.Cells["B3"].Value = $"{extractedBy}";
                worksheet.Cells["B4"].Value = $"Syvill";
                worksheet.Cells["B5"].Value = GetStatusFilterLabel(statusFilter);

                worksheet.Cells["A7"].Value = "Transaction Date";
                worksheet.Cells["B7"].Value = "Customer Name";
                worksheet.Cells["C7"].Value = "Customer Address";
                worksheet.Cells["D7"].Value = "Customer TIN";
                worksheet.Cells["E7"].Value = "Service Invoice#";
                worksheet.Cells["F7"].Value = "Service";
                worksheet.Cells["G7"].Value = "Period";
                worksheet.Cells["H7"].Value = "Due Date";
                worksheet.Cells["I7"].Value = "G. Amount";
                worksheet.Cells["J7"].Value = "Amount Paid";
                worksheet.Cells["K7"].Value = "Payment Status";
                worksheet.Cells["L7"].Value = "Instructions";
                worksheet.Cells["M7"].Value = "Type";

                // Add void/cancel columns — only for All or InvalidOnly
                if (showVoidCancelColumns)
                {
                    worksheet.Cells["N7"].Value = "VOIDED BY";
                    worksheet.Cells["O7"].Value = "VOIDED DATE";
                }

                // Apply styling to the header row
                string headerEndColumn = showVoidCancelColumns ? "O7" : "M7";
                using (var range = worksheet.Cells[$"A7:{headerEndColumn}"])
                {
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                // Populate the data rows
                int row = 8;
                string currencyFormatTwoDecimal = "#,##0.00";

                var totalAmount = 0m;
                var totalAmountPaid = 0m;

                foreach (var sv in serviceReport)
                {
                    worksheet.Cells[row, 1].Value = sv.CreatedDate;
                    worksheet.Cells[row, 2].Value = sv.CustomerName;
                    worksheet.Cells[row, 3].Value = sv.CustomerAddress;
                    worksheet.Cells[row, 4].Value = sv.CustomerTin;
                    worksheet.Cells[row, 5].Value = sv.ServiceInvoiceNo;
                    worksheet.Cells[row, 6].Value = sv.ServiceName;
                    worksheet.Cells[row, 7].Value = sv.Period;
                    worksheet.Cells[row, 8].Value = sv.DueDate;
                    worksheet.Cells[row, 9].Value = sv.Total;
                    worksheet.Cells[row, 10].Value = sv.AmountPaid;
                    worksheet.Cells[row, 11].Value = sv.PaymentStatus;
                    worksheet.Cells[row, 12].Value = sv.Instructions;
                    worksheet.Cells[row, 13].Value = sv.Type;

                    // Add void/cancel data — only for All or InvalidOnly
                    if (showVoidCancelColumns)
                    {
                        worksheet.Cells[row, 14].Value = sv.VoidedBy;
                        worksheet.Cells[row, 15].Value = sv.VoidedDate;
                        if (sv.VoidedDate.HasValue)
                        {
                            worksheet.Cells[row, 15].Style.Numberformat.Format = "MMM/dd/yyyy";
                        }
                    }

                    worksheet.Cells[row, 1].Style.Numberformat.Format = "MMM/dd/yyyy";
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "MMM yyyy";
                    worksheet.Cells[row, 8].Style.Numberformat.Format = "MMM/dd/yyyy";
                    worksheet.Cells[row, 9].Style.Numberformat.Format = currencyFormatTwoDecimal;
                    worksheet.Cells[row, 10].Style.Numberformat.Format = currencyFormatTwoDecimal;


                    totalAmount += sv.Total;
                    totalAmountPaid += sv.AmountPaid;
                    row++;
                }

                worksheet.Cells[row, 8].Value = "Total ";
                worksheet.Cells[row, 9].Value = totalAmount;
                worksheet.Cells[row, 10].Value = totalAmountPaid;

                worksheet.Cells[row, 9].Style.Numberformat.Format = currencyFormatTwoDecimal;
                worksheet.Cells[row, 10].Style.Numberformat.Format = currencyFormatTwoDecimal;

                // Apply style to subtotal row
                int lastColumn = showVoidCancelColumns ? 15 : 13;
                using (var range = worksheet.Cells[row, 1, row, lastColumn])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(172, 185, 202));
                }

                using (var range = worksheet.Cells[row, 8, row, lastColumn])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Double;
                }

                // Auto-fit columns for better readability
                worksheet.Cells.AutoFitColumns();
                worksheet.View.FreezePanes(8, 3);

                #region -- Audit Trail --

                AuditTrail auditTrailBook = new(GetUserFullName(), "Generate service invoice report excel file",
                    "Accounts Receivable Report");
                await _unitOfWork.AuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion

                var fileName =
                    $"Service_Invoice_Report_{DateTimeHelper.GetCurrentPhilippineTime():yyyyddMMHHmmss}.xlsx";
                var stream = new MemoryStream();
                await package.SaveAsAsync(stream, cancellationToken);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex,
                    "Failed to generate dispatch report excel file. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(ServiceInvoiceReport));
            }
        }

        #endregion
    }
}
