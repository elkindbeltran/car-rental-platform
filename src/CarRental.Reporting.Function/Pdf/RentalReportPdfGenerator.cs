using System.Globalization;
using PdfSharp.Drawing;
using PdfSharp.Fonts;
using PdfSharp.Pdf;

namespace CarRental.Reporting.Worker.Pdf;

public static class RentalReportPdfGenerator
{
    private static readonly object FontSync = new();
    private static bool _fontConfigured;

    public static byte[] Generate(RentalReportDocument report)
    {
        ArgumentNullException.ThrowIfNull(report);
        EnsureFontResolver();

        using var document = new PdfDocument();
        document.Info.Title = $"{report.ReportType} - {report.PeriodStartUtc:yyyy-MM-dd}";
        document.Info.Author = "Car Rental Platform";
        document.Info.Subject = "Rental activity report";

        var pageNumber = 0;
        PdfPage page = null!;
        XGraphics graphics = null!;
        var y = 0d;

        void StartPage()
        {
            graphics?.Dispose();
            page = document.AddPage();
            page.Size = PdfSharp.PageSize.A4;
            graphics = XGraphics.FromPdfPage(page);
            pageNumber++;
            y = DrawHeader(graphics, page, report, pageNumber);
            DrawFooter(graphics, page, report.ReportId);
            DrawTableHeader(graphics, y);
            y += 24;
        }

        StartPage();
        foreach (var rental in report.Rentals)
        {
            if (y > page.Height.Point - 64)
            {
                StartPage();
            }

            DrawRow(graphics, y, rental, ((int)(y / 24)) % 2 == 0);
            y += 24;
        }

        if (report.Rentals.Count == 0)
        {
            var emptyFont = new XFont("Bitstream Vera", 10);
            graphics.DrawString(
                "No bookings were created during this reporting period.",
                emptyFont,
                XBrushes.DimGray,
                new XRect(42, y + 18, page.Width.Point - 84, 30),
                XStringFormats.TopLeft);
        }

        graphics.Dispose();
        using var output = new MemoryStream();
        document.Save(output, false);
        return output.ToArray();
    }

    private static double DrawHeader(
        XGraphics graphics,
        PdfPage page,
        RentalReportDocument report,
        int pageNumber)
    {
        var titleFont = new XFont("Bitstream Vera", 20, XFontStyleEx.Bold);
        var bodyFont = new XFont("Bitstream Vera", 8.5);
        var accent = XColor.FromArgb(24, 61, 93);
        graphics.DrawRectangle(new XSolidBrush(accent), 0, 0, page.Width.Point, 96);
        graphics.DrawString("CAR RENTAL", titleFont, XBrushes.White, new XPoint(42, 40));
        graphics.DrawString(report.ReportType, bodyFont, XBrushes.White, new XPoint(43, 61));
        graphics.DrawString(
            string.Create(CultureInfo.InvariantCulture, $"Period: {report.PeriodStartUtc:yyyy-MM-dd} to {report.PeriodEndUtc:yyyy-MM-dd}"),
            bodyFont,
            XBrushes.White,
            new XPoint(43, 78));
        graphics.DrawString($"Page {pageNumber}", bodyFont, XBrushes.White, new XPoint(page.Width.Point - 82, 78));

        var metricFont = new XFont("Bitstream Vera", 11, XFontStyleEx.Bold);
        var total = report.Rentals.Sum(rental => rental.TotalAmount);
        graphics.DrawString($"Bookings  {report.Rentals.Count}", metricFont, XBrushes.Black, new XPoint(42, 126));
        graphics.DrawString(
            string.Create(CultureInfo.InvariantCulture, $"Total value  {total:F2}"),
            metricFont,
            XBrushes.Black,
            new XPoint(210, 126));
        graphics.DrawString($"Generated  {report.GeneratedAtUtc:u}", bodyFont, XBrushes.DimGray, new XPoint(42, 146));
        return 172;
    }

    private static void DrawTableHeader(XGraphics graphics, double y)
    {
        var font = new XFont("Bitstream Vera", 8, XFontStyleEx.Bold);
        graphics.DrawRectangle(new XSolidBrush(XColor.FromArgb(224, 232, 239)), 36, y, 523, 24);
        DrawCell(graphics, font, "Booking", 42, y, 75);
        DrawCell(graphics, font, "Customer", 117, y, 75);
        DrawCell(graphics, font, "Vehicle", 192, y, 75);
        DrawCell(graphics, font, "Pickup", 267, y, 88);
        DrawCell(graphics, font, "Return", 355, y, 88);
        DrawCell(graphics, font, "Amount", 443, y, 70);
        DrawCell(graphics, font, "Status", 513, y, 44);
    }

    private static void DrawRow(XGraphics graphics, double y, Persistence.RentalReportRow rental, bool shaded)
    {
        var font = new XFont("Bitstream Vera", 7.2);
        if (shaded)
        {
            graphics.DrawRectangle(new XSolidBrush(XColor.FromArgb(247, 249, 251)), 36, y, 523, 24);
        }

        DrawCell(graphics, font, ShortId(rental.BookingId), 42, y, 75);
        DrawCell(graphics, font, ShortId(rental.CustomerId), 117, y, 75);
        DrawCell(graphics, font, ShortId(rental.VehicleId), 192, y, 75);
        DrawCell(graphics, font, rental.PickupAtUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), 267, y, 88);
        DrawCell(graphics, font, rental.ReturnAtUtc.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture), 355, y, 88);
        DrawCell(
            graphics,
            font,
            string.Create(CultureInfo.InvariantCulture, $"{rental.TotalAmount:F2} {rental.Currency}"),
            443,
            y,
            70);
        DrawCell(graphics, font, rental.Status, 513, y, 44);
    }

    private static void DrawCell(XGraphics graphics, XFont font, string value, double x, double y, double width) =>
        graphics.DrawString(value, font, XBrushes.Black, new XRect(x, y + 7, width - 4, 14), XStringFormats.TopLeft);

    private static string ShortId(Guid id) => id.ToString("N")[..8].ToUpperInvariant();

    private static void DrawFooter(XGraphics graphics, PdfPage page, Guid reportId)
    {
        var font = new XFont("Bitstream Vera", 7);
        var y = page.Height.Point - 28;
        graphics.DrawLine(new XPen(XColor.FromArgb(210, 218, 225)), 36, y - 8, page.Width.Point - 36, y - 8);
        graphics.DrawString(
            $"Report {reportId:N}  |  Private document",
            font,
            XBrushes.DimGray,
            new XRect(36, y, page.Width.Point - 72, 12),
            XStringFormats.TopLeft);
    }

    private static void EnsureFontResolver()
    {
        if (_fontConfigured)
        {
            return;
        }

        lock (FontSync)
        {
            if (!_fontConfigured)
            {
                GlobalFontSettings.FontResolver = new EmbeddedFontResolver();
                _fontConfigured = true;
            }
        }
    }
}
