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

        public AnalysisController(ApplicationDbContext context, DbSeeder dbSeeder)
        {
            _context = context;
            _dbSeeder = dbSeeder;   
        }

        /// <summary>
        /// loads the analysis page.
        /// </summary>
        public async Task<IActionResult> Index()
        {
            await Task.CompletedTask;
            return View();
        }

        /// <summary>
        /// Seeds analysis data for ML experimentation.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> SeedAnalysisData()
        {
            await DbSeeder.SeedAnalysisDataAsync(_context);
            return RedirectToAction(nameof(DownloadCsv));
        }

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

        [HttpPost]
        public async Task<IActionResult> UploadAnalysisJson(IFormFile uploadedFile)
        {
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                TempData["UploadError"] = "No file selected.";
                return RedirectToAction("Index");
            }

            try
            {
                using var reader = new StreamReader(uploadedFile.OpenReadStream());
                var json = await reader.ReadToEndAsync();



                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                System.Diagnostics.Debug.WriteLine("📄 Raw JSON:");
                System.Diagnostics.Debug.WriteLine(json);


                var results = JsonSerializer.Deserialize<List<BinAnalysisResult>>(json, options);

                if (results == null)
                {
                    TempData["UploadError"] = "Failed to parse uploaded JSON.";
                    System.Diagnostics.Debug.WriteLine("❌ Deserialization returned null.");
                    return RedirectToAction("Index");
                }

                TempData["UploadSuccess"] = $"Successfully loaded {results.Count} results.";
                return View("Index", model: results);
            }
            catch (Exception ex)
            {
                TempData["UploadError"] = "❌ Failed to parse uploaded JSON.";
                System.Diagnostics.Debug.WriteLine("❌ Exception during JSON deserialization:");
                System.Diagnostics.Debug.WriteLine(ex.Message);
                System.Diagnostics.Debug.WriteLine(ex.StackTrace);
                return RedirectToAction("Index");
            }

        }
    }
}