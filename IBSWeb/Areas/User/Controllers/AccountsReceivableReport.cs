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
    public class AccountsReceivableReport : Controller
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
        public IActionResult PostedCollection()
        {
            return View();
        }

        #region -- Generated Posted Collection Report as Quest PDF

        public async Task<IActionResult> GeneratePostedCollection(ViewModelBook model, CancellationToken cancellationToken)
        {
            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The submitted information is invalid.";
                return RedirectToAction(nameof(PostedCollection));
            }

            try
            {
                var collectionReceiptReport = await _unitOfWork.FilprideReport
                    .GetCollectionReceiptReport(model.DateFrom, model.DateTo, companyClaims);

                if (!collectionReceiptReport.Any())
                {
                    TempData["info"] = "No records found";
                    return RedirectToAction(nameof(PostedCollection));
                }

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        #region -- Page Setup

                            page.Size(PageSizes.Legal.Landscape());
                            page.Margin(20);
                            page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Times New Roman"));

                        #endregion

                        #region -- Header

                            var imgFilprideLogoPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "Filpride-logo.png");

                            page.Header().Height(50).Row(row =>
                            {
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item()
                                        .Text("COLLECTION")
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
                                });

                            #endregion

                            #region -- Table Header

                                table.Header(header =>
                                {
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Customer No.").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Customer Name").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Acc. Type").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Tran. Date(INV)").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("CR No.").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Invoice No.").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Due Date").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Date of Check").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Bank").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Check No.").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Amount").SemiBold();
                                });

                            #endregion

                            #region -- Loop to Show Records

                            decimal totalAmount = 0;

                                foreach (var record in collectionReceiptReport)
                                {
                                    if (record.SalesInvoiceId != null)
                                    {
                                        var currentAmount = record.CashAmount + record.CheckAmount;

                                        table.Cell().Border(0.5f).Padding(3).Text(record.Customer?.CustomerCode);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.SalesInvoice?.CustomerOrderSlip?.CustomerName ?? record.Customer?.CustomerName);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.SalesInvoice?.CustomerOrderSlip?.CustomerType ?? record.Customer?.CustomerType);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.SalesInvoice?.TransactionDate.ToString(SD.Date_Format));
                                        table.Cell().Border(0.5f).Padding(3).Text(record.CollectionReceiptNo);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.SalesInvoice?.SalesInvoiceNo);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.SalesInvoice?.DueDate.ToString(SD.Date_Format));
                                        table.Cell().Border(0.5f).Padding(3).Text(record.CheckDate?.ToString(SD.Date_Format));
                                        table.Cell().Border(0.5f).Padding(3).Text($"{record.BankAccount?.Bank} {record.BankAccountNumber}");
                                        table.Cell().Border(0.5f).Padding(3).Text(record.CheckNo);
                                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(currentAmount != 0 ? currentAmount < 0 ? $"({Math.Abs(currentAmount).ToString(SD.Two_Decimal_Format)})" : currentAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(currentAmount < 0 ? Colors.Red.Medium : Colors.Black);

                                        totalAmount += currentAmount;
                                    }
                                    if (record.ServiceInvoiceId != null)
                                    {
                                        var currentAmount = record.CashAmount + record.CheckAmount;

                                        table.Cell().Border(0.5f).Padding(3).Text(record.Customer?.CustomerCode);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.ServiceInvoice?.CustomerName);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.Customer?.CustomerType);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.ServiceInvoice?.CreatedDate.ToString(SD.Date_Format));
                                        table.Cell().Border(0.5f).Padding(3).Text(record.CollectionReceiptNo);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.ServiceInvoice?.ServiceInvoiceNo);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.ServiceInvoice?.DueDate.ToString(SD.Date_Format));
                                        table.Cell().Border(0.5f).Padding(3).Text(record.CheckDate?.ToString(SD.Date_Format));
                                        table.Cell().Border(0.5f).Padding(3).Text($"{record.BankAccount?.Bank} {record.BankAccountNumber}");
                                        table.Cell().Border(0.5f).Padding(3).Text(record.CheckNo);
                                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(currentAmount != 0 ? currentAmount < 0 ? $"({Math.Abs(currentAmount).ToString(SD.Two_Decimal_Format)})" : currentAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(currentAmount < 0 ? Colors.Red.Medium : Colors.Black);

                                        totalAmount += currentAmount;
                                    }
                                    if (record.MultipleSIId != null)
                                    {
                                        var getSalesInvoice = _unitOfWork.FilprideSalesInvoice.GetAllAsync(x => record.MultipleSIId.Contains(x.SalesInvoiceId));
                                        foreach (var sales in getSalesInvoice.Result)
                                        {
                                            var currentAmount = record.CashAmount + record.CheckAmount;

                                            table.Cell().Border(0.5f).Padding(3).Text(record.Customer?.CustomerCode);
                                            table.Cell().Border(0.5f).Padding(3).Text(sales.CustomerOrderSlip?.CustomerName ?? record.Customer?.CustomerName);
                                            table.Cell().Border(0.5f).Padding(3).Text(sales.CustomerOrderSlip?.CustomerType ?? record.Customer?.CustomerType);
                                            table.Cell().Border(0.5f).Padding(3).Text(sales.TransactionDate.ToString(SD.Date_Format));
                                            table.Cell().Border(0.5f).Padding(3).Text(record.CollectionReceiptNo);
                                            table.Cell().Border(0.5f).Padding(3).Text(sales.SalesInvoiceNo);
                                            table.Cell().Border(0.5f).Padding(3).Text(sales.DueDate.ToString(SD.Date_Format));
                                            table.Cell().Border(0.5f).Padding(3).Text(record.CheckDate?.ToString(SD.Date_Format));
                                            table.Cell().Border(0.5f).Padding(3).Text($"{record.BankAccount?.Bank} {record.BankAccountNumber}");
                                            table.Cell().Border(0.5f).Padding(3).Text(record.CheckNo);
                                            table.Cell().Border(0.5f).Padding(3).AlignRight().Text(currentAmount != 0 ? currentAmount < 0 ? $"({Math.Abs(currentAmount).ToString(SD.Two_Decimal_Format)})" : currentAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(currentAmount < 0 ? Colors.Red.Medium : Colors.Black);

                                            totalAmount += currentAmount;
                                        }
                                    }
                                }

                            #endregion

                            #region -- Create Table Cell for Totals

                                table.Cell().ColumnSpan(10).Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text("TOTAL:").SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalAmount != 0 ? totalAmount < 0 ? $"({Math.Abs(totalAmount).ToString(SD.Two_Decimal_Format)})" : totalAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(totalAmount < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();

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

                AuditTrail auditTrailBook = new(GetUserFullName(), "Generate posted collection report quest pdf", "Accounts Receivable Report", companyClaims);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion

                var pdfBytes = document.GeneratePdf();
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to generate posted collection report quest pdf. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(PostedCollection));
            }
        }

        #endregion

        #region -- Generate Posted Collection Excel File --

            public async Task<IActionResult> GeneratePostedCollectionExcelFile(ViewModelBook model, CancellationToken cancellationToken)
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
                    var companyClaims = await GetCompanyClaimAsync();
                    if (companyClaims == null)
                    {
                        return BadRequest();
                    }

                    var statusFilter = NormalizeStatusFilter(model.StatusFilter);

                    var collectionReceiptReport = await _unitOfWork.FilprideReport
                        .GetCollectionReceiptReport(model.DateFrom, model.DateTo, companyClaims, statusFilter, cancellationToken);

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
                    worksheet.Cells["B4"].Value = $"{companyClaims}";
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
                        if (cr.SalesInvoiceId != null)
                        {
                            var currentAmount = cr.CashAmount + cr.CheckAmount;
                            worksheet.Cells[row, 1].Value = cr.Customer?.CustomerCode;
                            worksheet.Cells[row, 2].Value = cr.SalesInvoice?.CustomerOrderSlip?.CustomerName ?? cr.Customer?.CustomerName;
                            worksheet.Cells[row, 3].Value = cr.SalesInvoice?.CustomerOrderSlip?.CustomerType ?? cr.Customer?.CustomerType;
                            worksheet.Cells[row, 4].Value = cr.SalesInvoice?.TransactionDate;
                            worksheet.Cells[row, 5].Value = cr.CollectionReceiptNo;
                            worksheet.Cells[row, 6].Value = cr.SalesInvoice?.SalesInvoiceNo;
                            worksheet.Cells[row, 7].Value = cr.SalesInvoice?.DueDate;
                            worksheet.Cells[row, 8].Value = cr.CheckDate;
                            worksheet.Cells[row, 9].Value = $"{cr.BankAccount?.Bank} {cr.BankAccountNumber}";
                            worksheet.Cells[row, 10].Value = cr.CheckNo;
                            worksheet.Cells[row, 11].Value = currentAmount;

                            worksheet.Cells[row, 4].Style.Numberformat.Format = "MMM/dd/yyyy";
                            worksheet.Cells[row, 7].Style.Numberformat.Format = "MMM/dd/yyyy";
                            worksheet.Cells[row, 8].Style.Numberformat.Format = "MMM/dd/yyyy";
                            worksheet.Cells[row, 11].Style.Numberformat.Format = currencyFormat;

                            if (showVoidCancelColumns)
                            {
                                worksheet.Cells[row, 12].Value = cr.VoidedBy;
                                worksheet.Cells[row, 13].Value = cr.VoidedDate;
                                if (cr.VoidedDate.HasValue)
                                {
                                    worksheet.Cells[row, 13].Style.Numberformat.Format = "MMM/dd/yyyy";
                                }
                            }

                            totalAmount += currentAmount;
                            row++;
                        }
                        if (cr.ServiceInvoiceId != null)
                        {
                            var currentAmount = cr.CashAmount + cr.CheckAmount;
                            worksheet.Cells[row, 1].Value = cr.Customer?.CustomerCode;
                            worksheet.Cells[row, 2].Value = cr.ServiceInvoice?.CustomerName;
                            worksheet.Cells[row, 3].Value = cr.Customer?.CustomerType;
                            worksheet.Cells[row, 4].Value = cr.ServiceInvoice?.CreatedDate;
                            worksheet.Cells[row, 5].Value = cr.CollectionReceiptNo;
                            worksheet.Cells[row, 6].Value = cr.ServiceInvoice?.ServiceInvoiceNo;
                            worksheet.Cells[row, 7].Value = cr.ServiceInvoice?.DueDate;
                            worksheet.Cells[row, 8].Value = cr.CheckDate;
                            worksheet.Cells[row, 9].Value = $"{cr.BankAccount?.Bank} {cr.BankAccountNumber}";
                            worksheet.Cells[row, 10].Value = cr.CheckNo;
                            worksheet.Cells[row, 11].Value = currentAmount;

                            worksheet.Cells[row, 4].Style.Numberformat.Format = "MMM/dd/yyyy";
                            worksheet.Cells[row, 7].Style.Numberformat.Format = "MMM/dd/yyyy";
                            worksheet.Cells[row, 8].Style.Numberformat.Format = "MMM/dd/yyyy";
                            worksheet.Cells[row, 11].Style.Numberformat.Format = currencyFormat;

                            if (showVoidCancelColumns)
                            {
                                worksheet.Cells[row, 12].Value = cr.VoidedBy;
                                worksheet.Cells[row, 13].Value = cr.VoidedDate;
                                if (cr.VoidedDate.HasValue)
                                {
                                    worksheet.Cells[row, 13].Style.Numberformat.Format = "MMM/dd/yyyy";
                                }
                            }

                            totalAmount += currentAmount;
                            row++;
                        }
                        if (cr.MultipleSIId != null)
                        {
                            var getSalesInvoice = await _unitOfWork.FilprideSalesInvoice
                                .GetAllAsync(x => cr.MultipleSIId.Contains(x.SalesInvoiceId), cancellationToken);
                            foreach (var sales in getSalesInvoice)
                            {
                                var currentAmount = cr.CashAmount + cr.CheckAmount;
                                worksheet.Cells[row, 1].Value = cr.Customer?.CustomerCode;
                                worksheet.Cells[row, 2].Value = sales.CustomerOrderSlip?.CustomerName ?? cr.Customer?.CustomerName;
                                worksheet.Cells[row, 3].Value = sales.CustomerOrderSlip?.CustomerType ?? cr.Customer?.CustomerType;
                                worksheet.Cells[row, 4].Value = sales.TransactionDate;
                                worksheet.Cells[row, 5].Value = cr.CollectionReceiptNo;
                                worksheet.Cells[row, 6].Value = sales.SalesInvoiceNo;
                                worksheet.Cells[row, 7].Value = sales.DueDate;
                                worksheet.Cells[row, 8].Value = cr.CheckDate;
                                worksheet.Cells[row, 9].Value = $"{cr.BankAccount?.Bank} {cr.BankAccountNumber}";
                                worksheet.Cells[row, 10].Value = cr.CheckNo;
                                worksheet.Cells[row, 11].Value = currentAmount;

                                worksheet.Cells[row, 4].Style.Numberformat.Format = "MMM/dd/yyyy";
                                worksheet.Cells[row, 7].Style.Numberformat.Format = "MMM/dd/yyyy";
                                worksheet.Cells[row, 8].Style.Numberformat.Format = "MMM/dd/yyyy";
                                worksheet.Cells[row, 11].Style.Numberformat.Format = currencyFormat;

                                if (showVoidCancelColumns)
                                {
                                    worksheet.Cells[row, 12].Value = cr.VoidedBy;
                                    worksheet.Cells[row, 13].Value = cr.VoidedDate;
                                    if (cr.VoidedDate.HasValue)
                                        worksheet.Cells[row, 13].Style.Numberformat.Format = "MMM/dd/yyyy";
                                    worksheet.Cells[row, 14].Value = cr.CanceledBy;
                                    worksheet.Cells[row, 15].Value = cr.CanceledDate;
                                    if (cr.CanceledDate.HasValue)
                                        worksheet.Cells[row, 15].Style.Numberformat.Format = "MMM/dd/yyyy";
                                }

                                totalAmount += currentAmount;
                                row++;
                            }
                        }
                    }

                    int lastColumn = showVoidCancelColumns ? 13 : 11;

                    worksheet.Cells[row, 10].Value = "Total:";
                    worksheet.Cells[row, 11].Value = totalAmount;
                    worksheet.Cells[row, 11].Style.Numberformat.Format = currencyFormat;

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

                    AuditTrail auditTrailBook = new(GetUserFullName(), "Generate posted collection report excel file", "Accounts Receivable Report", companyClaims);
                    await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

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
                    _logger.LogError(ex, "Failed to generate posted collection report excel file. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
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

        #region -- Generated Aging Report as Quest PDF

        public async Task<IActionResult> GeneratedAgingReport(ViewModelBook model, CancellationToken cancellationToken)
        {
            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The submitted information is invalid.";
                return RedirectToAction(nameof(AgingReport));
            }

            try
            {
                var salesInvoice = await _unitOfWork.FilprideSalesInvoice
                    .GetAllAsync(si => si.PostedBy != null
                                       && si.AmountPaid == 0
                                       && !si.IsPaid && si.Company == companyClaims, cancellationToken);

                if (!salesInvoice.Any())
                {
                    TempData["info"] = "No records found!";
                    return RedirectToAction(nameof(AgingReport));
                }

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        #region -- Page Setup

                            page.Size(PageSizes.Legal.Landscape());
                            page.Margin(20);
                            page.DefaultTextStyle(x => x.FontSize(7).FontFamily("Times New Roman"));

                        #endregion

                        #region -- Header

                            var imgFilprideLogoPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "Filpride-logo.png");

                            page.Header().Height(50).Row(row =>
                            {
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item()
                                        .Text("AGING REPORT")
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
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Month").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Customer Name").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Acc. Type").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Terms").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("EWT%").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Sales Date").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Due Date").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Invoice No.").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("DR#").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Gross").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Partial Collections").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Adjusted Gross").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("EWT").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Net of VAT").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("VCF").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Retention Amount").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Adjusted Net").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Days Due").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Current").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("1-30 Days").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("31-60 Days").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("61-90 Days").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Over 90 Days").SemiBold();
                                });

                            #endregion

                            #region -- Loop to Show Records

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

                                var repoCalculator = _unitOfWork.FilprideSalesInvoice;

                                foreach (var record in salesInvoice)
                                {

                                    var gross = record.Amount;
                                    var netDiscount = record.Amount - record.Discount;
                                    var netOfVatAmount = record.CustomerOrderSlip?.VatType == SD.VatType_Vatable
                                        ? repoCalculator.ComputeNetOfVat(netDiscount)
                                        : netDiscount;
                                    var withHoldingTaxAmount = record.CustomerOrderSlip?.HasEWT ?? true
                                        ? repoCalculator.ComputeEwtAmount(netDiscount, 0.01m)
                                        : 0;
                                    var retentionAmount = (record.Customer?.RetentionRate ?? 0.0000m) * netOfVatAmount;
                                    var vcfAmount = 0.0000m;
                                    var adjustedGross = gross - vcfAmount;
                                    var adjustedNet = gross - vcfAmount - retentionAmount;

                                    var today = DateOnly.FromDateTime(DateTimeHelper.GetCurrentPhilippineTime());
                                    var daysDue = today > record.DueDate ? today.DayNumber - record.DueDate.DayNumber : 0;
                                    var current = (record.DueDate >= today) ? gross : 0.0000m;
                                    var oneToThirtyDays = (daysDue >= 1 && daysDue <= 30) ? gross : 0.0000m;
                                    var thirtyOneToSixtyDays = (daysDue >= 31 && daysDue <= 60) ? gross : 0.0000m;
                                    var sixtyOneToNinetyDays = (daysDue >= 61 && daysDue <= 90) ? gross : 0.0000m;
                                    var overNinetyDays = (daysDue > 90) ? gross : 0.0000m;

                                    table.Cell().Border(0.5f).Padding(3).Text(record.TransactionDate.ToString("MMM yyyy"));
                                    table.Cell().Border(0.5f).Padding(3).Text(record.CustomerOrderSlip?.CustomerName);
                                    table.Cell().Border(0.5f).Padding(3).Text(record.CustomerOrderSlip?.CustomerType);
                                    table.Cell().Border(0.5f).Padding(3).Text(record.Terms);
                                    table.Cell().Border(0.5f).Padding(3).Text(record.Customer!.WithHoldingTax ? 1.ToString() : 0.ToString());
                                    table.Cell().Border(0.5f).Padding(3).Text(record.TransactionDate.ToString(SD.Date_Format));
                                    table.Cell().Border(0.5f).Padding(3).Text(record.DueDate.ToString(SD.Date_Format));
                                    table.Cell().Border(0.5f).Padding(3).Text(record.SalesInvoiceNo);
                                    table.Cell().Border(0.5f).Padding(3).Text(record.DeliveryReceipt?.DeliveryReceiptNo);
                                    table.Cell().Border(0.5f).Padding(3).AlignRight().Text(gross != 0 ? gross < 0 ? $"({Math.Abs(gross).ToString(SD.Two_Decimal_Format)})" : gross.ToString(SD.Two_Decimal_Format) : null).FontColor(gross < 0 ? Colors.Red.Medium : Colors.Black);
                                    table.Cell().Border(0.5f).Padding(3).AlignRight().Text(record.AmountPaid != 0 ? record.AmountPaid < 0 ? $"({Math.Abs(record.AmountPaid).ToString(SD.Two_Decimal_Format)})" : record.AmountPaid.ToString(SD.Two_Decimal_Format) : null).FontColor(record.AmountPaid < 0 ? Colors.Red.Medium : Colors.Black);
                                    table.Cell().Border(0.5f).Padding(3).AlignRight().Text(adjustedGross != 0 ? adjustedGross < 0 ? $"({Math.Abs(adjustedGross).ToString(SD.Two_Decimal_Format)})" : adjustedGross.ToString(SD.Two_Decimal_Format) : null).FontColor(adjustedGross < 0 ? Colors.Red.Medium : Colors.Black);
                                    table.Cell().Border(0.5f).Padding(3).AlignRight().Text(withHoldingTaxAmount != 0 ? withHoldingTaxAmount < 0 ? $"({Math.Abs(withHoldingTaxAmount).ToString(SD.Four_Decimal_Format)})" : withHoldingTaxAmount.ToString(SD.Four_Decimal_Format) : null).FontColor(withHoldingTaxAmount < 0 ? Colors.Red.Medium : Colors.Black);
                                    table.Cell().Border(0.5f).Padding(3).AlignRight().Text(netOfVatAmount != 0 ? netOfVatAmount < 0 ? $"({Math.Abs(netOfVatAmount).ToString(SD.Four_Decimal_Format)})" : netOfVatAmount.ToString(SD.Four_Decimal_Format) : null).FontColor(netOfVatAmount < 0 ? Colors.Red.Medium : Colors.Black);
                                    table.Cell().Border(0.5f).Padding(3).AlignRight().Text(vcfAmount != 0 ? vcfAmount < 0 ? $"({Math.Abs(vcfAmount).ToString(SD.Two_Decimal_Format)})" : vcfAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(vcfAmount < 0 ? Colors.Red.Medium : Colors.Black);
                                    table.Cell().Border(0.5f).Padding(3).AlignRight().Text(retentionAmount != 0 ? retentionAmount < 0 ? $"({Math.Abs(retentionAmount).ToString(SD.Four_Decimal_Format)})" : retentionAmount.ToString(SD.Four_Decimal_Format) : null).FontColor(retentionAmount < 0 ? Colors.Red.Medium : Colors.Black);
                                    table.Cell().Border(0.5f).Padding(3).AlignRight().Text(adjustedNet != 0 ? adjustedNet < 0 ? $"({Math.Abs(adjustedNet).ToString(SD.Two_Decimal_Format)})" : adjustedNet.ToString(SD.Two_Decimal_Format) : null).FontColor(adjustedNet < 0 ? Colors.Red.Medium : Colors.Black);
                                    table.Cell().Border(0.5f).Padding(3).AlignRight().Text(daysDue != 0 ? daysDue < 0 ? $"({Math.Abs(daysDue).ToString(SD.Two_Decimal_Format)})" : daysDue.ToString(SD.Two_Decimal_Format) : null).FontColor(daysDue < 0 ? Colors.Red.Medium : Colors.Black);
                                    table.Cell().Border(0.5f).Padding(3).AlignRight().Text(current != 0 ? current < 0 ? $"({Math.Abs(current).ToString(SD.Two_Decimal_Format)})" : current.ToString(SD.Two_Decimal_Format) : null).FontColor(current < 0 ? Colors.Red.Medium : Colors.Black);
                                    table.Cell().Border(0.5f).Padding(3).AlignRight().Text(oneToThirtyDays != 0 ? oneToThirtyDays < 0 ? $"({Math.Abs(oneToThirtyDays).ToString(SD.Two_Decimal_Format)})" : oneToThirtyDays.ToString(SD.Two_Decimal_Format) : null).FontColor(oneToThirtyDays < 0 ? Colors.Red.Medium : Colors.Black);
                                    table.Cell().Border(0.5f).Padding(3).AlignRight().Text(totalThirtyOneToSixtyDays != 0 ? totalThirtyOneToSixtyDays < 0 ? $"({Math.Abs(totalThirtyOneToSixtyDays).ToString(SD.Two_Decimal_Format)})" : totalThirtyOneToSixtyDays.ToString(SD.Two_Decimal_Format) : null).FontColor(totalThirtyOneToSixtyDays < 0 ? Colors.Red.Medium : Colors.Black);
                                    table.Cell().Border(0.5f).Padding(3).AlignRight().Text(sixtyOneToNinetyDays != 0 ? sixtyOneToNinetyDays < 0 ? $"({Math.Abs(sixtyOneToNinetyDays).ToString(SD.Two_Decimal_Format)})" : sixtyOneToNinetyDays.ToString(SD.Two_Decimal_Format) : null).FontColor(sixtyOneToNinetyDays < 0 ? Colors.Red.Medium : Colors.Black);
                                    table.Cell().Border(0.5f).Padding(3).AlignRight().Text(overNinetyDays != 0 ? overNinetyDays < 0 ? $"({Math.Abs(overNinetyDays).ToString(SD.Two_Decimal_Format)})" : overNinetyDays.ToString(SD.Two_Decimal_Format) : null).FontColor(overNinetyDays < 0 ? Colors.Red.Medium : Colors.Black);

                                    totalGrossAmount += record.Amount;
                                    totalAmountPaid += record.AmountPaid;
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

                            #endregion

                            #region -- Create Table Cell for Totals

                                table.Cell().ColumnSpan(9).Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text("TOTAL:").SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalGrossAmount != 0 ? totalGrossAmount < 0 ? $"({Math.Abs(totalGrossAmount).ToString(SD.Two_Decimal_Format)})" : totalGrossAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(totalGrossAmount < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalAmountPaid != 0 ? totalAmountPaid < 0 ? $"({Math.Abs(totalAmountPaid).ToString(SD.Two_Decimal_Format)})" : totalAmountPaid.ToString(SD.Two_Decimal_Format) : null).FontColor(totalAmountPaid < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalAdjustedGross != 0 ? totalAdjustedGross < 0 ? $"({Math.Abs(totalAdjustedGross).ToString(SD.Two_Decimal_Format)})" : totalAdjustedGross.ToString(SD.Two_Decimal_Format) : null).FontColor(totalAdjustedGross < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalWithHoldingTaxAmount != 0 ? totalWithHoldingTaxAmount < 0 ? $"({Math.Abs(totalWithHoldingTaxAmount).ToString(SD.Four_Decimal_Format)})" : totalWithHoldingTaxAmount.ToString(SD.Four_Decimal_Format) : null).FontColor(totalWithHoldingTaxAmount < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalNetOfVatAmount != 0 ? totalNetOfVatAmount < 0 ? $"({Math.Abs(totalNetOfVatAmount).ToString(SD.Four_Decimal_Format)})" : totalNetOfVatAmount.ToString(SD.Four_Decimal_Format) : null).FontColor(totalNetOfVatAmount < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalVcfAmount != 0 ? totalVcfAmount < 0 ? $"({Math.Abs(totalVcfAmount).ToString(SD.Two_Decimal_Format)})" : totalVcfAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(totalVcfAmount < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalRetentionAmount != 0 ? totalRetentionAmount < 0 ? $"({Math.Abs(totalRetentionAmount).ToString(SD.Four_Decimal_Format)})" : totalRetentionAmount.ToString(SD.Four_Decimal_Format) : null).FontColor(totalRetentionAmount < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalAdjustedNet != 0 ? totalAdjustedNet < 0 ? $"({Math.Abs(totalAdjustedNet).ToString(SD.Two_Decimal_Format)})" : totalAdjustedNet.ToString(SD.Two_Decimal_Format) : null).FontColor(totalAdjustedNet < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f);
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalCurrent != 0 ? totalCurrent < 0 ? $"({Math.Abs(totalCurrent).ToString(SD.Two_Decimal_Format)})" : totalCurrent.ToString(SD.Two_Decimal_Format) : null).FontColor(totalCurrent < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalOneToThirtyDays != 0 ? totalOneToThirtyDays < 0 ? $"({Math.Abs(totalOneToThirtyDays).ToString(SD.Two_Decimal_Format)})" : totalOneToThirtyDays.ToString(SD.Two_Decimal_Format) : null).FontColor(totalOneToThirtyDays < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalThirtyOneToSixtyDays != 0 ? totalThirtyOneToSixtyDays < 0 ? $"({Math.Abs(totalThirtyOneToSixtyDays).ToString(SD.Two_Decimal_Format)})" : totalThirtyOneToSixtyDays.ToString(SD.Two_Decimal_Format) : null).FontColor(totalThirtyOneToSixtyDays < 0 ? Colors.Red.Medium : Colors.Black);
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalSixtyOneToNinetyDays != 0 ? totalSixtyOneToNinetyDays < 0 ? $"({Math.Abs(totalSixtyOneToNinetyDays).ToString(SD.Two_Decimal_Format)})" : totalSixtyOneToNinetyDays.ToString(SD.Two_Decimal_Format) : null).FontColor(totalSixtyOneToNinetyDays < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalOverNinetyDays != 0 ? totalOverNinetyDays < 0 ? $"({Math.Abs(totalOverNinetyDays).ToString(SD.Two_Decimal_Format)})" : totalOverNinetyDays.ToString(SD.Two_Decimal_Format) : null).FontColor(totalOverNinetyDays < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();

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

                AuditTrail auditTrailBook = new(GetUserFullName(), "Generate aging report quest pdf", "Accounts Receivable Report", companyClaims);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion

                var pdfBytes = document.GeneratePdf();
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to generate aging report quest pdf. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(AgingReport));
            }
        }

        #endregion

        #region -- Generate Aging Report Excel File --

        public async Task<IActionResult> GenerateAgingReportExcelFile(ViewModelBook model, CancellationToken cancellationToken)
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
                var companyClaims = await GetCompanyClaimAsync();

                var salesInvoice = await _unitOfWork.FilprideSalesInvoice
                    .GetAllAsync(si => si.PostedBy != null
                                       && si.AmountPaid == 0 && !si.IsPaid
                                       && si.Company == companyClaims, cancellationToken);

                if (!salesInvoice.Any())
                {
                    TempData["info"] = "No Record Found";
                    return RedirectToAction(nameof(AgingReport));
                }
                if (companyClaims == null)
                {
                    return BadRequest();
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
                worksheet.Cells["B4"].Value = $"{companyClaims}";

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
                var repoCalculator = _unitOfWork.FilprideSalesInvoice;

                foreach (var si in salesInvoice)
                {
                    var gross = si.Amount;
                    var netDiscount = si.Amount - si.Discount;
                    var netOfVatAmount = (si.CustomerOrderSlip?.VatType ?? SD.VatType_Vatable) == SD.VatType_Vatable
                        ? repoCalculator.ComputeNetOfVat(netDiscount)
                        : netDiscount;
                    var withHoldingTaxAmount = si.CustomerOrderSlip?.HasEWT ?? true
                        ? repoCalculator.ComputeEwtAmount(netDiscount, 0.01m)
                        : 0;
                    var retentionAmount = (si.Customer?.RetentionRate ?? 0.0000m) * netOfVatAmount;
                    var vcfAmount = 0.0000m;
                    var adjustedGross = gross - vcfAmount;
                    var adjustedNet = gross - vcfAmount - retentionAmount;

                    var today = DateOnly.FromDateTime(DateTime.Today);
                    var daysDue = (today > si.DueDate) ? (today.DayNumber - si.DueDate.DayNumber) : 0;
                    var current = (si.DueDate >= today) ? gross : 0.0000m;
                    var oneToThirtyDays = (daysDue >= 1 && daysDue <= 30) ? gross : 0.0000m;
                    var thirtyOneToSixtyDays = (daysDue >= 31 && daysDue <= 60) ? gross : 0.0000m;
                    var sixtyOneToNinetyDays = (daysDue >= 61 && daysDue <= 90) ? gross : 0.0000m;
                    var overNinetyDays = (daysDue > 90) ? gross : 0.0000m;

                    worksheet.Cells[row, 1].Value = si.TransactionDate.ToString("MMMM yyyy");
                    worksheet.Cells[row, 2].Value = si.CustomerOrderSlip?.CustomerName;
                    worksheet.Cells[row, 3].Value = si.CustomerOrderSlip?.CustomerType;
                    worksheet.Cells[row, 4].Value = si.Terms;
                    worksheet.Cells[row, 5].Value = si.Customer?.WithHoldingTax ?? false ? "1" : "0";
                    worksheet.Cells[row, 6].Value = si.TransactionDate;
                    worksheet.Cells[row, 7].Value = si.DueDate;
                    worksheet.Cells[row, 8].Value = si.SalesInvoiceNo;
                    worksheet.Cells[row, 9].Value = si.DeliveryReceipt?.DeliveryReceiptNo;
                    worksheet.Cells[row, 10].Value = gross;
                    worksheet.Cells[row, 11].Value = si.AmountPaid;
                    worksheet.Cells[row, 12].Value = adjustedGross;
                    worksheet.Cells[row, 13].Value = withHoldingTaxAmount;
                    worksheet.Cells[row, 14].Value = netOfVatAmount;
                    worksheet.Cells[row, 15].Value = vcfAmount;
                    worksheet.Cells[row, 16].Value = retentionAmount;
                    worksheet.Cells[row, 17].Value = adjustedNet;
                    worksheet.Cells[row, 18].Value = daysDue;
                    worksheet.Cells[row, 19].Value = current;
                    worksheet.Cells[row, 20].Value = oneToThirtyDays;
                    worksheet.Cells[row, 21].Value = thirtyOneToSixtyDays;
                    worksheet.Cells[row, 22].Value = sixtyOneToNinetyDays;
                    worksheet.Cells[row, 23].Value = overNinetyDays;

                    worksheet.Cells[row, 6].Style.Numberformat.Format = "MMM/dd/yyyy";
                    worksheet.Cells[row, 7].Style.Numberformat.Format = "MMM/dd/yyyy";
                    worksheet.Cells[row, 10].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 11].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 12].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 13].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 14].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 15].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 16].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 17].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 19].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 20].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 21].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 22].Style.Numberformat.Format = currencyFormat;
                    worksheet.Cells[row, 23].Style.Numberformat.Format = currencyFormat;

                    row++;

                    totalGrossAmount += si.Amount;
                    totalAmountPaid += si.AmountPaid;
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

                worksheet.Cells[row, 9].Value = "Total ";
                worksheet.Cells[row, 10].Value = totalGrossAmount;
                worksheet.Cells[row, 11].Value = totalAmountPaid;
                worksheet.Cells[row, 12].Value = totalAdjustedGross;
                worksheet.Cells[row, 13].Value = totalWithHoldingTaxAmount;
                worksheet.Cells[row, 14].Value = totalNetOfVatAmount;
                worksheet.Cells[row, 15].Value = totalVcfAmount;
                worksheet.Cells[row, 16].Value = totalRetentionAmount;
                worksheet.Cells[row, 17].Value = totalAdjustedNet;
                worksheet.Cells[row, 19].Value = totalCurrent;
                worksheet.Cells[row, 20].Value = totalOneToThirtyDays;
                worksheet.Cells[row, 21].Value = totalThirtyOneToSixtyDays;
                worksheet.Cells[row, 22].Value = totalSixtyOneToNinetyDays;
                worksheet.Cells[row, 23].Value = totalOverNinetyDays;

                worksheet.Cells[row, 10].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 11].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 12].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 13].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 14].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 15].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 16].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 17].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 19].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 20].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 21].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 22].Style.Numberformat.Format = currencyFormat;
                worksheet.Cells[row, 23].Style.Numberformat.Format = currencyFormat;

                // Apply style to subtotal row
                using (var range = worksheet.Cells[row, 1, row, 23])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Fill.PatternType = ExcelFillStyle.Solid;
                    range.Style.Fill.BackgroundColor.SetColor(Color.FromArgb(172, 185, 202));
                }

                using (var range = worksheet.Cells[row, 9, row, 23])
                {
                    range.Style.Font.Bold = true;
                    range.Style.Border.Top.Style = ExcelBorderStyle.Thin; // Single top border
                    range.Style.Border.Bottom.Style = ExcelBorderStyle.Double; // Double bottom border
                }

                // Auto-fit columns for better readability
                worksheet.Cells.AutoFitColumns();
                worksheet.View.FreezePanes(8, 1);

                #region -- Audit Trail --

                AuditTrail auditTrailBook = new(GetUserFullName(), "Generate aging report excel file", "Accounts Receivable Report", companyClaims);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

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
                _logger.LogError(ex, "Failed to generate aging report excel file. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(AgingReport));
            }
        }

        #endregion

        [HttpGet]
        public async Task<IActionResult> ArPerCustomer()
        {
            var companyClaims = await GetCompanyClaimAsync();
            if (companyClaims == null)
            {
                return BadRequest();
            }

            ViewModelBook viewmodel = new()
            {
                CustomerList = await _unitOfWork.GetFilprideCustomerListAsyncById(companyClaims)
            };

            return View(viewmodel);
        }

        #region -- Generated AR Per Customer Report as Quest PDF

        public async Task<IActionResult> GeneratedArPerCustomer(ViewModelBook model, CancellationToken cancellationToken)
        {
            var companyClaims = await GetCompanyClaimAsync();

            if (companyClaims == null)
            {
                return BadRequest();
            }
            var statusFilter = NormalizeStatusFilter(model.StatusFilter);

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The submitted information is invalid.";
                return RedirectToAction(nameof(ArPerCustomer));
            }

            try
            {
                var salesInvoice = await _unitOfWork.FilprideReport
                    .GetARPerCustomerReport(model.DateFrom, model.DateTo, companyClaims, model.Customers, statusFilter, cancellationToken);

                if (!salesInvoice.Any())
                {
                    TempData["info"] = "No records found";
                    return RedirectToAction(nameof(ArPerCustomer));
                }

                var document = Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        #region -- Page Setup

                            page.Size(PageSizes.Legal.Landscape());
                            page.Margin(20);
                            page.DefaultTextStyle(x => x.FontSize(7).FontFamily("Times New Roman"));

                        #endregion

                        #region -- Header

                            var imgFilprideLogoPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "Filpride-logo.png");

                            page.Header().Height(50).Row(row =>
                            {
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item()
                                        .Text("AR PER CUSTOMER REPORT")
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
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Customer No.").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Customer Name").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Acc. Type").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Terms").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Tran. Date").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Due Date").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Invoice No.").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("DR No.").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("PO No.").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("COS No.").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Remarks").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Product").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Quantity").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Unit").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Unit Price").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Freight").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Freight/Ltr").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("VAT/Ltr").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("VAT Amt.").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Total Amt.").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Amt. Paid").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("SI Balance").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("EWT Amt").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("EWT Paid").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("CWT Balance").SemiBold();
                                });

                            #endregion

                            #region -- Loop to Show Records

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
                                var repoCalculator = _unitOfWork.FilprideDeliveryReceipt;

                                foreach (var groupByCustomer in salesInvoice.GroupBy(x => x.Customer))
                                {
                                    foreach (var record in groupByCustomer)
                                    {
                                        var isVatable = (record.CustomerOrderSlip?.VatType ?? SD.VatType_Vatable) ==
                                                        SD.VatType_Vatable;
                                        var isTaxable = record.CustomerOrderSlip?.HasEWT ?? true;
                                        var freight = record.DeliveryReceipt?.FreightAmount;
                                        var grossAmount = record.Amount;
                                        var netOfVat = isVatable
                                            ? repoCalculator.ComputeNetOfVat(grossAmount)
                                            : grossAmount;
                                        var vatAmount = isVatable
                                            ? repoCalculator.ComputeVatAmount(netOfVat)
                                            : 0m;
                                        var vatPerLiter = vatAmount / record.Quantity;
                                        var ewtAmount = isTaxable
                                            ? repoCalculator.ComputeEwtAmount(netOfVat, 0.01m)
                                            : 0m;
                                        var isEwtAmountPaid = record.IsTaxAndVatPaid ? ewtAmount : 0m;
                                        var ewtBalance = ewtAmount - isEwtAmountPaid;

                                        table.Cell().Border(0.5f).Padding(3).Text(record.Customer?.CustomerCode);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.CustomerOrderSlip?.CustomerName ?? record.Customer?.CustomerName);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.CustomerOrderSlip?.CustomerType ?? record.Customer?.Type);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.Terms);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.TransactionDate.ToString(SD.Date_Format));
                                        table.Cell().Border(0.5f).Padding(3).Text(record.DueDate.ToString(SD.Date_Format));
                                        table.Cell().Border(0.5f).Padding(3).Text(record.SalesInvoiceNo);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.DeliveryReceipt?.DeliveryReceiptNo);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.CustomerOrderSlip?.CustomerPoNo);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.CustomerOrderSlip?.CustomerOrderSlipNo);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.Remarks);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.CustomerOrderSlip?.ProductName);
                                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(record.Quantity != 0 ? record.Quantity < 0 ? $"({Math.Abs(record.Quantity).ToString(SD.Two_Decimal_Format)})" : record.Quantity.ToString(SD.Two_Decimal_Format) : null).FontColor(record.Quantity < 0 ? Colors.Red.Medium : Colors.Black);
                                        table.Cell().Border(0.5f).Padding(3).Text(record.Product?.ProductUnit);
                                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(record.UnitPrice != 0 ? record.UnitPrice < 0 ? $"({Math.Abs(record.UnitPrice).ToString(SD.Four_Decimal_Format)})" : record.UnitPrice.ToString(SD.Four_Decimal_Format) : null).FontColor(record.UnitPrice < 0 ? Colors.Red.Medium : Colors.Black);
                                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(freight != 0 ? freight < 0 ? $"({Math.Abs((decimal)freight).ToString(SD.Two_Decimal_Format)})" : freight?.ToString(SD.Two_Decimal_Format) : null).FontColor(freight < 0 ? Colors.Red.Medium : Colors.Black);
                                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(record.DeliveryReceipt?.Freight != 0 ? record.DeliveryReceipt?.Freight < 0 ? $"({Math.Abs(record.DeliveryReceipt?.Freight ?? 0).ToString(SD.Four_Decimal_Format)})" : record.DeliveryReceipt?.Freight.ToString(SD.Four_Decimal_Format) : null).FontColor(record.DeliveryReceipt?.Freight < 0 ? Colors.Red.Medium : Colors.Black);
                                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(vatPerLiter != 0 ? vatPerLiter < 0 ? $"({Math.Abs(vatPerLiter).ToString(SD.Two_Decimal_Format)})" : vatPerLiter.ToString(SD.Two_Decimal_Format) : null).FontColor(vatPerLiter < 0 ? Colors.Red.Medium : Colors.Black);
                                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(vatAmount != 0 ? vatAmount < 0 ? $"({Math.Abs(vatAmount).ToString(SD.Two_Decimal_Format)})" : vatAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(vatAmount < 0 ? Colors.Red.Medium : Colors.Black);
                                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(grossAmount != 0 ? grossAmount < 0 ? $"({Math.Abs(grossAmount).ToString(SD.Two_Decimal_Format)})" : grossAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(grossAmount < 0 ? Colors.Red.Medium : Colors.Black);
                                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(record.AmountPaid != 0 ? record.AmountPaid < 0 ? $"({Math.Abs(record.AmountPaid).ToString(SD.Two_Decimal_Format)})" : record.AmountPaid.ToString(SD.Two_Decimal_Format) : null).FontColor(record.AmountPaid < 0 ? Colors.Red.Medium : Colors.Black);
                                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(record.Balance != 0 ? record.Balance < 0 ? $"({Math.Abs(record.Balance).ToString(SD.Two_Decimal_Format)})" : record.Balance.ToString(SD.Two_Decimal_Format) : null).FontColor(record.Balance < 0 ? Colors.Red.Medium : Colors.Black);
                                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(ewtAmount != 0 ? ewtAmount < 0 ? $"({Math.Abs(ewtAmount).ToString(SD.Two_Decimal_Format)})" : ewtAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(ewtAmount < 0 ? Colors.Red.Medium : Colors.Black);
                                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(isEwtAmountPaid != 0 ? isEwtAmountPaid < 0 ? $"({Math.Abs(isEwtAmountPaid).ToString(SD.Two_Decimal_Format)})" : isEwtAmountPaid.ToString(SD.Two_Decimal_Format) : null).FontColor(isEwtAmountPaid < 0 ? Colors.Red.Medium : Colors.Black);
                                        table.Cell().Border(0.5f).Padding(3).AlignRight().Text(ewtBalance != 0 ? ewtBalance < 0 ? $"({Math.Abs(ewtBalance).ToString(SD.Two_Decimal_Format)})" : ewtBalance.ToString(SD.Two_Decimal_Format) : null).FontColor(ewtBalance < 0 ? Colors.Red.Medium : Colors.Black);

                                        totalQuantity += record.Quantity;
                                        totalFreight += freight ?? 0m;
                                        totalFreightPerLiter += record.DeliveryReceipt?.Freight ?? 0m;
                                        totalVatPerLiter += vatPerLiter;
                                        totalVatAmount += vatAmount;
                                        totalGrossAmount += grossAmount;
                                        totalAmountPaid += record.AmountPaid;
                                        totalBalance += record.Balance;
                                        totalEwtAmount += ewtAmount;
                                        totalEwtAmountPaid += isEwtAmountPaid;
                                        totalEwtBalance += ewtBalance;
                                    }

                                    var subTotalQuantity = groupByCustomer.Sum(x => x.Quantity);

                                    var isVatableSub = groupByCustomer.Select(x => x.CustomerOrderSlip?.VatType).FirstOrDefault();
                                    var isTaxableSub = groupByCustomer.Select(x => x.CustomerOrderSlip?.HasEWT).FirstOrDefault();
                                    var subTotalFreight = groupByCustomer.Sum(x => x.DeliveryReceipt?.FreightAmount) ?? 0m;
                                    var subTotalFreightPerLiter = subTotalFreight != 0m && subTotalQuantity != 0m ? subTotalFreight / subTotalQuantity : 0m;
                                    var subTotalGrossAmount = groupByCustomer.Sum(x => x.Amount);
                                    var subTotalNetOfVat = isVatableSub == SD.VatType_Vatable
                                        ? repoCalculator.ComputeNetOfVat(subTotalGrossAmount)
                                        : subTotalGrossAmount;
                                    var subTotalVatAmount = isVatableSub == SD.VatType_Vatable
                                        ? repoCalculator.ComputeVatAmount(subTotalNetOfVat)
                                        : 0m;
                                    var subTotalAmountPaid = groupByCustomer.Sum(x => x.AmountPaid);
                                    var subTotalVatPerLiter = subTotalVatAmount / subTotalQuantity;
                                    var subTotalEwtAmount = isTaxableSub == true
                                        ? repoCalculator.ComputeEwtAmount(subTotalNetOfVat, 0.01m)
                                        : 0m;
                                    var isEwtAmountPaidSub = groupByCustomer.Select(x => x.IsTaxAndVatPaid).FirstOrDefault() ? subTotalEwtAmount : 0m;
                                    var subTotalEwtBalance = subTotalEwtAmount - isEwtAmountPaidSub;
                                    var subTotalUnitPrice = subTotalGrossAmount / subTotalQuantity;
                                    var subTotalBalance = groupByCustomer.Sum(x => x.Balance);
                                    var subTotalEwtAmountPaid = isEwtAmountPaidSub;

                                    table.Cell().ColumnSpan(12).Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text("SUB TOTAL:").SemiBold();
                                    table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(subTotalQuantity != 0 ? subTotalQuantity < 0 ? $"({Math.Abs(subTotalQuantity).ToString(SD.Two_Decimal_Format)})" : subTotalQuantity.ToString(SD.Two_Decimal_Format) : null).FontColor(subTotalQuantity < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                    table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f);
                                    table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(subTotalUnitPrice != 0 ? subTotalUnitPrice < 0 ? $"({Math.Abs(subTotalUnitPrice).ToString(SD.Four_Decimal_Format)})" : subTotalUnitPrice.ToString(SD.Four_Decimal_Format) : null).FontColor(subTotalUnitPrice < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                    table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(subTotalFreight != 0 ? subTotalFreight < 0 ? $"({Math.Abs(subTotalFreight).ToString(SD.Two_Decimal_Format)})" : subTotalFreight.ToString(SD.Two_Decimal_Format) : null).FontColor(subTotalFreight < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                    table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(subTotalFreightPerLiter != 0 ? subTotalFreightPerLiter < 0 ? $"({Math.Abs(subTotalFreightPerLiter).ToString(SD.Four_Decimal_Format)})" : subTotalFreightPerLiter.ToString(SD.Four_Decimal_Format) : null).FontColor(subTotalFreightPerLiter < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                    table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(subTotalVatPerLiter != 0 ? subTotalVatPerLiter < 0 ? $"({Math.Abs(subTotalVatPerLiter).ToString(SD.Two_Decimal_Format)})" : subTotalVatPerLiter.ToString(SD.Two_Decimal_Format) : null).FontColor(subTotalVatPerLiter < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                    table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(subTotalVatAmount != 0 ? subTotalVatAmount < 0 ? $"({Math.Abs(subTotalVatAmount).ToString(SD.Two_Decimal_Format)})" : subTotalVatAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(subTotalVatAmount < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                    table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(subTotalGrossAmount != 0 ? subTotalGrossAmount < 0 ? $"({Math.Abs(subTotalGrossAmount).ToString(SD.Two_Decimal_Format)})" : subTotalGrossAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(subTotalGrossAmount < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                    table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(subTotalAmountPaid != 0 ? subTotalAmountPaid < 0 ? $"({Math.Abs(subTotalAmountPaid).ToString(SD.Two_Decimal_Format)})" : subTotalAmountPaid.ToString(SD.Two_Decimal_Format) : null).FontColor(subTotalAmountPaid < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                    table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(subTotalBalance != 0 ? subTotalBalance < 0 ? $"({Math.Abs(subTotalBalance).ToString(SD.Two_Decimal_Format)})" : subTotalBalance.ToString(SD.Two_Decimal_Format) : null).FontColor(subTotalBalance < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                    table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(subTotalEwtAmount != 0 ? subTotalEwtAmount < 0 ? $"({Math.Abs(subTotalEwtAmount).ToString(SD.Two_Decimal_Format)})" : subTotalEwtAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(subTotalEwtAmount < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                    table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(subTotalEwtAmountPaid != 0 ? subTotalEwtAmountPaid < 0 ? $"({Math.Abs(subTotalEwtAmountPaid).ToString(SD.Two_Decimal_Format)})" : subTotalEwtAmountPaid.ToString(SD.Two_Decimal_Format) : null).FontColor(subTotalEwtAmountPaid < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                    table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(subTotalEwtBalance != 0 ? subTotalEwtBalance < 0 ? $"({Math.Abs(subTotalEwtBalance).ToString(SD.Two_Decimal_Format)})" : subTotalEwtBalance.ToString(SD.Two_Decimal_Format) : null).FontColor(subTotalEwtBalance < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                }

                                totalFreightPerLiter = totalFreight != 0 && totalQuantity != 0 ? totalFreight / totalQuantity : 0m;
                            #endregion

                            #region -- Create Table Cell for Totals

                                var unitPrice = totalGrossAmount / totalQuantity;

                                table.Cell().ColumnSpan(12).Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text("GRAND TOTAL:").SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalQuantity != 0 ? totalQuantity < 0 ? $"({Math.Abs(totalQuantity).ToString(SD.Two_Decimal_Format)})" : totalQuantity.ToString(SD.Two_Decimal_Format) : null).FontColor(totalQuantity < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f);
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(unitPrice != 0 ? unitPrice < 0 ? $"({Math.Abs(unitPrice).ToString(SD.Four_Decimal_Format)})" : unitPrice.ToString(SD.Four_Decimal_Format) : null).FontColor(unitPrice < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalFreight != 0 ? totalFreight < 0 ? $"({Math.Abs(totalFreight).ToString(SD.Two_Decimal_Format)})" : totalFreight.ToString(SD.Two_Decimal_Format) : null).FontColor(totalFreight < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalFreightPerLiter != 0 ? totalFreightPerLiter < 0 ? $"({Math.Abs(totalFreightPerLiter).ToString(SD.Four_Decimal_Format)})" : totalFreightPerLiter.ToString(SD.Four_Decimal_Format) : null).FontColor(totalFreightPerLiter < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalVatPerLiter != 0 ? totalVatPerLiter < 0 ? $"({Math.Abs(totalVatPerLiter).ToString(SD.Two_Decimal_Format)})" : totalVatPerLiter.ToString(SD.Two_Decimal_Format) : null).FontColor(totalVatPerLiter < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalVatAmount != 0 ? totalVatAmount < 0 ? $"({Math.Abs(totalVatAmount).ToString(SD.Two_Decimal_Format)})" : totalVatAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(totalVatAmount < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalGrossAmount != 0 ? totalGrossAmount < 0 ? $"({Math.Abs(totalGrossAmount).ToString(SD.Two_Decimal_Format)})" : totalGrossAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(totalGrossAmount < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalAmountPaid != 0 ? totalAmountPaid < 0 ? $"({Math.Abs(totalAmountPaid).ToString(SD.Two_Decimal_Format)})" : totalAmountPaid.ToString(SD.Two_Decimal_Format) : null).FontColor(totalAmountPaid < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalBalance != 0 ? totalBalance < 0 ? $"({Math.Abs(totalBalance).ToString(SD.Two_Decimal_Format)})" : totalBalance.ToString(SD.Two_Decimal_Format) : null).FontColor(totalBalance < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalEwtAmount != 0 ? totalEwtAmount < 0 ? $"({Math.Abs(totalEwtAmount).ToString(SD.Two_Decimal_Format)})" : totalEwtAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(totalEwtAmount < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalEwtAmountPaid != 0 ? totalEwtAmountPaid < 0 ? $"({Math.Abs(totalEwtAmountPaid).ToString(SD.Two_Decimal_Format)})" : totalEwtAmountPaid.ToString(SD.Two_Decimal_Format) : null).FontColor(totalEwtAmountPaid < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalEwtBalance != 0 ? totalEwtBalance < 0 ? $"({Math.Abs(totalEwtBalance).ToString(SD.Two_Decimal_Format)})" : totalEwtBalance.ToString(SD.Two_Decimal_Format) : null).FontColor(totalEwtBalance < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();

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

                AuditTrail auditTrailBook = new(GetUserFullName(), "Generate ar per customer report quest pdf", "Accounts Receivable Report", companyClaims);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion

                var pdfBytes = document.GeneratePdf();
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to generate AR per customer report quest pdf. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(ArPerCustomer));
            }
        }

        #endregion

        #region -- Generate AR Per Customer Excel File --

        public async Task<IActionResult> GenerateArPerCustomerExcelFile(ViewModelBook model, CancellationToken cancellationToken)
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
                var companyClaims = await GetCompanyClaimAsync();
                if (companyClaims == null)
                {
                    return BadRequest();
                }

                var statusFilter = NormalizeStatusFilter(model.StatusFilter);

                var salesInvoice = await _unitOfWork.FilprideReport
                    .GetARPerCustomerReport(model.DateFrom, model.DateTo, companyClaims, model.Customers, statusFilter, cancellationToken);

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

                worksheet.Cells["B2"].Value = $"{dateFrom.ToString(SD.Date_Format)} - {dateTo.ToString(SD.Date_Format)}";
                worksheet.Cells["B3"].Value = $"{extractedBy}";
                worksheet.Cells["B4"].Value = $"{companyClaims}";
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
                var repoCalculator = _unitOfWork.FilprideDeliveryReceipt;

                foreach (var groupByCustomer in salesInvoice.GroupBy(x => x.Customer))
                {
                    foreach (var si in groupByCustomer)
                    {
                        var isVatable = (si.CustomerOrderSlip?.VatType ?? SD.VatType_Vatable) == SD.VatType_Vatable;
                        var isTaxable = si.CustomerOrderSlip?.HasEWT ?? true;
                        var freight = si.DeliveryReceipt?.FreightAmount;
                        var grossAmount = si.Amount;
                        var netOfVat = isVatable
                            ? repoCalculator.ComputeNetOfVat(grossAmount)
                            : grossAmount;
                        var vatAmount = isVatable ? repoCalculator.ComputeVatAmount(netOfVat) : 0m;
                        var vatPerLiter = vatAmount / si.Quantity;
                        var ewtAmount = isTaxable ? repoCalculator.ComputeEwtAmount(netOfVat, 0.01m) : 0m;
                        var isEwtAmountPaid = si.IsTaxAndVatPaid ? ewtAmount : 0m;
                        var ewtBalance = ewtAmount - isEwtAmountPaid;

                        worksheet.Cells[row, 1].Value = si.Customer?.CustomerCode;
                        worksheet.Cells[row, 2].Value = si.CustomerOrderSlip?.CustomerName ?? si.Customer?.CustomerName;
                        worksheet.Cells[row, 3].Value = si.CustomerOrderSlip?.CustomerType ?? si.Customer?.CustomerType;
                        worksheet.Cells[row, 4].Value = si.Terms;
                        worksheet.Cells[row, 5].Value = si.TransactionDate;
                        worksheet.Cells[row, 6].Value = si.DueDate;
                        worksheet.Cells[row, 7].Value = si.SalesInvoiceNo;
                        worksheet.Cells[row, 8].Value = si.DeliveryReceipt?.DeliveryReceiptNo;
                        worksheet.Cells[row, 9].Value = si.CustomerOrderSlip?.CustomerPoNo;
                        worksheet.Cells[row, 10].Value = si.CustomerOrderSlip?.CustomerOrderSlipNo;
                        worksheet.Cells[row, 11].Value = si.Remarks;
                        worksheet.Cells[row, 12].Value = si.CustomerOrderSlip?.ProductName;
                        worksheet.Cells[row, 13].Value = si.Quantity;
                        worksheet.Cells[row, 14].Value = si.Product?.ProductUnit;
                        worksheet.Cells[row, 15].Value = si.UnitPrice;
                        worksheet.Cells[row, 16].Value = freight;
                        worksheet.Cells[row, 17].Value = si.DeliveryReceipt?.Freight;
                        worksheet.Cells[row, 18].Value = vatPerLiter;
                        worksheet.Cells[row, 19].Value = vatAmount;
                        worksheet.Cells[row, 20].Value = grossAmount;
                        worksheet.Cells[row, 21].Value = si.AmountPaid;
                        worksheet.Cells[row, 22].Value = si.Balance;
                        worksheet.Cells[row, 23].Value = ewtAmount;
                        worksheet.Cells[row, 24].Value = isEwtAmountPaid;
                        worksheet.Cells[row, 25].Value = ewtBalance;

                        // Add void/cancel data — only for All or InvalidOnly
                        if (showVoidCancelColumns)
                        {
                            worksheet.Cells[row, 26].Value = si.VoidedBy;
                            worksheet.Cells[row, 27].Value = si.VoidedDate;
                            if (si.VoidedDate.HasValue)
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

                        totalQuantity += si.Quantity;
                        totalFreight += freight ?? 0m;
                        totalFreightPerLiter += si.DeliveryReceipt?.Freight ?? 0m;
                        totalVatPerLiter += vatPerLiter;
                        totalVatAmount += vatAmount;
                        totalGrossAmount += grossAmount;
                        totalAmountPaid += si.AmountPaid;
                        totalBalance += si.Balance;
                        totalEwtAmount += ewtAmount;
                        totalEwtAmountPaid += isEwtAmountPaid;
                        totalEwtBalance += ewtBalance;
                    }
                    var subTotalQuantity = groupByCustomer.Sum(x => x.Quantity);

                    var isVatableSub = groupByCustomer.Select(x => x.CustomerOrderSlip?.VatType).FirstOrDefault();
                    var isTaxableSub = groupByCustomer.Select(x => x.CustomerOrderSlip?.HasEWT).FirstOrDefault();
                    var subTotalFreight = groupByCustomer.Sum(x => x.DeliveryReceipt?.FreightAmount) ?? 0m;
                    var subTotalFreightPerLiter = subTotalFreight != 0m && subTotalQuantity != 0m ? subTotalFreight / subTotalQuantity : 0m;
                    var subTotalGrossAmount = groupByCustomer.Sum(x => x.Amount);
                    var subTotalNetOfVat = isVatableSub == SD.VatType_Vatable
                        ? repoCalculator.ComputeNetOfVat(subTotalGrossAmount)
                        : subTotalGrossAmount;
                    var subTotalVatAmount = isVatableSub == SD.VatType_Vatable
                        ? repoCalculator.ComputeVatAmount(subTotalNetOfVat)
                        : 0m;
                    var subTotalAmountPaid = groupByCustomer.Sum(x => x.AmountPaid);
                    var subTotalVatPerLiter = subTotalVatAmount / subTotalQuantity;
                    var subTotalEwtAmount = isTaxableSub == true
                        ? repoCalculator.ComputeEwtAmount(subTotalNetOfVat, 0.01m)
                        : 0m;
                    var isEwtAmountPaidSub = groupByCustomer.Select(x => x.IsTaxAndVatPaid).FirstOrDefault() ? subTotalEwtAmount : 0m;
                    var subTotalEwtBalance = subTotalEwtAmount - isEwtAmountPaidSub;
                    var subTotalUnitPrice = subTotalGrossAmount / subTotalQuantity;
                    var subTotalBalance = groupByCustomer.Sum(x => x.Balance);
                    var subTotalEwtAmountPaid = isEwtAmountPaidSub;

                    worksheet.Cells[row, 12].Value = "SUB TOTAL ";

                    worksheet.Cells[row, 13].Value = subTotalQuantity;
                    worksheet.Cells[row, 15].Value = subTotalUnitPrice;
                    worksheet.Cells[row, 16].Value = subTotalFreight;
                    worksheet.Cells[row, 17].Value = subTotalFreightPerLiter;
                    worksheet.Cells[row, 18].Value = subTotalVatPerLiter;
                    worksheet.Cells[row, 19].Value = subTotalVatAmount;
                    worksheet.Cells[row, 20].Value = subTotalGrossAmount;
                    worksheet.Cells[row, 21].Value = subTotalAmountPaid;
                    worksheet.Cells[row, 22].Value = subTotalBalance;
                    worksheet.Cells[row, 23].Value = subTotalEwtAmount;
                    worksheet.Cells[row, 24].Value = subTotalEwtAmountPaid;
                    worksheet.Cells[row, 25].Value = subTotalEwtBalance;

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

                AuditTrail auditTrailBook = new(GetUserFullName(), "Generate ar per customer report excel file", "Accounts Receivable Report", companyClaims);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion

                var fileName = $"AR_Per_Customer_Report_{DateTimeHelper.GetCurrentPhilippineTime():yyyyddMMHHmmss}.xlsx";
                var stream = new MemoryStream();
                await package.SaveAsAsync(stream, cancellationToken);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to generate ar per customer report excel file. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
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

        public async Task<IActionResult> GeneratedServiceInvoiceReport(ViewModelBook model, CancellationToken cancellationToken)
        {
            var companyClaims = await GetCompanyClaimAsync();


            if (companyClaims == null)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                TempData["warning"] = "The submitted information is invalid.";
                return RedirectToAction(nameof(ServiceInvoiceReport));
            }

            var statusFilter = NormalizeStatusFilter(model.StatusFilter);

            try
            {
                var serviceInvoice = await _unitOfWork.FilprideReport
                    .GetServiceInvoiceReport(model.DateFrom, model.DateTo, companyClaims, statusFilter, cancellationToken);

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

                            var imgFilprideLogoPath = Path.Combine(_webHostEnvironment.WebRootPath, "img", "Filpride-logo.png");

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
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Transaction Date").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Customer Name").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Customer Address").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Customer TIN").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Service Invoice#").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Service").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Period").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Due Date").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("G. Amount").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Amount Paid").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Payment Status").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Instructions").SemiBold();
                                    header.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignCenter().AlignMiddle().Text("Type").SemiBold();
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
                                    table.Cell().Border(0.5f).Padding(3).AlignRight().Text(record.Total != 0 ? record.Total < 0 ? $"({Math.Abs(record.Total).ToString(SD.Two_Decimal_Format)})" : record.Total.ToString(SD.Two_Decimal_Format) : null).FontColor(record.Total < 0 ? Colors.Red.Medium : Colors.Black);
                                    table.Cell().Border(0.5f).Padding(3).AlignRight().Text(record.AmountPaid != 0 ? record.AmountPaid < 0 ? $"({Math.Abs(record.AmountPaid).ToString(SD.Two_Decimal_Format)})" : record.AmountPaid.ToString(SD.Two_Decimal_Format) : null).FontColor(record.AmountPaid < 0 ? Colors.Red.Medium : Colors.Black);
                                    table.Cell().Border(0.5f).Padding(3).Text(record.PaymentStatus);
                                    table.Cell().Border(0.5f).Padding(3).Text(record.Instructions);
                                    table.Cell().Border(0.5f).Padding(3).Text(record.Type);

                                    totalAmount += record.Total;
                                    totalAmountPaid += record.AmountPaid;
                                }

                            #endregion

                            #region -- Create Table Cell for Totals

                                table.Cell().ColumnSpan(8).Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text("TOTAL:").SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalAmount != 0 ? totalAmount < 0 ? $"({Math.Abs(totalAmount).ToString(SD.Two_Decimal_Format)})" : totalAmount.ToString(SD.Two_Decimal_Format) : null).FontColor(totalAmount < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
                                table.Cell().Background(Colors.Grey.Lighten1).Border(0.5f).Padding(3).AlignRight().Text(totalAmountPaid != 0 ? totalAmountPaid < 0 ? $"({Math.Abs(totalAmountPaid).ToString(SD.Two_Decimal_Format)})" : totalAmountPaid.ToString(SD.Two_Decimal_Format) : null).FontColor(totalAmountPaid < 0 ? Colors.Red.Medium : Colors.Black).SemiBold();
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

                AuditTrail auditTrailBook = new(GetUserFullName(), "Generate service invoice report quest pdf", "Accounts Receivable Report", companyClaims);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion

                var pdfBytes = document.GeneratePdf();
                return File(pdfBytes, "application/pdf");
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to generate service invoice report quest pdf. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(ServiceInvoiceReport));
            }
        }

        #endregion

        #region -- Generate Service Invoice Report Excel File --

        public async Task<IActionResult> GenerateServiceInvoiceReportExcelFile(ViewModelBook model, CancellationToken cancellationToken)
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
                var companyClaims = await GetCompanyClaimAsync();
                if (companyClaims == null)
                {
                    return BadRequest();
                }
                var statusFilter = NormalizeStatusFilter(model.StatusFilter);

                var serviceReport = await _unitOfWork.FilprideReport.GetServiceInvoiceReport(model.DateFrom, model.DateTo, companyClaims, statusFilter, cancellationToken);

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
                worksheet.Cells["B4"].Value = $"{companyClaims}";
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

                AuditTrail auditTrailBook = new(GetUserFullName(), "Generate service invoice report excel file", "Accounts Receivable Report", companyClaims);
                await _unitOfWork.FilprideAuditTrail.AddAsync(auditTrailBook, cancellationToken);

                #endregion

                var fileName = $"Service_Invoice_Report_{DateTimeHelper.GetCurrentPhilippineTime():yyyyddMMHHmmss}.xlsx";
                var stream = new MemoryStream();
                await package.SaveAsAsync(stream, cancellationToken);
                stream.Position = 0;
                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["error"] = ex.Message;
                _logger.LogError(ex, "Failed to generate dispatch report excel file. Error: {ErrorMessage}, Stack: {StackTrace}. Generated by: {UserName}",
                    ex.Message, ex.StackTrace, _userManager.GetUserName(User));
                return RedirectToAction(nameof(ServiceInvoiceReport));
            }
        }

        #endregion
    }
}
