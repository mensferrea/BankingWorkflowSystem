using BankingWorkflow.Domain.Entities;
using BankingWorkflow.Domain.Enums;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Xunit;

namespace BankingWorkflow.Tests.Infrastructure;

public class DocumentExportTests
{
    [Fact]
    public void ClosedXml_CanGenerateWorkbookWithData()
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Тестовый реестр");

        worksheet.Cell(1, 1).Value = "Номер заявки";
        worksheet.Cell(1, 2).Value = "Сумма";
        worksheet.Cell(2, 1).Value = "APP-20261001-0001";
        worksheet.Cell(2, 2).Value = 500_000m;

        using var memoryStream = new MemoryStream();
        workbook.SaveAs(memoryStream);

        var bytes = memoryStream.ToArray();
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public void OpenXml_CanGenerateWordprocessingDocument()
    {
        using var memoryStream = new MemoryStream();
        using (var wordDoc = WordprocessingDocument.Create(memoryStream, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
        {
            var mainPart = wordDoc.AddMainDocumentPart();
            mainPart.Document = new Document();
            var body = mainPart.Document.AppendChild(new Body());

            var p = new Paragraph(new Run(new Text("КРЕДИТНЫЙ ДОГОВОР")));
            body.AppendChild(p);

            mainPart.Document.Save();
        }

        var bytes = memoryStream.ToArray();
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }
}
