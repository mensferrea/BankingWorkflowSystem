using BankingWorkflow.Application.Interfaces;
using BankingWorkflow.Domain.Entities;
using BankingWorkflow.Domain.Exceptions;
using BankingWorkflow.Infrastructure.Data;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using AppEntity = BankingWorkflow.Domain.Entities.Application;

namespace BankingWorkflow.Infrastructure.Services;

public class WordDocumentGenerationService : IDocumentGenerationService
{
    private readonly BankingDbContext _db;

    public WordDocumentGenerationService(BankingDbContext db)
    {
        _db = db;
    }

    public async Task<byte[]> GenerateLoanAgreementDocxAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        var app = await _db.Applications
            .AsNoTracking()
            .Include(a => a.Client)
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        if (app == null)
        {
            throw new DomainException($"Заявка с ID '{applicationId}' не найдена.");
        }

        var client = app.Client ?? throw new DomainException("Данные заемщика отсутствуют.");

        using var memoryStream = new MemoryStream();
        using (var wordDoc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            AddHeading(body, $"КРЕДИТНЫЙ ДОГОВОР № {app.ApplicationNumber}", 28, JustificationValues.Center);
            AddParagraph(body, $"г. Москва, {DateTime.Now:dd MMMM yyyy} г.", JustificationValues.Center, isItalic: true);
            AddEmptyLine(body);

            var preamble = $"ПАО «ФИНАНСОВЫЙ СТАНДАРТ», именуемое в дальнейшем «Банк», в лице Уполномоченного представителя, с одной стороны, и гражданин(ка) {client.FullName}, паспорт серии {client.PassportSeries} № {client.PassportNumber}, ИНН {client.TaxNumber}, проживающий(ая) по адресу: {client.RegistrationAddress}, именуемый(ая) в дальнейшем «Заемщик», с другой стороны, заключили настоящий Договор о нижеследующем:";
            AddParagraph(body, preamble, JustificationValues.Both);
            AddEmptyLine(body);

            AddHeading(body, "1. ПРЕДМЕТ ДОГОВОРА И ОСНОВНЫЕ УСЛОВИЯ", 22, JustificationValues.Left);

            var table = new Table();
            AddTableHeader(table, "Параметр", "Значение");
            AddTableRow(table, "Номер договора / заявки", app.ApplicationNumber);
            AddTableRow(table, "Продукт", app.ProductType.ToString());
            AddTableRow(table, "Сумма кредита", $"{app.LoanAmount:N2} рублей");
            AddTableRow(table, "Срок кредитования", $"{app.LoanTermMonths} месяцев");
            AddTableRow(table, "Процентная ставка", $"{app.InterestRate:N2}% годовых");
            AddTableRow(table, "Ежемесячный аннуитетный платеж", $"{app.MonthlyPayment:N2} рублей");
            AddTableRow(table, "Общая сумма к выплате", $"{app.TotalPayment:N2} рублей");
            AddTableRow(table, "Переплата за весь срок", $"{app.TotalOverpayment:N2} рублей");
            body.AppendChild(table);

            AddEmptyLine(body);
            AddHeading(body, "2. ПРАВА И ОБЯЗАННОСТИ СТОРОН", 22, JustificationValues.Left);
            AddParagraph(body, "2.1. Банк обязуется предоставить Заемщику денежные средства в размере и на условиях, предусмотренных настоящим Договором, путем зачисления на счет Заемщика.", JustificationValues.Both);
            AddParagraph(body, "2.2. Заемщик обязуется возвратить полученный кредит и уплатить проценты за пользование кредитом в порядке и в сроки, установленные Графиком платежей.", JustificationValues.Both);
            AddParagraph(body, "2.3. Досрочное погашение кредита осуществляется без взимания комиссий при условии предварительного уведомления Банка.", JustificationValues.Both);

            AddEmptyLine(body);
            AddHeading(body, "3. АДРЕСА, РЕКВИЗИТЫ И ПОДПИСИ СТОРОН", 22, JustificationValues.Left);

            var sigTable = new Table();
            var row = new TableRow();

            var bankCell = new TableCell();
            bankCell.Append(new Paragraph(new Run(new Text("ОТ БАНКА:") { Space = SpaceProcessingModeValues.Preserve }) { RunProperties = new RunProperties(new Bold()) }));
            bankCell.Append(new Paragraph(new Run(new Text("ПАО «ФИНАНСОВЫЙ СТАНДАРТ»"))));
            bankCell.Append(new Paragraph(new Run(new Text("БИК: 044525000 / К/с: 30101810400000000225"))));
            bankCell.Append(new Paragraph(new Run(new Text("\n\nПодпись: _________________ (М.П.)"))));

            var clientCell = new TableCell();
            clientCell.Append(new Paragraph(new Run(new Text("ЗАЕМЩИК:") { Space = SpaceProcessingModeValues.Preserve }) { RunProperties = new RunProperties(new Bold()) }));
            clientCell.Append(new Paragraph(new Run(new Text(client.FullName))));
            clientCell.Append(new Paragraph(new Run(new Text($"Паспорт: {client.PassportSeries} {client.PassportNumber}"))));
            clientCell.Append(new Paragraph(new Run(new Text($"Тел: {client.PhoneNumber}"))));
            clientCell.Append(new Paragraph(new Run(new Text("\n\nПодпись: _________________"))));

            row.Append(bankCell);
            row.Append(clientCell);
            sigTable.Append(row);
            body.AppendChild(sigTable);

            mainPart.Document.Save();
        }

