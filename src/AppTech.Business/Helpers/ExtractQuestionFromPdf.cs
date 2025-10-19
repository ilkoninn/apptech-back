using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppTech.Core.Entities;
using AppTech.Core.Enums;
using AppTech.Business.DTOs.QuestionDTOs;

namespace AppTech.Business.Helpers
{
    public static class ExtractQuestionFromPdf
    {
        public static async Task<List<GetAllQuestionsFromPdfDTO>> ExtractQuestionsFromFileAsync(IFormFile pdfFile)
        {
            string pdfText = await ExtractTextFromPdfAsync(pdfFile);
            return await ProcessPdfContentAsync(pdfText);
        }

        private static async Task<string> ExtractTextFromPdfAsync(IFormFile pdfFile)
        {
            using (var memoryStream = new MemoryStream())
            {
                await pdfFile.CopyToAsync(memoryStream);
                memoryStream.Position = 0;

                using (PdfReader reader = new PdfReader(memoryStream))
                using (PdfDocument pdfDoc = new PdfDocument(reader))
                {
                    StringBuilder pdfText = new StringBuilder();

                    for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                    {
                        var page = pdfDoc.GetPage(i);

                        var strategy = new SimpleTextExtractionStrategy();
                        string extractedText = PdfTextExtractor.GetTextFromPage(page, strategy);

                        extractedText = FixTextSpacing(extractedText);

                        pdfText.Append(extractedText);
                        pdfText.Append("\n");
                    }

                    return pdfText.ToString();
                }
            }
        }

        public static string FixTextSpacing(string text)
        {

            text = System.Text.RegularExpressions.Regex.Replace(text, @"(?<=[a-z])(?=[A-Z])", " ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"([.,;!?])(?=[a-zA-Z])", "$1 ");
            // Handling other specific patterns as needed...

            return text;
        }

        public static async Task<List<GetAllQuestionsFromPdfDTO>> ProcessPdfContentAsync(string pdfText)
        {
            List<GetAllQuestionsFromPdfDTO> questions = new List<GetAllQuestionsFromPdfDTO>();

            var questionBlocks = pdfText.Split(new[] { "Question:", "Question ", "QUESTION " }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var block in questionBlocks)
            {
                var lines = block.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                 .Select(line => line.Trim())
                                 .Where(line => !string.IsNullOrEmpty(line))
                                 .ToArray();

                string questionLine = string.Join(" ", lines.TakeWhile(line => !line.StartsWith("A.") && !line.StartsWith("B.") && !line.StartsWith("C.") && !line.StartsWith("D.")).ToArray()).Trim();

                // Remove leading numbers from questionLine
                questionLine = System.Text.RegularExpressions.Regex.Replace(questionLine, @"^\d+\s*", "");

                if (!string.IsNullOrEmpty(questionLine))
                {
                    List<VariantDTOForPdf> variants = new List<VariantDTOForPdf>();
                    for (int i = 1; i < lines.Length; i++)
                    {
                        if (lines[i].Length > 2 && char.IsLetter(lines[i][0]) && lines[i][1] == '.')
                        {
                            variants.Add(new VariantDTOForPdf
                            {
                                Key = lines[i][0].ToString().ToUpper(),
                                Text = lines[i].Substring(3).Trim()
                            });
                        }
                    }

                    string answerLine = lines.FirstOrDefault(line => line.Contains("Answer:") || line.Contains("Correct Answer:"));
                    List<string> correctAnswers = new List<string>(); // correctAnswers listesini başlatıyoruz

                    if (!string.IsNullOrEmpty(answerLine))
                    {
                        correctAnswers = answerLine.Substring(answerLine.IndexOf("Answer:") + "Answer:".Length)
                            .Trim()
                            .ToUpper()
                            .Select(c => c.ToString()) // Her bir karakteri string'e çevir
                            .Where(c => char.IsLetter(c[0])) // Sadece harf olanları al
                            .ToList();
                    }


                    // Her bir variant için doğru olup olmadığını kontrol edin
                    foreach (var variant in variants)
                    {
                        variant.IsCorrect = correctAnswers.Contains(variant.Key, StringComparer.OrdinalIgnoreCase);
                    }

                    // Soru tipini belirleyin
                    string typeLine = correctAnswers.Count == 1 ? "SingleChoice" : "MultipleChoice";
                    if (variants.Any())
                    {
                        questions.Add(new GetAllQuestionsFromPdfDTO
                        {
                            Type = Enum.Parse<EQuestionType>(typeLine, true),
                            Content = questionLine,
                            Point = "1",
                            Variants = variants
                        });
                    }
                }
            }

            return questions;
        }


    }
}
