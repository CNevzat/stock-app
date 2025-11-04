using Markdig;
using PuppeteerSharp;
using StockApp.App.Product.Query;

namespace StockApp.Services;

public interface IPdfService
{
    Task<byte[]> GenerateCriticalStockPdf(List<ProductDto> products);
}

public class PdfService : IPdfService
{
    private readonly IMarkdownService _markdownService;

    public PdfService(IMarkdownService markdownService)
    {
        _markdownService = markdownService;
    }

    public async Task<byte[]> GenerateCriticalStockPdf(List<ProductDto> products)
    {
        // Generate Markdown report
        var markdown = _markdownService.GenerateCriticalStockReport(products);
        
        // Convert Markdown to HTML using Markdig
        var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
        var html = Markdown.ToHtml(markdown, pipeline);
        
        // Create styled HTML with CSS
        var styledHtml = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <style>
        body {{
            font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
            font-size: 12pt;
            line-height: 1.6;
            color: #333;
            margin: 20px;
            padding: 20px;
        }}
        h1 {{
            color: #2c3e50;
            border-bottom: 3px solid #3498db;
            padding-bottom: 10px;
            margin-bottom: 20px;
        }}
        h2 {{
            color: #34495e;
            margin-top: 30px;
            margin-bottom: 15px;
        }}
        table {{
            width: 100%;
            border-collapse: collapse;
            margin: 20px 0;
            font-size: 11pt;
        }}
        th {{
            background-color: #3498db;
            color: white;
            padding: 12px;
            text-align: left;
            font-weight: bold;
        }}
        td {{
            padding: 10px;
            border-bottom: 1px solid #ddd;
        }}
        tr:nth-child(even) {{
            background-color: #f9f9f9;
        }}
        strong {{
            color: #e74c3c;
            font-weight: bold;
        }}
        hr {{
            border: none;
            border-top: 2px solid #ecf0f1;
            margin: 20px 0;
        }}
        blockquote {{
            border-left: 4px solid #3498db;
            padding-left: 20px;
            margin: 20px 0;
            color: #7f8c8d;
            font-style: italic;
        }}
        ul {{
            margin: 10px 0;
            padding-left: 25px;
        }}
        li {{
            margin: 5px 0;
        }}
        @page {{
            margin: 2cm;
        }}
        @media print {{
            body {{
                margin: 0;
                padding: 15px;
            }}
        }}
    </style>
</head>
<body>
    {html}
</body>
</html>";

        // Download Chromium if needed (only first time)
        await new BrowserFetcher().DownloadAsync();

        // Launch browser
        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" }
        });

        // Create page
        using var page = await browser.NewPageAsync();
        
        // Set content
        await page.SetContentAsync(styledHtml, new NavigationOptions
        {
            WaitUntil = new[] { WaitUntilNavigation.Networkidle0 }
        });

        // Generate PDF
        var pdfBytes = await page.PdfDataAsync(new PdfOptions
        {
            Format = PuppeteerSharp.Media.PaperFormat.A4,
            MarginOptions = new PuppeteerSharp.Media.MarginOptions
            {
                Top = "20mm",
                Bottom = "20mm",
                Left = "15mm",
                Right = "15mm"
            },
            PrintBackground = true
        });

        return pdfBytes ?? Array.Empty<byte>();
    }
}