        return memoryStream.ToArray();
    }

    public async Task<byte[]> GenerateDecisionCertificateDocxAsync(Guid applicationId, CancellationToken cancellationToken = default)
    {
        var app = await _db.Applications
            .AsNoTracking()
            .Include(a => a.Client)
            .FirstOrDefaultAsync(a => a.Id == applicationId, cancellationToken);

        if (app == null)
        {
            throw new DomainException($"Заявка с ID '{applicationId}' не найдена.");
        }

        var client = app.Client ?? throw new DomainException("Данные заемщика отсутствуют.");

        using var memoryStream = new MemoryStream();
        using (var wordDoc = WordprocessingDocument.Create(memoryStream, WordprocessingDocumentType.Document, true))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            AddHeading(body, "ВЫПИСКА ИЗ РЕШЕНИЯ КРЕДИТНОГО КОМИТЕТА", 26, JustificationValues.Center);
            AddParagraph(body, $"к протоколу рассмотрения заявки № {app.ApplicationNumber}", JustificationValues.Center, isItalic: true);
            AddEmptyLine(body);

            AddParagraph(body, $"Дата рассмотрения: {app.UpdatedAt:dd.MM.yyyy HH:mm}", JustificationValues.Left);
            AddParagraph(body, $"Заемщик: {client.FullName} (ИНН {client.TaxNumber})", JustificationValues.Left);
            AddParagraph(body, $"Кредитный рейтинг заемщика: {client.CreditRating}", JustificationValues.Left);
            AddEmptyLine(body);

            var decisionText = app.Status switch
            {
                Domain.Enums.ApplicationStatus.Approved => "СТАТУС: ЗАЯВКА ОДОБРЕНА",
                Domain.Enums.ApplicationStatus.Rejected => "СТАТУС: ЗАЯВКА ОТКЛОНЕНА",
                _ => $"СТАТУС: {AppEntity.GetStatusTitle(app.Status).ToUpperInvariant()}"
            };

            AddHeading(body, decisionText, 24, JustificationValues.Center);
            AddEmptyLine(body);

            var table = new Table();
            AddTableHeader(table, "Параметр", "Значение");
            AddTableRow(table, "Запрашиваемая сумма", $"{app.LoanAmount:N2} ₽");
            AddTableRow(table, "Срок", $"{app.LoanTermMonths} мес.");
            AddTableRow(table, "Утвержденная ставка", $"{app.InterestRate:N2}%");
            AddTableRow(table, "Ежемесячный платеж", $"{app.MonthlyPayment:N2} ₽");
            AddTableRow(table, "Комментарий андеррайтера", app.StatusComment ?? "Без комментариев");
            body.AppendChild(table);

            AddEmptyLine(body);
            AddParagraph(body, $"Решение зафиксировано в автоматизированной банковской системе пользователем: {app.CreatedByUserName}", JustificationValues.Left);
            AddParagraph(body, "Подпись ответственного лица: _______________________ (Кредитный отдел)", JustificationValues.Left);

            mainPart.Document.Save();
        }

        return memoryStream.ToArray();
    }

    private static void AddHeading(Body body, string text, int fontSizeHalfPt, JustificationValues alignment)
    {
        var run = new Run(new Text(text));
        run.RunProperties = new RunProperties(
            new Bold(),
            new FontSize { Val = fontSizeHalfPt.ToString() },
            new Color { Val = "1F4E79" });

        var paragraph = new Paragraph(run);
        paragraph.ParagraphProperties = new ParagraphProperties(new Justification { Val = alignment });
        body.AppendChild(paragraph);
    }

    private static void AddParagraph(Body body, string text, JustificationValues alignment, bool isItalic = false)
    {
        var runProps = new RunProperties(new FontSize { Val = "22" });
        if (isItalic) runProps.Append(new Italic());

        var run = new Run(new Text(text)) { RunProperties = runProps };
        var paragraph = new Paragraph(run);
        paragraph.ParagraphProperties = new ParagraphProperties(new Justification { Val = alignment });
        body.AppendChild(paragraph);
    }

    private static void AddEmptyLine(Body body)
    {
        body.AppendChild(new Paragraph());
    }

    private static void AddTableHeader(Table table, string col1, string col2)
    {
        var row = new TableRow();
        row.Append(CreateCell(col1, isBold: true, isHeader: true));
        row.Append(CreateCell(col2, isBold: true, isHeader: true));
        table.Append(row);
    }

    private static void AddTableRow(Table table, string col1, string col2)
    {
        var row = new TableRow();
        row.Append(CreateCell(col1, isBold: false, isHeader: false));
        row.Append(CreateCell(col2, isBold: false, isHeader: false));
        table.Append(row);
    }

    private static TableCell CreateCell(string text, bool isBold, bool isHeader)
    {
        var cell = new TableCell();
        var runProps = new RunProperties(new FontSize { Val = "20" });
        if (isBold) runProps.Append(new Bold());
        if (isHeader) runProps.Append(new Color { Val = "FFFFFF" });

        var run = new Run(new Text(text)) { RunProperties = runProps };
        var p = new Paragraph(run);

        var cellProps = new TableCellProperties();
        if (isHeader)
        {
            cellProps.Append(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "1F4E79" });
        }
        else
        {
            cellProps.Append(new Shading { Val = ShadingPatternValues.Clear, Color = "auto", Fill = "F2F2F2" });
        }

        cell.Append(cellProps);
        cell.Append(p);
        return cell;
    }
}
