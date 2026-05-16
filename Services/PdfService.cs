using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AgentNovel.Models;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using PdfPageModel = AgentNovel.Models.PdfPage;

namespace AgentNovel.Services;

public class PdfService
{
    public async Task<PdfFile> LoadPdfAsync(string filePath)
    {
        return await Task.Run(() =>
        {
            var document = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
            var fileInfo = new FileInfo(filePath);
            
            var pdfFile = new PdfFile
            {
                FileName = Path.GetFileName(filePath),
                FilePath = filePath,
                PageCount = document.PageCount,
                FileSize = fileInfo.Length
            };

            for (int i = 0; i < document.PageCount; i++)
            {
                pdfFile.Pages.Add(new PdfPageModel
                {
                    PageNumber = i + 1,
                    Rotation = (int)document.Pages[i].Rotate
                });
            }

            document.Close();
            return pdfFile;
        });
    }

    public async Task MergePdfsAsync(List<string> filePaths, string outputPath, IProgress<int>? progress = null)
    {
        await Task.Run(() =>
        {
            using var outputDocument = new PdfDocument();
            int totalFiles = filePaths.Count;
            int processedFiles = 0;

            foreach (var filePath in filePaths)
            {
                using var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
                foreach (var page in inputDocument.Pages)
                {
                    outputDocument.AddPage(page);
                }
                processedFiles++;
                progress?.Report((processedFiles * 100) / totalFiles);
            }

            outputDocument.Save(outputPath);
        });
    }

    public async Task SplitPdfAsync(string filePath, List<int> pageNumbers, string outputPath)
    {
        await Task.Run(() =>
        {
            using var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
            using var outputDocument = new PdfDocument();

            foreach (var pageNum in pageNumbers.OrderBy(p => p))
            {
                if (pageNum >= 1 && pageNum <= inputDocument.PageCount)
                {
                    outputDocument.AddPage(inputDocument.Pages[pageNum - 1]);
                }
            }

            outputDocument.Save(outputPath);
        });
    }

    public async Task ReorderPagesAsync(string filePath, List<int> newOrder, string outputPath)
    {
        await Task.Run(() =>
        {
            using var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
            using var outputDocument = new PdfDocument();

            foreach (var pageNum in newOrder)
            {
                if (pageNum >= 1 && pageNum <= inputDocument.PageCount)
                {
                    outputDocument.AddPage(inputDocument.Pages[pageNum - 1]);
                }
            }

            outputDocument.Save(outputPath);
        });
    }

    public async Task RotatePagesAsync(string filePath, List<int> pageNumbers, int rotation, string outputPath)
    {
        await Task.Run(() =>
        {
            using var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
            
            foreach (var pageNum in pageNumbers)
            {
                if (pageNum >= 1 && pageNum <= inputDocument.PageCount)
                {
                    var page = inputDocument.Pages[pageNum - 1];
                    page.Rotate = (int)page.Rotate + rotation;
                }
            }

            inputDocument.Save(outputPath);
        });
    }

    public async Task SavePagesAsync(string filePath, List<PdfPageModel> pages, string outputPath)
    {
        await Task.Run(() =>
        {
            using var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
            using var outputDocument = new PdfDocument();

            foreach (var pageModel in pages)
            {
                if (pageModel.PageNumber >= 1 && pageModel.PageNumber <= inputDocument.PageCount)
                {
                    var page = outputDocument.AddPage(inputDocument.Pages[pageModel.PageNumber - 1]);
                    page.Rotate = pageModel.Rotation;
                }
            }

            outputDocument.Save(outputPath);
        });
    }

    public async Task DeletePagesAsync(string filePath, List<int> pageNumbers, string outputPath)
    {
        await Task.Run(() =>
        {
            using var inputDocument = PdfReader.Open(filePath, PdfDocumentOpenMode.Import);
            var pagesToKeep = Enumerable.Range(1, inputDocument.PageCount)
                .Where(p => !pageNumbers.Contains(p))
                .ToList();

            using var outputDocument = new PdfDocument();
            foreach (var pageNum in pagesToKeep)
            {
                outputDocument.AddPage(inputDocument.Pages[pageNum - 1]);
            }

            outputDocument.Save(outputPath);
        });
    }
}
