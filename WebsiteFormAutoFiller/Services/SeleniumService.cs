using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;

namespace WebsiteFormAutoFiller.Services
{
    public class SeleniumService
    {
        public async Task ProcessUrlsLiveAsync(string[] urls, Dictionary<string, string> extraFields, Action<string> report)
        {
            int success = 0, failed = 0, skipped = 0;

            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            options.AddArgument("--disable-infobars");
            options.AddExcludedArgument("enable-automation");
            options.AddAdditionalOption("useAutomationExtension", false);

            using var driver = new ChromeDriver(options);

            var csvRows = new List<string[]>();
            csvRows.Add(new[] { "URL", "Result" });

            int count = 0;
            foreach (var raw in urls)
            {
                count++;
                string url = raw?.Trim();
                if (string.IsNullOrWhiteSpace(url)) continue;
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                    url = "https://" + url;

                try
                {
                    report($"🌐 [{count}/{urls.Length}] Total: {url}");
                    driver.Navigate().GoToUrl(url);

                    await Task.Delay(1000);
                    var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
                    wait.Until(d => ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState").Equals("complete"));

                    bool filled = TryFillFormWithAI(driver, extraFields);

                    if (!filled)
                    {
                        string[] linkKeywords = new[] { "contact", "contact us", "reach us", "support", "get in touch", "about", "about us", "who we are", "know us" };

                        try
                        {
                            var links = driver.FindElements(By.TagName("a"));
                            foreach (var keyword in linkKeywords)
                            {
                                var matchingLink = links.FirstOrDefault(a => (a.Text ?? "").Trim().ToLower().Contains(keyword));
                                if (matchingLink != null)
                                {
                                    matchingLink.Click();
                                    await Task.Delay(5000);
                                    filled = TryFillFormWithAI(driver, extraFields);
                                    if (filled) break;
                                }
                            }
                        }
                        catch { }
                    }

                    if (filled)
                    {
                        success++;
                        csvRows.Add(new[] { url, "Success" });
                        report($"✅ Success: {url} (Total: ✅ {success} ❌ {failed} ⚠️ {skipped})");
                    }
                    else
                    {
                        skipped++;
                        csvRows.Add(new[] { url, "Skipped" });
                        report($"⚠️ Skipped (no form): {url} (Total: ✅ {success} ❌ {failed} ⚠️ {skipped})");
                    }
                }
                catch (Exception ex)
                {
                    failed++;
                    csvRows.Add(new[] { url, "Failed" });
                    report($"❌ Failed: {url} | {ex.Message} (Total: ✅ {success} ❌ {failed} ⚠️ {skipped})");
                }
            }

            driver.Quit();

            var reportPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "report.csv");
            using (var writer = new StreamWriter(reportPath))
            {
                foreach (var row in csvRows)
                {
                    await writer.WriteLineAsync(string.Join(",", row));
                }
            }

            report($"📊 Finished: Total {urls.Length} | ✅ Success: {success} | ❌ Failed: {failed} | ⚠️ Skipped: {skipped}");
        }

        private bool TryFillFormWithAI(IWebDriver driver, Dictionary<string, string> extraFields)
        {
            try
            {
                var formElements = driver.FindElements(By.CssSelector("input, textarea"));
                if (!formElements.Any()) return false;

                bool filledAny = false;

                foreach (var el in formElements)
                {
                    string allAttributes = string.Join(" ",
                        el.GetAttribute("name") ?? "",
                        el.GetAttribute("id") ?? "",
                        el.GetAttribute("placeholder") ?? "",
                        GetLabelTextFor(driver, el)).ToLower();

                    try
                    {
                        bool matched = false;

                        var fieldMapping = new Dictionary<string, string[]>
                        {
                            { "Name", new[] { "name", "fullname", "yourname", "contactperson", "username", "first name", "last name", "your name" } },
                            { "Email", new[] { "email", "e-mail", "youremail", "mail", "email address" } },
                            { "Phone", new[] { "phone", "mobile", "contact", "whatsapp", "tel", "number", "telephone", "contact number" } },
                            { "Message", new[] { "message", "comment", "description", "enquiry", "note", "feedback", "remarks", "your message" } },
                            { "Subject", new[] { "subject", "title", "regarding", "about" } },
                            { "Company", new[] { "company", "organization", "business", "company name" } }
                        };

                        foreach (var mapping in fieldMapping)
                        {
                            if (extraFields.TryGetValue(mapping.Key, out var value) && ContainsAny(allAttributes, mapping.Value))
                            {
                                el.Clear();
                                el.SendKeys(value);
                                filledAny = true;
                                matched = true;
                                break;
                            }
                        }

                        if (!matched)
                        {
                            foreach (var kv in extraFields)
                            {
                                if (ContainsAny(allAttributes, kv.Key.ToLower()))
                                {
                                    el.Clear();
                                    el.SendKeys(kv.Value);
                                    filledAny = true;
                                    break;
                                }
                            }
                        }
                    }
                    catch { }
                }

                try
                {
                    var submitBtn = driver.FindElements(By.CssSelector("button, input[type=submit]"))
                        .FirstOrDefault(b => b.Text.ToLower().Contains("submit") || (b.GetAttribute("type")?.ToLower() == "submit"));
                    submitBtn?.Click();
                }
                catch { }

                return filledAny;
            }
            catch { return false; }
        }

        private string GetLabelTextFor(IWebDriver driver, IWebElement input)
        {
            try
            {
                var id = input.GetAttribute("id");
                if (!string.IsNullOrWhiteSpace(id))
                {
                    var label = driver.FindElements(By.TagName("label"))
                        .FirstOrDefault(l => (l.GetAttribute("for") ?? "") == id);
                    return label?.Text ?? "";
                }
            }
            catch { }
            return "";
        }

        private bool ContainsAny(string text, params string[] keywords)
        {
            return keywords.Any(k => text.Contains(k));
        }

        private int RandomDelay(int minMs, int maxMs)
        {
            return new Random().Next(minMs, maxMs);
        }
    }
}
