using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using WebsiteFormAutoFiller.Models;
using WebsiteFormAutoFiller.Services;

namespace WebsiteFormAutoFiller.Controllers
{
    public class HomeController : Controller
    {
        private static BlockingCollection<string> _liveLogs = new();
        private readonly SeleniumService _selenium;


        public HomeController(SeleniumService selenium)
        {
            _selenium = selenium;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }


        [HttpPost("/Home/UploadAndStart")]
        public async Task<IActionResult> UploadAndStart(FormInput input)
        {
            if (input.UrlFile == null || input.UrlFile.Length == 0)
                return BadRequest("No file uploaded.");

            var tempFile = Path.GetTempFileName();
            using (var stream = new FileStream(tempFile, FileMode.Create))
                await input.UrlFile.CopyToAsync(stream);

            var urls = await System.IO.File.ReadAllLinesAsync(tempFile);

            // ✅ Convert extra fields to dictionary
            var extras = input.ExtraFields?
                .Where(f => !string.IsNullOrWhiteSpace(f.Key))
                .ToDictionary(f => f.Key.Trim(), f => f.Value?.Trim() ?? "") ?? new Dictionary<string, string>();

            _ = Task.Run(async () =>
            {
                await _selenium.ProcessUrlsLiveAsync(
     urls,
     extras,
     msg => _liveLogs.Add(msg)
 );

                _liveLogs.CompleteAdding();
            });

            return Ok();
        }

        [HttpGet("/Home/StreamProgress")]
        public async Task StreamProgress()
        {
            Response.Headers.Add("Content-Type", "text/event-stream");
            foreach (var log in _liveLogs.GetConsumingEnumerable())
            {
                await Response.WriteAsync($"data: {log}\n\n");
                await Response.Body.FlushAsync();
            }
        }

        [HttpGet("/Home/DownloadReport")]
        public IActionResult DownloadReport()
        {
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "report.csv");
            if (!System.IO.File.Exists(path)) return NotFound("Report not found.");
            var fileBytes = System.IO.File.ReadAllBytes(path);
            return File(fileBytes, "text/csv", "AutofillReport.csv");
        }

    }
}
