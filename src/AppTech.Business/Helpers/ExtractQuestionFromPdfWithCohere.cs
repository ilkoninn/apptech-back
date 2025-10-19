using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AppTech.Business.DTOs.QuestionDTOs;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace AppTech.Business.Helpers
{
    public partial class ExtractQuestionFromPdfWithCohere
    {
        public static string ExtractTextFromPdf(IFormFile pdf)
        {
            using (var stream = pdf.OpenReadStream())
            using (var reader = new PdfReader(stream))
            using (var pdfDoc = new PdfDocument(reader))
            {
                StringBuilder text = new StringBuilder();
                for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
                {
                    text.Append(PdfTextExtractor.GetTextFromPage(pdfDoc.GetPage(i)));
                }
                return text.ToString();
            }
        }

        public async static Task<string> AskCohereAsync(string prompt, string model)
        {
            var client = new RestClient(new RestClientOptions("https://api.cohere.ai/generate")
            {
                Timeout = TimeSpan.FromHours(3)
            });
            var request = new RestRequest();
            request.Method = Method.Post;
            request.AddHeader("Content-Type", "application/json");
            request.AddHeader("Authorization", "Bearer RryI2ry8jq23sIkdt1NQb8ScQ7JDXVLCAZn2fqE3");

            var body = new
            {
                prompt = prompt,
                model = model,
                temperature = 0.8,
            };
            request.AddJsonBody(body);
            var response = await client.ExecuteAsync(request);
            if (!response.IsSuccessful)
            {
                throw new Exception($"API response contains an error: {response.Content}");
            }
            return response.Content;
        }

        private static List<string> ExtractQuestionChunks(string text, int questionsPerChunk)
        {
            List<string> chunks = new List<string>();
            StringBuilder currentChunk = new StringBuilder();
            int questionCount = 0;

            var matches = MyRegex().Matches(text);

            foreach (Match match in matches)
            {
                string questionText = match.Value.Trim();

                if (questionText.Contains("SIMULATION", StringComparison.OrdinalIgnoreCase) ||
                    questionText.Contains("DRAG DROP", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                currentChunk.Append(match.Value.Trim() + "\n\n");
                questionCount++;

                if (questionCount == questionsPerChunk)
                {
                    chunks.Add(currentChunk.ToString());
                    currentChunk.Clear();
                    questionCount = 0;
                }
            }

            if (currentChunk.Length > 0)
            {
                chunks.Add(currentChunk.ToString());
            }

            return chunks;
        }

        public async static Task<List<GetAllQuestionsFromPdfDTO>> ProcessPdfInChunksAsync(IFormFile pdf, string model)
        {
            string extractedText = ExtractTextFromPdf(pdf);
            List<string> questionChunks = ExtractQuestionChunks(extractedText, 15);
            List<GetAllQuestionsFromPdfDTO> allQuestions = new List<GetAllQuestionsFromPdfDTO>();

            foreach (var chunk in questionChunks)
            {
                string prompt = $@"
                Below is a text snippet extracted from a PDF document. Extract all the questions and their options along with the correct answers, and format them in JSON:

                [
                    {{
                        ""Content"": ""Question text here"",
                        ""Variants"": [
                            {{""Key"": ""A"", ""Text"": ""Option A"", ""IsCorrect"": false}},
                            {{""Key"": ""B"", ""Text"": ""Option B"", ""IsCorrect"": true}}
                        ]
                    }}
                ]

                Text: {chunk}
                ";

                try
                {
                    string result = await AskCohereAsync(prompt, model);
                    var questions = ParseQuestions(result);
                    allQuestions.AddRange(questions);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error processing chunk: {ex.Message}");
                }
            }

            return allQuestions;
        }

        private static List<GetAllQuestionsFromPdfDTO> ParseQuestions(string jsonOutput)
        {
            var rootObject = JsonConvert.DeserializeObject<dynamic>(jsonOutput);
            string jsonText = rootObject.text;

            if (!IsValidJsonArray(jsonText))
            {
                jsonText = CleanCohereResponse(jsonText);
            }

            if (!IsValidJsonArray(jsonText))
            {
                return ExtractQuestionsFromText(jsonText);
            }

            var questions = JsonConvert.DeserializeObject<List<GetAllQuestionsFromPdfDTO>>(jsonText);

            if (questions == null || !questions.Any())
            {
                throw new Exception("Deserialization returned null or no questions. Please check the JSON format.");
            }

            return questions;
        }

        private static bool IsValidJsonArray(string json)
        {
            json = json.Trim();
            if ((json.StartsWith("[") && json.EndsWith("]")) || (json.StartsWith("{") && json.EndsWith("}")))
            {
                try
                {
                    JToken.Parse(json);
                    return true;
                }
                catch (JsonReaderException)
                {
                    return false;
                }
            }
            return false;
        }

        private static string CleanCohereResponse(string response)
        {
            var cleanedResponse = response.Replace("```json", "").Replace("```", "").Trim();

            var startIndex = cleanedResponse.IndexOf("[");
            var endIndex = cleanedResponse.LastIndexOf("]") + 1;

            if (startIndex == -1 || endIndex == -1)
            {
                throw new Exception("Failed to find valid JSON array in the response.");
            }

            var jsonContent = cleanedResponse.Substring(startIndex, endIndex - startIndex).Trim();
            jsonContent = jsonContent.Replace("\\n", "").Replace("\\t", "").Replace("\\r", "");

            return jsonContent;
        }

        private static List<GetAllQuestionsFromPdfDTO> ExtractQuestionsFromText(string text)
        {
            List<GetAllQuestionsFromPdfDTO> questions = new List<GetAllQuestionsFromPdfDTO>();

            // Adjusted regex pattern to handle flexible spacing, "Question" prefixes, and multiple answer choices
            string questionPattern = @"(?<content>Question\s*[:#]?\s*\d*\s*[:\s]*)?(?<content>.+?)\n(?:A\.\s(?<a>.+?)\nB\.\s(?<b>.+?))(?:\nC\.\s(?<c>.+?))?(?:\nD\.\s(?<d>.+?))?";

            var matches = Regex.Matches(text, questionPattern, RegexOptions.Multiline);

            foreach (Match match in matches)
            {
                var question = new GetAllQuestionsFromPdfDTO
                {
                    Content = match.Groups["content"].Value.Trim(),
                    Variants = new List<VariantDTOForPdf>()
                };

                if (match.Groups["a"].Success)
                    question.Variants.Add(new VariantDTOForPdf { Text = match.Groups["a"].Value.Trim(), IsCorrect = false });
                if (match.Groups["b"].Success)
                    question.Variants.Add(new VariantDTOForPdf { Text = match.Groups["b"].Value.Trim(), IsCorrect = false });
                if (match.Groups["c"].Success)
                    question.Variants.Add(new VariantDTOForPdf { Text = match.Groups["c"].Value.Trim(), IsCorrect = false });
                if (match.Groups["d"].Success)
                    question.Variants.Add(new VariantDTOForPdf { Text = match.Groups["d"].Value.Trim(), IsCorrect = false });

                questions.Add(question);
            }

            return questions;
        }
        public static void RemoveQuestionPrefix(List<GetAllQuestionsFromPdfDTO> questions)
        {
            string pattern = @"^\s*Question\s*[#:]?\s*\d+\s*";

            foreach (var question in questions)
            {
                question.Content = Regex.Replace(question.Content, pattern, "", RegexOptions.IgnoreCase).Trim();
            }
        }
        public static void RemoveInvalidPrefix(List<GetAllQuestionsFromPdfDTO> questions)
        {
            string pattern = @"^[^a-zA-Z]*";

            foreach (var question in questions)
            {
                question.Content = Regex.Replace(question.Content, pattern, "").Trim();
            }
        }

        public static int CalculateTime(string extractedTest)
        {
            var newText = extractedTest.Replace("\n", "");

            var questionMatches = MyRegex().Matches(newText);

            int totalQuestions = questionMatches.Count;

            if (totalQuestions == 0)
            {
                return 0;
            }

            int questionsPerChunk = 15;
            int chunkCount = (int)Math.Ceiling((double)totalQuestions / questionsPerChunk);
            int estimatedTimeInSeconds = chunkCount * 85;

            return estimatedTimeInSeconds;
        }

        [GeneratedRegex(@"(question\s*[#:.]?\s*\d+\s*[\s\S]*?)(?=(question\s*[#:.]?\s*\d+|$))", RegexOptions.IgnoreCase | RegexOptions.Singleline, "en-US")]
        private static partial Regex MyRegex();
    }
}
