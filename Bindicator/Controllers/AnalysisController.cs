using Bindicator.Data;
using Bindicator.Services;
using Bindicator.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace Bindicator.Controllers
{
    /// <summary>
    /// Controller for displaying historical analysis and bin performance insights.
    /// </summary>
    public class AnalysisController : Controller
    {
        private readonly DbSeeder _dbSeeder;
        private readonly ApplicationDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="AnalysisController"/> class.
        /// </summary>
        /// <param name="context">The application's database context.</param>
        /// <param name="dbSeeder">The database seeder service.</param>
        public AnalysisController(ApplicationDbContext context, DbSeeder dbSeeder)
        {
            _context = context;
            _dbSeeder = dbSeeder;
        }

        /// <summary>
        /// Loads the analysis page.
        /// </summary>
        /// <returns>The analysis page view.</returns>
        public async Task<IActionResult> Index()
        {
            await Task.CompletedTask;
            return View();
        }

        /// <summary>
        /// Seeds analysis data for machine learning experimentation.
        /// </summary>
        /// <returns>A redirection to the <see cref="DownloadCsv"/> action.</returns>
        [HttpPost]
        public async Task<IActionResult> SeedAnalysisData()
        {
            await DbSeeder.SeedAnalysisDataAsync(_context);
            return RedirectToAction(nameof(DownloadCsv));
        }

        /// <summary>
        /// Downloads the analysis data as a CSV file.
        /// </summary>
        /// <returns>A CSV file containing the analysis data.</returns>
        [HttpGet]
        public async Task<IActionResult> DownloadCsv()
        {
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Postcode,Street,BinNumber,FillLevel,Weight,Timestamp");

            var records = await _context.SensorAnalysisReadings
                .OrderBy(r => r.Postcode)
                .ThenBy(r => r.Street)
                .ThenBy(r => r.BinNumber)
                .ThenBy(r => r.Timestamp)
                .ToListAsync();

            foreach (var record in records)
            {
                csvBuilder.AppendLine($"{record.Postcode},{record.Street},{record.BinNumber},{record.FillLevel},{record.Weight},{record.Timestamp:O}");
            }

            var csvBytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
            return File(csvBytes, "text/csv", "analysis_data.csv");
        }

        /// <summary>
        /// Uploads and processes a JSON file containing analysis data.
        /// </summary>
        /// <param name="uploadedFile">The uploaded JSON file.</param>
        /// <returns>A redirection to the <see cref="Index"/> action or the analysis page view with the uploaded data.</returns>
        [HttpPost]
        public async Task<IActionResult> UploadAnalysisJson(IFormFile uploadedFile)
        {
            const long MaxFileSizeBytes = 1 * 1024 * 1024;

            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                TempData["UploadError"] = "No file selected.";
                return RedirectToAction("Index");
            }

            if (uploadedFile.Length > MaxFileSizeBytes)
            {
                TempData["UploadError"] = "File is too large. Max size is 1MB.";
                return RedirectToAction("Index");
            }

            if (!uploadedFile.ContentType.Equals("application/json", StringComparison.OrdinalIgnoreCase) &&
                !uploadedFile.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                TempData["UploadError"] = "Only .json files are allowed.";
                return RedirectToAction("Index");
            }

            try
            {
                using var reader = new StreamReader(uploadedFile.OpenReadStream());
                var json = await reader.ReadToEndAsync();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    MaxDepth = 32
                };

                System.Diagnostics.Debug.WriteLine("📄 Raw JSON:");
                System.Diagnostics.Debug.WriteLine(json);

                var results = JsonSerializer.Deserialize<List<BinAnalysisResult>>(json, options);

                if (results == null)
                {
                    TempData["UploadError"] = "Failed to parse uploaded JSON.";
                    return RedirectToAction("Index");
                }

                TempData["UploadSuccess"] = $"Successfully loaded {results.Count} results.";
                return View("Index", model: results);
            }
            catch (JsonException jsonEx)
            {
                TempData["UploadError"] = "❌ Invalid JSON format.";
                System.Diagnostics.Debug.WriteLine(jsonEx);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["UploadError"] = "❌ Unexpected error while processing the file.";
                System.Diagnostics.Debug.WriteLine(ex);
                return RedirectToAction("Index");
            }
        }
    }
}