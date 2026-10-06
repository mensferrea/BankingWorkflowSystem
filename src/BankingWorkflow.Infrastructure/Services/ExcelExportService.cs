using BankingWorkflow.Application.DTOs.Applications;
using BankingWorkflow.Application.Interfaces;
using BankingWorkflow.Domain.Entities;
using BankingWorkflow.Domain.Enums;
using ClosedXML.Excel;

namespace BankingWorkflow.Infrastructure.Services;

public class ExcelExportService : IExcelExportService
{
    private readonly IApplicationService _applicationService;

    public ExcelExportService(IApplicationService applicationService)
    {
        _applicationService = applicationService;
    }

    public async Task<byte[]> ExportApplicationsToExcelAsync(
        ApplicationFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var unpagedFilter = new ApplicationFilterDto
        {
            Search = filter.Search,
            Status = filter.Status,
            ProductType = filter.ProductType,
            MinAmount = filter.MinAmount,
            MaxAmount = filter.MaxAmount,
            DateFrom = filter.DateFrom,
            DateTo = filter.DateTo,
            SortBy = filter.SortBy,
            SortDescending = filter.SortDescending,
            PageNumber = 1,
            PageSize = 100_000
        };

        var (items, total) = await _applicationService.GetPagedAsync(unpagedFilter, cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Реестр заявок");

        worksheet.Cell(1, 1).Value = "БАНКОВСКИЙ РЕЕСТР КРЕДИТНЫХ ЗАЯВОК";
        worksheet.Range(1, 1, 1, 9).Merge();
        worksheet.Cell(1, 1).Style.Font.Bold = true;
        worksheet.Cell(1, 1).Style.Font.FontSize = 14;
        worksheet.Cell(1, 1).Style.Font.FontColor = XLColor.FromHtml("#0D47A1");
        worksheet.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        worksheet.Cell(2, 1).Value = $"Дата выгрузки: {DateTime.Now:dd.MM.yyyy HH:mm} | Всего записей: {total}";
        worksheet.Range(2, 1, 2, 9).Merge();
        worksheet.Cell(2, 1).Style.Font.Italic = true;
        worksheet.Cell(2, 1).Style.Font.FontSize = 10;
        worksheet.Cell(2, 1).Style.Font.FontColor = XLColor.Gray;

        var headers = new[]
        {
            "№",
            "Номер заявки",
            "Дата создания",
            "ФИО Заемщика",
            "ИНН",
            "Продукт",
            "Сумма (₽)",
            "Срок (мес.)",
            "Ставка (%)",
            "Статус",
            "Документы"
        };

        const int headerRow = 4;
        for (var i = 0; i < headers.Length; i++)
        {
            var cell = worksheet.Cell(headerRow, i + 1);
            cell.Value = headers[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#1F4E79");
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = XLColor.DarkGray;
        }

        var row = headerRow + 1;
        var seq = 1;

        foreach (var item in items)
        {
            worksheet.Cell(row, 1).Value = seq++;
            worksheet.Cell(row, 2).Value = item.ApplicationNumber;
            worksheet.Cell(row, 3).Value = item.CreatedAt.ToString("dd.MM.yyyy HH:mm");
            worksheet.Cell(row, 4).Value = item.ClientFullName;
            worksheet.Cell(row, 5).Value = item.ClientTaxNumber;
            worksheet.Cell(row, 6).Value = GetProductTitle(item.ProductType);

            var amountCell = worksheet.Cell(row, 7);
            amountCell.Value = item.LoanAmount;
            amountCell.Style.NumberFormat.Format = "#,##0.00 ₽";

            worksheet.Cell(row, 8).Value = item.LoanTermMonths;

            var rateCell = worksheet.Cell(row, 9);
            rateCell.Value = item.InterestRate;
            rateCell.Style.NumberFormat.Format = "0.00'%'";

            var statusCell = worksheet.Cell(row, 10);
            statusCell.Value = item.StatusTitle;
            ApplyStatusStyle(statusCell, item.Status);

            worksheet.Cell(row, 11).Value = item.DocumentsCount;

            for (var c = 1; c <= 11; c++)
            {
                worksheet.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                worksheet.Cell(row, c).Style.Border.OutsideBorderColor = XLColor.LightGray;
            }

            row++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static void ApplyStatusStyle(IXLCell cell, ApplicationStatus status)
    {
        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        cell.Style.Font.Bold = true;

        switch (status)
        {
            case ApplicationStatus.New:
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E3F2FD");
                cell.Style.Font.FontColor = XLColor.FromHtml("#0D47A1");
                break;
            case ApplicationStatus.InProgress:
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFF9C4");
                cell.Style.Font.FontColor = XLColor.FromHtml("#F57F17");
                break;
            case ApplicationStatus.Approved:
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#E8F5E9");
                cell.Style.Font.FontColor = XLColor.FromHtml("#1B5E20");
                break;
            case ApplicationStatus.Rejected:
                cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#FFEBEE");
                cell.Style.Font.FontColor = XLColor.FromHtml("#B71C1C");
                break;
        }
    }

    private static string GetProductTitle(ProductType type) => type switch
    {
        ProductType.ConsumerLoan => "Потребительский",
        ProductType.Mortgage => "Ипотека",
        ProductType.AutoLoan => "Автокредит",
        ProductType.BusinessLoan => "Бизнес-кредит",
        _ => type.ToString()
    };
}
