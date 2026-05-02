using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;

namespace AgentNovel.Services;

public class ThumbnailService
{
    private readonly int _thumbnailWidth = 200;

    public async Task<Bitmap?> GenerateThumbnailAsync(string pdfPath, int pageNumber)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var document = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
                if (pageNumber < 1 || pageNumber > document.PageCount)
                    return null;

                var page = document.Pages[pageNumber - 1];
                return RenderPageToBitmap(page);
            }
            catch
            {
                return null;
            }
        });
    }

    public async Task<Bitmap?> GenerateThumbnailAsync(PdfPage page)
    {
        return await Task.Run(() => RenderPageToBitmap(page));
    }

    private Bitmap? RenderPageToBitmap(PdfPage page)
    {
        try
        {
            var width = _thumbnailWidth;
            var height = (int)(width * (page.Height / page.Width));

            using var stream = new MemoryStream();
            
            // PDFsharp不直接支持渲染，这里创建占位图
            // 实际项目中需要使用PdfiumViewer或其他渲染库
            var pixels = new byte[width * height * 4];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = 255;     // B
                pixels[i + 1] = 255; // G
                pixels[i + 2] = 255; // R
                pixels[i + 3] = 255; // A
            }

            return new Bitmap(PixelFormat.Bgra8888, AlphaFormat.Premul, pixels, new PixelSize(width, height), new Vector(96, 96), width * 4);
        }
        catch
        {
            return null;
        }
    }
}
