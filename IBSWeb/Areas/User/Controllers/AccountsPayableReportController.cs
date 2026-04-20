using System.Security.Claims;
using IBS.DataAccess.Data;
using IBS.DataAccess.Repository.IRepository;
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
    public class AccountsPayableReportController : Controller
    {
        private readonly ApplicationDbContext _dbContext;

        private readonly UserManager<ApplicationUser> _userManager;

        private readonly IUnitOfWork _unitOfWork;

        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly ILogger<AccountsPayableReportController> _logger;

        public AccountsPayableReportController(
            ApplicationDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            IWebHostEnvironment webHostEnvironment,
            ILogger<AccountsPayableReportController> logger)
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

        private async Task<string?> GetCompanyClaimAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return null;
            }

            var claims = await _userManager.GetClaimsAsync(user);
            return claims.FirstOrDefault(c => c.Type == "Company")?.Value;
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
        public IActionResult ClearedDisbursementReport()
        {
            return View();
        }

        [HttpGet]
        public IActionResult NonTradeInvoiceReport()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CvDisbursementReport()
        {
            return View();
        }

        #region -- Generated Cleared Disbursement Report as Quest PDF

        [HttpPost]
        public async Task<IActionResult> GeneratedClearedDisbursementReport(ViewModelBook model, CancellationToken cancellationToken)
        {
            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The submitted information is invalid.";
                return RedirectToAction(nameof(ClearedDisbursementReport));
            }

            try
            {
                var checkVoucherHeader = await _unitOfWork.FilprideReport.GetClearedDisbursementReport(model.DateFrom, model.DateTo, companyClaims, cancellationToken);

                if (checkVoucherHeader.Count == 0)
                {
                    TempData["info"] = "No records found!";
                    return RedirectToAction(nameof(ClearedDisbursementReport));
                }

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        #region -- Page Setup

                        page.Size(PageSizes.Legal.Landscape());
                        page.Margin(20);
                        page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Times New Roman"));

                        #endregion -- Page Setup

                        #region -- Header

                        var imgFilprideLogoPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "Filpride-logo.png");

                        page.Header().Height(50).Row(row =>
                        {
                            row.RelativeItem().Column(column =>
                            {
                                column.Item()
                                    .Text("CLEARED DISBURSEMENT REPORT")
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

                        #endregion -- Header

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
                            });

                            #endregion -- Columns Definition

                            #region -- Table Header

                            table.Header(header =>
                            {
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Category").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Subcategory").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Payee").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Date").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Voucher#").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Bank Name").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Check").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Particulars").SemiBold();
                                header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Total").SemiBold();
                            });

                            #endregion -- Table Header

                            #region -- Loop to Show Records

                            var totalAmt = 0m;
                            foreach (var record in checkVoucherHeader)
                            {
                                table.Cell().Border(0.5f).Padding(3).Text("Empty");
                                table.Cell().Border(0.5f).Padding(3).Text("Empty");
                                table.Cell().Border(0.5f).Padding(3).Text(record.Payee);
                                table.Cell().Border(0.5f).Padding(3).Text(record.Date.ToString(SD.Date_Format));
                                table.Cell().Border(0.5f).Padding(3).Text(record.CheckVoucherHeaderNo);
                                table.Cell().Border(0.5f).Padding(3).Text(record.BankAccountName);
                                table.Cell().Border(0.5f).Padding(3).Text(record.CheckNo);
                                table.Cell().Border(0.5f).Padding(3).Text(record.Particulars);
                                table.Cell().Border(0.5f).Padding(3).AlignRight().Text(record.Total != 0 ? record.Total < 0 ? $"({Math.Abs(record.Total).ToString(SD.Two_Decimal_Format)})" : record.Total.ToString(SD.Two_Decimal_Format) : null).FontColor(record.Total < 0 ? Colors.Red.Medium : Colors.Black);
                                totalAmt += record.Total;
                            }

                            #endregion -- Loop to Show Records

                            #region Create Table Cell for Totals

                            table.Cell().ColumnSpan(8).Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text("TOTAL:").SemiBold();
                            table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalAmt != 0 ? totalAmt < 0 ? $"({Math.Abs(totalAmt).ToString(SD.Two_Decimal_Format)})" : totalAmt.ToString(SD.Two_Decimal_Format) : null).SemiBold().FontColor(totalAmt < 0 ? Colors.Red.Medium : Colors.Black);

                            #endregion Create Table Cell for Totals
                        });

                        #endregion -- Content

                        #region -- Footer

                        page.Footer().AlignRight().Text(x =>
                        {
                            x.Span("Page ");
                            x.CurrentPageNumber();
                            x.Span(" of ");
                            x.TotalPages();
                        });

                        #endregion -- Footer
                    });
                });

                #region -- Audit Trail --

                AuditTrail auditTrailBook = new(GetUserFullName(), "Generate cleared disbursement report quest pdf", "Accounts Payable Report", companyClaims);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion -- Audit Trail --

                var pdfBytes = document.GeneratePdf();
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to generate cleared disbursement report quest pdf. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(ClearedDisbursementReport));
            }
        }

        #endregion -- Generated Cleared Disbursement Report as Quest PDF

        #region -- Generate Cleared Disbursement Report as Excel File --

        public async Task<IActionResult> GenerateClearedDisbursementReportExcelFile(ViewModelBook model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["warning"] = "Please input date range";
                return RedirectToAction(nameof(ClearedDisbursementReport));
            }

            try
            {
                var dateFrom = model.DateFrom;
                var dateTo = model.DateTo;
                var extractedBy = GetUserFullName();
                var companyClaims = await GetCompanyClaimAsync();
                if (companyClaims == null)
                {
                    return BadRequest();
                }

                var clearedDisbursementReport =
                    await _unitOfWork.FilprideReport.GetClearedDisbursementReport(model.DateFrom, model.DateTo,
                        companyClaims, cancellationToken);

                if (clearedDisbursementReport.Count == 0)
                {
                    TempData["info"] = "No Record Found";
                    return RedirectToAction(nameof(clearedDisbursementReport));
                }

                // Create the Excel package
                using var package = new ExcelPackage();
                // Add a new worksheet to the Excel package
                var worksheet = package.Workbook.Worksheets.Add("ClearedDisbursementReport");

                // Set the column headers
                var mergedCells = worksheet.Cells["A1:C1"];
                mergedCells.Merge = true;
                mergedCells.Value = "CLEARED DISBURSEMENT REPORT";
                mergedCells.Style.Font.Size = 13;

                worksheet.Cells["A2"].Value = "Date Range:";
                worksheet.Cells["A3"].Value = "Extracted By:";
                worksheet.Cells["A4"].Value = "Company:";

                worksheet.Cells["B2"].Value = $"{dateFrom} - {dateTo}";
                worksheet.Cells["B3"].Value = $"{extractedBy}";
                worksheet.Cells["B4"].Value = $"{companyClaims}";

                worksheet.Cells["A7"].Value = "Category";
                worksheet.Cells["B7"].Value = "Subcategory";
                worksheet.Cells["C7"].Value = "Payee";
                worksheet.Cells["D7"].Value = "Date";
                worksheet.Cells["E7"].Value = "Voucher #";
                worksheet.Cells["F7"].Value = "Bank Name";
                worksheet.Cells["G7"].Value = "Check #";
                worksheet.Cells["H7"].Value = "Particulars";
                worksheet.Cells["I7"].Value = "Amount";

                // Apply styling to the header row
                using (var range = worksheet.Cells["A7:I7"])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                // Populate the data rows
                var row = 8;
                var currencyFormat = "#,##0.00";

                var coaLookup = await _dbContext.ChartOfAccounts
                    .AsNoTracking()
                    .Include(coa => coa.ParentAccount)
                        .ThenInclude(a => a!.ParentAccount)
                        .ThenInclude(a => a!.ParentAccount)
                    .ToDictionaryAsync(c => c.AccountNumber!, cancellationToken);

                foreach (var cd in clearedDisbursementReport)
                {
                    var invoiceDebit = cd.Details!
                        .Where(d => !d.IsDisplayEntry)
                        .OrderByDescending(d => d.Debit)
                        .FirstOrDefault();

                    if (invoiceDebit == null)
                    {
                        return BadRequest();
                    }

                    if (!coaLookup.TryGetValue(invoiceDebit.AccountNo, out var coa))
                    {
                        continue;
                    }

                    var levelOneAccount = coa?.ParentAccount?.ParentAccount?.ParentAccount;

                    worksheet.Cells[row, 1].Value = $"{levelOneAccount?.AccountNumber} " +
                                                    $"{levelOneAccount?.AccountName}";
                    worksheet.Cells[row, 2].Value = $"{invoiceDebit.AccountNo} {invoiceDebit.AccountName}";
                    worksheet.Cells[row, 3].Value = cd.Payee;
                    worksheet.Cells[row, 4].Value = cd.Date;
                    worksheet.Cells[row, 5].Value = cd.CheckVoucherHeaderNo;
                    worksheet.Cells[row, 6].Value = cd.BankAccountName;
                    worksheet.Cells[row, 7].Value = cd.CheckNo;
                    worksheet.Cells[row, 8].Value = cd.Particulars;
                    worksheet.Cells[row, 9].Value = cd.Total;

                    worksheet.Cells[row, 4].Style.Numberformat.Format = "MMM/dd/yyyy";
                    worksheet.Cells[row, 9].Style.Numberformat.Format = currencyFormat;

                    row++;
                }

                worksheet.Cells[row, 8].Value = "Total: ";
                worksheet.Cells[row, 9].Value = clearedDisbursementReport.Sum(cv => cv.Total);
                using (var range = worksheet.Cells[row, 1, row, 9])
                {
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thick; // Apply thick border at the top of the row
                }

                worksheet.Cells[row, 8].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheet.Cells[row, 9].Style.Numberformat.Format = currencyFormat;
                // Auto-fit columns for better readability
                worksheet.Cells.AutoFitColumns();
                worksheet.View.FreezePanes(8, 1);

                #region -- Audit Trail --

                AuditTrail auditTrailBook = new(GetUserFullName(), "Generate cleared disbursement report excel file", "Accounts Payable Report", companyClaims);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion -- Audit Trail --

                var fileName = $"Cleared_Disbursement_Report_{DateTimeHelper.GetCurrentPhilippineTime():yyyyddMMHHmmss}.xlsx";
                var stream = new MemoryStream();
                await package.SaveAsAsync(stream, cancellationToken);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to generate cleared disbursement report excel file. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(ClearedDisbursementReport));
            }
        }

        #endregion -- Generate Cleared Disbursement Report as Excel File --

        #region -- Generate NonTrade Invoice Report as Excel File --

        public async Task<IActionResult> GenerateNonTradeInvoiceReportExcelFile(ViewModelBook model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["warning"] = "Please input date range";
                return RedirectToAction(nameof(NonTradeInvoiceReport));
            }

            try
            {
                var dateFrom = model.DateFrom;
                var dateTo = model.DateTo;
                var extractedBy = GetUserFullName();
                var companyClaims = await GetCompanyClaimAsync();

                if (companyClaims == null)
                {
                    return BadRequest();
                }
                var statusFilter = NormalizeStatusFilter(model.StatusFilter);

                var nonTradeInvoiceReport =
                    await _dbContext.CheckVoucherDetails
                        .AsNoTracking()
                        .Where(cvd => cvd.CheckVoucherHeader!.Company == companyClaims
                                      && cvd.CheckVoucherHeader.CvType == nameof(CVType.Invoicing)
                                      && cvd.CheckVoucherHeader.Date >= dateFrom &&
                                      cvd.CheckVoucherHeader.Date <= dateTo
                                      && (statusFilter == "ValidOnly"
                                          ? cvd.CheckVoucherHeader.VoidedBy == null
                                          : statusFilter == "InvalidOnly"
                                              ? cvd.CheckVoucherHeader.VoidedBy != null
                                              : true))
                        .Include(cvd => cvd.CheckVoucherHeader)
                        .ThenInclude(cvh => cvh!.Supplier)
                        .OrderBy(cvd => cvd.CheckVoucherHeader!.Date)
                        .ThenBy(cvd => cvd.CheckVoucherHeader!.CheckVoucherHeaderNo)
                        .ThenByDescending(cvd => cvd.Debit)
                        .ToListAsync(cancellationToken);

                var nonTradeNos = nonTradeInvoiceReport
                    .Select(x => x.TransactionNo)
                    .ToList();

                var payments = await _dbContext.CheckVoucherHeaders
                    .AsNoTracking()
                    .Where(x => x.Reference != null && nonTradeNos.Contains(x.Reference) && x.Company == companyClaims)
                    .Select(x => new
                    {
                        x.Reference,
                        x.CheckVoucherHeaderNo,
                        x.DcrDate
                    })
                    .ToListAsync(cancellationToken);

                var paymentDict = payments
                    .GroupBy(x => x.Reference)
                    .ToDictionary(g => g.Key!, g => g.First());

                if (nonTradeInvoiceReport.Count == 0)
                {
                    TempData["info"] = "No Record Found";
                    return RedirectToAction(nameof(NonTradeInvoiceReport));
                }

                // Create the Excel package
                using var package = new ExcelPackage();
                // Add a new worksheet to the Excel package
                var worksheet = package.Workbook.Worksheets.Add("APNonTradeInvoiceReport");

                var mergedCells = worksheet.Cells["A1:C1"];
                mergedCells.Merge = true;
                mergedCells.Value = "AP NON-TRADE INVOICE REPORT";
                mergedCells.Style.Font.Size = 13;

                worksheet.Cells["A2"].Value = "Date Range:";
                worksheet.Cells["A3"].Value = "Extracted By:";
                worksheet.Cells["A4"].Value = "Company:";
                worksheet.Cells["A5"].Value = "Status Filter:";

                worksheet.Cells["B2"].Value =
                    $"{dateFrom.ToString("MMM dd, yyyy")} - {dateTo.ToString("MMM dd, yyyy")}";
                worksheet.Cells["B3"].Value = $"{extractedBy}";
                worksheet.Cells["B4"].Value = $"{companyClaims}";
                worksheet.Cells["B5"].Value = GetStatusFilterLabel(statusFilter);

                // Determine if we need to show void/cancel columns
                bool showVoidCancelColumns = statusFilter != "ValidOnly";

                var row = 6;
                var col = 1;

                worksheet.Cells[row, col].Value = "DATE"; col++;
                worksheet.Cells[row, col].Value = "INV NO."; col++;
                worksheet.Cells[row, col].Value = "CV PAYMENT"; col++;
                worksheet.Cells[row, col].Value = "DCR"; col++;
                worksheet.Cells[row, col].Value = "PAYEE"; col++;
                worksheet.Cells[row, col].Value = "PARTICULARS"; col++;
                worksheet.Cells[row, col].Value = "DOCUMENT TYPE"; col++;
                worksheet.Cells[row, col].Value = "ACCOUNT NUMBER"; col++;
                worksheet.Cells[row, col].Value = "ACCOUNT NAME"; col++;
                worksheet.Cells[row, col].Value = "SUB ACCOUNT NAME"; col++;
                worksheet.Cells[row, col].Value = "DEBIT"; col++;
                worksheet.Cells[row, col].Value = "CREDIT"; col++;
                worksheet.Cells[row, col].Value = "STATUS"; col++;

                if (showVoidCancelColumns)
                {
                    worksheet.Cells[row, col].Value = "VOIDED BY"; col++;
                    worksheet.Cells[row, col].Value = "VOIDED DATE";
                    worksheet.Cells[row, col].Style.Numberformat.Format = "MMM/dd/yyyy";
                }

                using (var range = worksheet.Cells[row, 1, row, col])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                row++;
                var currencyFormat = "#,##0.00";
                var totalCredit = 0m;
                var totalDebit = 0m;

                foreach (var inv in nonTradeInvoiceReport)
                {
                    var paymentInfo = paymentDict.GetValueOrDefault(inv.TransactionNo);
                    col = 1;
                    worksheet.Cells[row, col].Value = inv.CheckVoucherHeader!.Date.ToDateTime(TimeOnly.MinValue);
                    worksheet.Cells[row, col].Style.Numberformat.Format = "MMM/dd/yyyy"; col++;
                    worksheet.Cells[row, col].Value = inv.CheckVoucherHeader.CheckVoucherHeaderNo; col++;
                    worksheet.Cells[row, col].Value = paymentInfo?.CheckVoucherHeaderNo ?? ""; col++;
                    if (paymentInfo?.DcrDate.HasValue == true)
                    {
                        worksheet.Cells[row, col].Value = paymentInfo.DcrDate.Value.ToDateTime(TimeOnly.MinValue);
                        worksheet.Cells[row, col].Style.Numberformat.Format = "MMM/dd/yyyy";
                    }
                    col++;
                    worksheet.Cells[row, col].Value = inv.CheckVoucherHeader.Payee; col++;
                    worksheet.Cells[row, col].Value = inv.CheckVoucherHeader.Particulars;
                    worksheet.Cells[row, col].Style.WrapText = true; col++;
                    worksheet.Cells[row, col].Value = inv.CheckVoucherHeader.Type; col++;
                    worksheet.Cells[row, col].Value = inv.AccountNo; col++;
                    worksheet.Cells[row, col].Value = inv.AccountName; col++;
                    worksheet.Cells[row, col].Value = inv.SubAccountName; col++;
                    worksheet.Cells[row, col].Value = inv.Debit;
                    worksheet.Cells[row, col].Style.Numberformat.Format = currencyFormat; col++;
                    worksheet.Cells[row, col].Value = inv.Credit;
                    worksheet.Cells[row, col].Style.Numberformat.Format = currencyFormat; col++;
                    worksheet.Cells[row, col].Value = inv.CheckVoucherHeader.Status; col++;

                    if (showVoidCancelColumns)
                    {
                        worksheet.Cells[row, col].Value = inv.CheckVoucherHeader.VoidedBy; col++;
                        worksheet.Cells[row, col].Value = inv.CheckVoucherHeader.VoidedDate;
                        worksheet.Cells[row, col].Style.Numberformat.Format = "MMM/dd/yyyy";
                    }

                    totalCredit += inv.Credit;
                    totalDebit += inv.Debit;

                    row++;
                }

                int totalRow = row;
                int lastDataCol = showVoidCancelColumns ? 15 : 13;
                worksheet.Cells[totalRow, 10].Value = "TOTAL: ";
                worksheet.Cells[totalRow, 11].Value = totalDebit;
                worksheet.Cells[totalRow, 12].Value = totalCredit;

                using (var range = worksheet.Cells[totalRow, 1, totalRow, lastDataCol])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Double;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }
                using (var range = worksheet.Cells[totalRow, 8, totalRow, lastDataCol])
                {
                    range.Style.Numberformat.Format = currencyFormat;
                }

                worksheet.Cells.AutoFitColumns();
                worksheet.View.FreezePanes(7, 1);

                #region -- Audit Trail --

                AuditTrail auditTrailBook = new(GetUserFullName(), "Generate Non-Trade Invoice report excel file", "Accounts Payable Report", companyClaims);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion -- Audit Trail --

                var fileName = $"NonTrade_Invoice_Report_{DateTimeHelper.GetCurrentPhilippineTime():yyyyddMMHHmmss}.xlsx";
                var stream = new MemoryStream();
                await package.SaveAsAsync(stream, cancellationToken);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to generate non trade invoice report excel file. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(NonTradeInvoiceReport));
            }
        }

        #endregion -- Generate NonTrade Invoice Report as Excel File --

        #region -- Generate CV Disbursement Report as Excel File --

        public async Task<IActionResult> GenerateCvDisbursementReportExcelFile(ViewModelBook model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["warning"] = "Please input date range";
                return RedirectToAction(nameof(CvDisbursementReport));
            }

            try
            {
                var dateFrom = model.DateFrom;
                var dateTo = model.DateTo;
                var extractedBy = GetUserFullName();
                var companyClaims = await GetCompanyClaimAsync();

                if (companyClaims == null)
                {
                    return BadRequest();
                }

                var statusFilter = NormalizeStatusFilter(model.StatusFilter);

                var cvTradeHeaderReport = await _dbContext.CheckVoucherHeaders
                        .AsNoTracking()
                        .Where(cvh =>
                            cvh.Company == companyClaims &&
                            cvh.CvType != nameof(CVType.Invoicing) &&
                            cvh.Date >= dateFrom &&
                            cvh.Date <= dateTo
                            && (statusFilter == "ValidOnly"
                                ? cvh.VoidedBy == null
                                : statusFilter == "InvalidOnly"
                                    ? cvh.VoidedBy != null
                                    : true))
                        .Include(cvh => cvh.Details!)
                        .Include(cvh => cvh.Supplier)
                        .OrderBy(cvh => cvh.Date)
                        .ThenBy(cvh => cvh.CheckVoucherHeaderNo)
                        .ToListAsync(cancellationToken);

                var cvTradeHeaderIds = cvTradeHeaderReport.Select(cvh => cvh.CheckVoucherHeaderId).ToList();
                var cvTradePayments = await _dbContext.CVTradePayments.Where(cvp => cvTradeHeaderIds.Contains(cvp.CheckVoucherId)).ToListAsync(cancellationToken);

                var supplierIds = cvTradeHeaderReport.Where(cvh => cvh.Category == "Trade" && cvh.CvType == "Supplier").Select(cvh => cvh.CheckVoucherHeaderId).ToList();
                var receivingReportIds = cvTradePayments.Where(cvp => supplierIds.Contains(cvp.CheckVoucherId)).Select(cvp => cvp.DocumentId).ToList();
                var receivingReports = await _unitOfWork.FilprideReceivingReport.GetAllAsync(dr => receivingReportIds.Contains(dr.ReceivingReportId), cancellationToken);

                var notSupplierIds = cvTradeHeaderReport.Where(cvh => cvh.Category == "Trade" && cvh.CvType != "Supplier").Select(cvh => cvh.CheckVoucherHeaderId).ToList();
                var deliveryReceiptIds = cvTradePayments.Where(cvp => notSupplierIds.Contains(cvp.CheckVoucherId)).Select(cvp => cvp.DocumentId).ToList();
                var deliveryReceipts = await _unitOfWork.FilprideDeliveryReceipt.GetAllAsync(dr => deliveryReceiptIds.Contains(dr.DeliveryReceiptId), cancellationToken);

                if (cvTradeHeaderReport.Count == 0)
                {
                    TempData["info"] = "No Record Found";
                    return RedirectToAction(nameof(CvDisbursementReport));
                }

                // Create the Excel package
                using var package = new ExcelPackage();
                // Add a new worksheet to the Excel package
                var worksheet = package.Workbook.Worksheets.Add("CvDisbursementReport");

                var mergedCells = worksheet.Cells["A1:C1"];
                mergedCells.Merge = true;
                mergedCells.Value = "CV DISBURSEMENT REPORT";
                mergedCells.Style.Font.Size = 13;

                worksheet.Cells["A2"].Value = "Date Range:";
                worksheet.Cells["A3"].Value = "Extracted By:";
                worksheet.Cells["A4"].Value = "Company:";
                worksheet.Cells["A5"].Value = "Status Filter:";

                worksheet.Cells["B2"].Value = $"{dateFrom.ToString("MMM dd, yyyy")} - {dateTo.ToString("MMM dd, yyyy")}";
                worksheet.Cells["B3"].Value = $"{extractedBy}";
                worksheet.Cells["B4"].Value = $"{companyClaims}";
                worksheet.Cells["B5"].Value = GetStatusFilterLabel(statusFilter);

                // Determine if we need to show void/cancel columns
                bool showVoidCancelColumns = statusFilter != "ValidOnly";

                int row = 6;
                int col = 1;

                worksheet.Cells[row, col].Value = "DATE"; col++;
                worksheet.Cells[row, col].Value = "CV No."; col++;
                worksheet.Cells[row, col].Value = "DCR DATE"; col++;
                worksheet.Cells[row, col].Value = "CHECK #"; col++;
                worksheet.Cells[row, col].Value = "PAYEE"; col++;
                worksheet.Cells[row, col].Value = "PARTICULARS"; col++;
                worksheet.Cells[row, col].Value = "DOCUMENT TYPE"; col++;
                worksheet.Cells[row, col].Value = "INVOICE No."; col++;
                worksheet.Cells[row, col].Value = "ACCOUNT NUMBER"; col++;
                worksheet.Cells[row, col].Value = "ACCOUNT NAME"; col++;
                worksheet.Cells[row, col].Value = "SUB ACCOUNT NAME"; col++;
                worksheet.Cells[row, col].Value = "DEBIT"; col++;
                worksheet.Cells[row, col].Value = "CREDIT"; col++;
                worksheet.Cells[row, col].Value = "STATUS"; col++;

                if (showVoidCancelColumns)
                {
                    worksheet.Cells[row, col].Value = "VOIDED BY"; col++;
                    worksheet.Cells[row, col].Value = "VOIDED DATE";
                }

                using (var range = worksheet.Cells[row, 1, row, col])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                row++;
                var currencyFormat = "#,##0.00";
                var totalCredit = 0m;
                var totalDebit = 0m;

                foreach (var header in cvTradeHeaderReport)
                {
                    foreach (var details in header.Details!
                                 .Where(x => !x.IsDisplayEntry)
                                 .OrderByDescending(d => d.Debit))
                    {
                        col = 1;
                        worksheet.Cells[row, col].Value = header.Date.ToDateTime(TimeOnly.MinValue); col++;
                        worksheet.Cells[row, col].Value = header.CheckVoucherHeaderNo; col++;
                        worksheet.Cells[row, col].Value = header.DcrDate?.ToDateTime(TimeOnly.MinValue); col++;
                        worksheet.Cells[row, col].Value = header.CheckNo; col++;
                        worksheet.Cells[row, col].Value = header.Payee; col++;
                        worksheet.Cells[row, col].Value = header.Particulars;
                        worksheet.Cells[row, col].Style.WrapText = true;
                        col++;
                        worksheet.Cells[row, col].Value = header.Type == nameof(DocumentType.Documented) ? "Doc" : "Undoc"; col++;

                        if (header.Category == "Trade")
                        {
                            var rrListOfString = new List<string>();

                            if (header.CvType == "Supplier")
                            {
                                var cvTradeRrs = cvTradePayments
                                    .Where(ctp => ctp.CheckVoucherId == header.CheckVoucherHeaderId)
                                    .ToList();

                                if (cvTradeRrs.Count > 0)
                                {
                                    foreach (var cvTradeRr in cvTradeRrs)
                                    {
                                        var rr = receivingReports.FirstOrDefault(r => r.ReceivingReportId == cvTradeRr.DocumentId);
                                        if (rr != null)
                                        {
                                            rrListOfString.Add(rr.ReceivingReportNo!);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                var cvTradeDrs = cvTradePayments
                                    .Where(ctp => ctp.CheckVoucherId == header.CheckVoucherHeaderId)
                                    .ToList();

                                if (cvTradeDrs.Count > 0)
                                {
                                    foreach (var cvTradeRr in cvTradeDrs)
                                    {
                                        var rr = deliveryReceipts.FirstOrDefault(r => r.DeliveryReceiptId == cvTradeRr.DocumentId);
                                        if (rr != null)
                                        {
                                            rrListOfString.Add(rr.DeliveryReceiptNo);
                                        }
                                    }
                                }
                            }

                            if (rrListOfString.Count > 0)
                            {
                                worksheet.Cells[row, col].Value = $"{string.Join(", ", rrListOfString)}";
                            }
                        }
                        else
                        {
                            worksheet.Cells[row, col].Value = header.Reference;
                        }
                        worksheet.Cells[row, col].Style.WrapText = true;
                        col++;

                        worksheet.Cells[row, col].Value = details.AccountNo; col++;
                        worksheet.Cells[row, col].Value = details.AccountName; col++;
                        worksheet.Cells[row, col].Value = details.SubAccountName; col++;
                        worksheet.Cells[row, col].Value = details.Debit; col++;
                        worksheet.Cells[row, col].Value = details.Credit; col++;
                        worksheet.Cells[row, col].Value = header.Status; col++;


                        if (showVoidCancelColumns)
                        {
                            worksheet.Cells[row, col].Value = header.VoidedBy; col++;
                            worksheet.Cells[row, col].Value = header.VoidedDate;
                            worksheet.Cells[row, col].Style.Numberformat.Format = "MMM/dd/yyyy";
                        }

                        worksheet.Cells[row, 1].Style.Numberformat.Format = "MMM/dd/yyyy";
                        worksheet.Cells[row, 12].Style.Numberformat.Format = currencyFormat;
                        worksheet.Cells[row, 13].Style.Numberformat.Format = currencyFormat;

                        totalCredit += details.Credit;
                        totalDebit += details.Debit;

                        row++;
                    }
                }

                int totalRow = row;
                int lastDataCol = showVoidCancelColumns ? 16 : 14;
                worksheet.Cells[totalRow, 11].Value = "TOTAL: ";
                worksheet.Cells[totalRow, 12].Value = totalDebit;
                worksheet.Cells[totalRow, 13].Value = totalCredit;

                using (var range = worksheet.Cells[totalRow, 1, totalRow, lastDataCol])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Double;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }
                using (var range = worksheet.Cells[totalRow, 11, totalRow, lastDataCol])
                {
                    range.Style.Numberformat.Format = currencyFormat;
                }

                worksheet.Cells.AutoFitColumns();

                worksheet.View.FreezePanes(7, 1);

                #region -- Audit Trail --

                AuditTrail auditTrailBook = new(GetUserFullName(), "Generate Cv Disbursement report excel file", "Accounts Payable Report", companyClaims);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion -- Audit Trail --

                var fileName = $"CV_Disbursement_Report_{DateTimeHelper.GetCurrentPhilippineTime():yyyyddMMHHmmss}.xlsx";
                var stream = new MemoryStream();
                await package.SaveAsAsync(stream, cancellationToken);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to generate cleared disbursement report excel file. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(CvDisbursementReport));
            }
        }

        #endregion -- Generate CV Disbursement Report as Excel File --

        [HttpGet]
        public IActionResult JournalVoucherReport()
        {
            return View();
        }

        #region -- Generated Journal Voucher Report as Excel File --

        [HttpPost]
        public async Task<IActionResult> GenerateJournalVoucherExcelFile(ViewModelBook model, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
            {
                TempData["warning"] = "Please input date range";
                return RedirectToAction(nameof(JournalVoucherReport));
            }

            try
            {
                var dateFrom = model.DateFrom;
                var dateTo = model.DateTo;
                var extractedBy = GetUserFullName();
                var companyClaims = await GetCompanyClaimAsync();

                if (companyClaims == null)
                {
                    return BadRequest();
                }

                var statusFilter = NormalizeStatusFilter(model.StatusFilter);

                // Fetch journal voucher report data
                var journalVoucherReport = await _unitOfWork.FilprideReport
                    .GetJournalVoucherReport(model.DateFrom, model.DateTo, companyClaims, statusFilter, cancellationToken);

                if (journalVoucherReport.Count == 0)
                {
                    TempData["info"] = "No Record Found";
                    return RedirectToAction(nameof(JournalVoucherReport));
                }

                // Create the Excel package
                using var package = new ExcelPackage();
                var worksheet = package.Workbook.Worksheets.Add("JournalVoucherReport");

                // Set report title
                var reportTitle = worksheet.Cells["A1:B1"];
                reportTitle.Merge = true;
                reportTitle.Value = "JOURNAL VOUCHER REPORT";
                reportTitle.Style.Font.Size = 13;

                // Set filter information
                worksheet.Cells["A2"].Value = "Date Range: ";
                worksheet.Cells["B2"].Value = $"{model.DateFrom} - {model.DateTo}";
                worksheet.Cells["A3"].Value = "Extracted By: ";
                worksheet.Cells["B3"].Value = GetUserFullName();
                worksheet.Cells["A4"].Value = "Company: ";
                worksheet.Cells["B4"].Value = await GetCompanyClaimAsync();
                worksheet.Cells["A5"].Value = "Status Filter: ";
                worksheet.Cells["B5"].Value = GetStatusFilterLabel(statusFilter);

                // Determine if we need to show void/cancel columns
                bool showVoidCancelColumns = statusFilter != "ValidOnly";

                // Set column headers (Row 7)
                int headerRow = 7;

                worksheet.Cells[headerRow, 1].Value = "DATE";
                worksheet.Cells[headerRow, 2].Value = "JV #";
                worksheet.Cells[headerRow, 3].Value = "PARTICULARS";
                worksheet.Cells[headerRow, 4].Value = "DEBIT";
                worksheet.Cells[headerRow, 5].Value = "CREDIT";
                worksheet.Cells[headerRow, 6].Value = "ACCOUNT NUMBER";
                worksheet.Cells[headerRow, 7].Value = "ACCOUNT NAME";
                worksheet.Cells[headerRow, 8].Value = "JV STATUS";
                worksheet.Cells[headerRow, 9].Value = "JV REASON";
                worksheet.Cells[headerRow, 10].Value = "CHECK NO";
                worksheet.Cells[headerRow, 11].Value = "CV #";
                worksheet.Cells[headerRow, 12].Value = "PAYEE";
                worksheet.Cells[headerRow, 13].Value = "PREPARED BY";

                int lastColIndex = 13;
                if (showVoidCancelColumns)
                {
                    worksheet.Cells[headerRow, 14].Value = "VOIDED BY";
                    worksheet.Cells[headerRow, 15].Value = "VOIDED DATE";
                    lastColIndex = 15;
                }

                // Align all cells left
                worksheet.Cells[worksheet.Dimension.Address].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;

                // Apply border to left, right of header
                using (var range = worksheet.Cells[headerRow, 1, headerRow, lastColIndex])
                {
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.LightGray);
                    range.Style.Font.Bold = true;
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    range.Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                }

                // Populate the data rows
                int row = headerRow + 1;
                string currencyFormat = "#,##0.00";

                foreach (var detail in journalVoucherReport)
                {
                    worksheet.Cells[row, 1].Value = detail.JournalVoucherHeader!.Date;
                    worksheet.Cells[row, 1].Style.Numberformat.Format = "MMM/dd/yyyy";
                    worksheet.Cells[row, 2].Value = detail.JournalVoucherHeader.JournalVoucherHeaderNo;
                    worksheet.Cells[row, 3].Value = detail.JournalVoucherHeader.Particulars;
                    worksheet.Cells[row, 3].Style.WrapText = true;
                    worksheet.Cells[row, 4].Value = detail.Debit;
                    worksheet.Cells[row, 4].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 5].Value = detail.Credit;
                    worksheet.Cells[row, 5].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 6].Value = detail.AccountNo;
                    worksheet.Cells[row, 7].Value = detail.AccountName;
                    worksheet.Cells[row, 8].Value = detail.JournalVoucherHeader.Status;
                    worksheet.Cells[row, 9].Value = detail.JournalVoucherHeader.JVReason;
                    worksheet.Cells[row, 10].Value = detail.JournalVoucherHeader.CheckVoucherHeader?.CheckNo;
                    worksheet.Cells[row, 11].Value = detail.JournalVoucherHeader.CheckVoucherHeader?.CheckVoucherHeaderNo;
                    worksheet.Cells[row, 12].Value = detail.JournalVoucherHeader.CheckVoucherHeader?.Payee;
                    worksheet.Cells[row, 13].Value = detail.JournalVoucherHeader.CreatedBy;

                    if (showVoidCancelColumns)
                    {
                        worksheet.Cells[row, 14].Value = detail.JournalVoucherHeader.VoidedBy;
                        worksheet.Cells[row, 15].Value = detail.JournalVoucherHeader.VoidedDate;
                        worksheet.Cells[row, 15].Style.Numberformat.Format = "MMM/dd/yyyy";
                    }

                    row++;
                }

                // Append the total of credit and debit
                worksheet.Cells[row, 3].Value = "TOTAL:";
                worksheet.Cells[row, 4].Value = journalVoucherReport.Sum(jv => jv.Debit);
                worksheet.Cells[row, 4].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 5].Value = journalVoucherReport.Sum(jv => jv.Credit);
                worksheet.Cells[row, 5].Style.Numberformat.Format = currencyFormat;

                // Apply the specified styling to the total row
                using (var range = worksheet.Cells[row, 1, row, lastColIndex])
                {
                    range.Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(172, 185, 202));
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Double;
                }

                // Auto-fit columns for better readability
                worksheet.Cells.AutoFitColumns();
                worksheet.Column(3).Width = 60;
                worksheet.Column(9).Width = 30;

                // Freeze panes at particulars and
                worksheet.View.FreezePanes(headerRow + 1, 3);

                #region -- Audit Trail --

                AuditTrail auditTrailBook = new(
                    GetUserFullName(),
                    "Generate journal voucher report excel file",
                    "Journal Voucher Report",
                    companyClaims
                );
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion -- Audit Trail --

                var fileName = $"JournalVoucher_Report_{DateTimeHelper.GetCurrentPhilippineTime():yyyyMMddHHmmss}.xlsx";
                var stream = new MemoryStream();
                await package.SaveAsAsync(stream, cancellationToken);
                stream.Position = 0;

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to generate journal voucher report excel file. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(JournalVoucherReport));
            }
        }

        #endregion -- Generated Journal Voucher Report as Excel File --
    }
}
