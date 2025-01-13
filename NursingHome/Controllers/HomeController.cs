using Microsoft.AspNetCore.Mvc;
using NursingHome.Models;
using System.Diagnostics;
using NursingHome.Db.Implementation;
using NursingHome.Db.Interface;
using NursingHome.Db.ViewModel;
using System;

using System.Net.Http;

using System.Threading.Tasks;

using HtmlAgilityPack;
using System.Text;
using Tesseract;
namespace NursingHome.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IHomeService _DbConn;
        private readonly HttpClient _httpClient;
        private readonly IWebHostEnvironment _env;

        public HomeController(ILogger<HomeController> logger, IHomeService db, IWebHostEnvironment env)
        {
            _logger = logger;
            _DbConn = db;
            _httpClient = new HttpClient();
            _env = env;
            //  GetDropdownValues();
            //GetDropdownValuesAsync();
            //  GetTalukValues(2);
            GetDropdownValuesAsyncData();
        }

        public IActionResult Index()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _DbConn.SaveLog("HomeController", "Index", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult Reports()
        {
            try
            {
                return View();
            }
            catch (Exception ex)
            {
                _DbConn.SaveLog("HomeController", "Reports", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult GetLast30DaysRecords()
        {
            try
            {
                var oldageData = _DbConn.TotalOldAgeAdmissionLast30days();
                var NursingData = _DbConn.TotalHomeNursingAdmissionLast30days();
                var HelperData = _DbConn.TotalHelpersAdded30days();

                return Json(new { oldeAge = oldageData, nursingHome = NursingData, helper = HelperData });
            }
            catch (Exception ex)
            {
                _DbConn.SaveLog("HomeController", "GetLast30DaysRecords", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult GetAdmissionData(DateTime startDate, DateTime endDate)
        {
            try
            {
                var admissionCounts = new AdmissionCounts
                {
                    OldAge = _DbConn.GetOldAgeCounts(startDate, endDate),
                    NursingHome = _DbConn.GetNursingHomeCounts(startDate, endDate),
                    Helpers = _DbConn.GetHelpersCounts(startDate, endDate),
                };

                return Json(admissionCounts);
            }
            catch (Exception ex)
            {
                _DbConn.SaveLog("HomeController", "GetAdmissionData", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            try
            {
                return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
            catch (Exception ex)
            {
                _DbConn.SaveLog("HomeController", "Error", ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }
        //public async Task<ActionResult<string[]>> GetDropdownValues()

        //{

        // //   var dropdownValueFetcher = new DropdownValueFetcher();

        //    var dropdownValues = await GetDropdownValuesAsync();


        //    return dropdownValues;

        //}

        //public async Task<string[]> GetDropdownValuesAsync()

        //{

        //    using (var httpClient = new HttpClient())

        //    {

        //        var response = await httpClient.GetAsync("https://landrecords.karnataka.gov.in/Service2/");


        //        if (response.IsSuccessStatusCode)

        //        {

        //            var htmlContent = await response.Content.ReadAsStringAsync();

        //            var htmlDocument = new HtmlDocument();

        //            htmlDocument.LoadHtml(htmlContent);


        //            var dropdown = htmlDocument.GetElementbyId("ctl00_MainContent_ddlCDistrict");

        //            var options = dropdown.SelectNodes(".//option");


        //            var dropdownValues = new string[options.Count];


        //            for (int i = 0; i < options.Count; i++)

        //            {

        //                dropdownValues[i] = options[i].InnerText;

        //            }


        //            return dropdownValues;

        //        }

        //        else

        //        {

        //            throw new Exception("Failed to retrieve dropdown values");

        //        }

        //    }


        //}
        //public async Task<string[]> GetTalukValuesAsync(string districtId)

        //{

        //    // Assuming the selected districtId is used to fetch the taluk options

        //    var response = await _httpClient.GetAsync($"https://landrecords.karnataka.gov.in/Service2/GetTaluks?districtId={districtId}");

        //    response.EnsureSuccessStatusCode();


        //    var htmlContent = await response.Content.ReadAsStringAsync();

        //    var htmlDocument = new HtmlDocument();

        //    htmlDocument.LoadHtml(htmlContent);


        //    var talukDropdown = htmlDocument.GetElementbyId("ctl00_MainContent_ddlCTaluk");

        //    var options = talukDropdown.SelectNodes(".//option");


        //    var talukValues = options.Select(option => option.InnerText).ToArray();

        //    return talukValues;

        //}
        public async Task<string[]> GetDropdownValuesAsync()

        {

            var response = await _httpClient.GetAsync("https://rtc.karnataka.gov.in/Service78/RTC.aspx");

            response.EnsureSuccessStatusCode();


            var htmlContent = await response.Content.ReadAsStringAsync();

            var htmlDocument = new HtmlDocument();

            htmlDocument.LoadHtml(htmlContent);


            var dropdown = htmlDocument.GetElementbyId("MainContent_ddlCDistrict");

            var options = dropdown.SelectNodes(".//option");


            var dropdownValues = options.Select(option => option.InnerText).ToArray();

            return dropdownValues;

        }


        //public async Task<string[]> GetTalukValuesAsync(string districtId)

        //{

        //    // Assuming the selected districtId is used to fetch the taluk options

        //    var response = await _httpClient.GetAsync($"https://landrecords.karnataka.gov.in/Service2/GetTaluks?districtId={districtId}");

        //    response.EnsureSuccessStatusCode();


        //    var htmlContent = await response.Content.ReadAsStringAsync();

        //    var htmlDocument = new HtmlDocument();

        //    htmlDocument.LoadHtml(htmlContent);


        //    var talukDropdown = htmlDocument.GetElementbyId("ctl00_MainContent_ddlCTaluk");

        //    var options = talukDropdown.SelectNodes(".//option");


        //    var talukValues = options.Select(option => option.InnerText).ToArray();

        //    return talukValues;



        //}
        //public async Task<string[]> GetTalukValuesAsync(string districtId)

        //{

        //    // Assuming the selected districtId is used to fetch the taluk options

        //    var response = await _httpClient.GetAsync($"https://landrecords.karnataka.gov.in/Service2/GetTaluks?districtId={districtId}");

        //    response.EnsureSuccessStatusCode();


        //    var htmlContent = await response.Content.ReadAsStringAsync();

        //    var htmlDocument = new HtmlDocument();

        //    htmlDocument.LoadHtml(htmlContent);


        //    var talukDropdown = htmlDocument.GetElementbyId("ctl00_MainContent_ddlCTaluk");

        //    var options = talukDropdown.SelectNodes(".//option");


        //    var talukValues = options.Select(option => option.InnerText).ToArray();

        //    return talukValues;

        //}

        //public async Task<string[]> GetTalukValuesAsync(string districtId)
        //{
        //    // Hardcoded district ID for testing
        //    districtId = "2"; // Replace with a valid district ID

        //    var url = "https://landrecords.karnataka.gov.in/Service2/GetTaluks";
        //    var postData = $"__EVENTTARGET=ctl00%24MainContent%24ddlCDistrict&__EVENTARGUMENT=&__VIEWSTATE={ViewState}&__VIEWSTATEGENERATOR={ViewStateGenerator}&ctl00%24MainContent%24ddlCDistrict={districtId}";

        //    var response = await _httpClient.PostAsync(url, new StringContent(postData, Encoding.UTF8, "application/x-www-form-urlencoded"));
        //    response.EnsureSuccessStatusCode();

        //    var htmlContent = await response.Content.ReadAsStringAsync();
        //    var htmlDocument = new HtmlDocument();
        //    htmlDocument.LoadHtml(htmlContent);

        //    var talukDropdown = htmlDocument.GetElementbyId("ctl00_MainContent_ddlCTaluk");
        //    var options = talukDropdown.SelectNodes(".//option");

        //    var talukValues = options.Select(option => option.InnerText).ToArray();
        //    return talukValues;
        //}
        public async Task<string[]> GetTalukValuesAsync(int districtId)
        {
            // Step 1: Get the main page content to retrieve required view state values
            var response = await _httpClient.GetAsync("https://landrecords.karnataka.gov.in/Service2/");
            response.EnsureSuccessStatusCode();

            var htmlContent = await response.Content.ReadAsStringAsync();
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(htmlContent);

            // Extract necessary values (like __VIEWSTATE and __EVENTVALIDATION)
            var viewState = htmlDocument.GetElementbyId("__VIEWSTATE")?.GetAttributeValue("value", "");
            var eventValidation = htmlDocument.GetElementbyId("__EVENTVALIDATION")?.GetAttributeValue("value", "");

            // Step 2: Prepare the postback parameters
            var postData = new FormUrlEncodedContent(new[]
            {
        new KeyValuePair<string, string>("__EVENTTARGET", "ctl00$MainContent$ddlCDistrict"),
        new KeyValuePair<string, string>("__EVENTARGUMENT", ""),
        new KeyValuePair<string, string>("__VIEWSTATE", viewState),
        new KeyValuePair<string, string>("__EVENTVALIDATION", eventValidation),
        new KeyValuePair<string, string>("ctl00$MainContent$ddlCDistrict", districtId.ToString())
    });

            // Step 3: Send the post request to the same URL
            var talukResponse = await _httpClient.PostAsync("https://landrecords.karnataka.gov.in/Service2/", postData);

            talukResponse.EnsureSuccessStatusCode();
            var talukHtmlContent = await talukResponse.Content.ReadAsStringAsync();
            htmlDocument.LoadHtml(talukHtmlContent);

            // Step 4: Extract options from the Taluk dropdown
            var talukDropdown = htmlDocument.GetElementbyId("ctl00_MainContent_ddlCTaluk");
            var options = talukDropdown?.SelectNodes(".//option");

            var talukValues = options?.Select(option => option.InnerText).ToArray() ?? Array.Empty<string>();

            return talukValues;
        }


        public async Task<ActionResult<string[]>> GetTalukValues(int districtId)

        {

            var talukValues = await GetTalukValuesAsync(districtId);

            return talukValues;

        }

        public async Task<string[]> GetDropdownValuesAsyncData()
        {
            string username = "Kushi2020";
            string password = "Csc2024@";

            try
            {
                // Step 1: Get the captcha image
                var captchaImageResponse = await _httpClient.GetAsync("https://rtc.karnataka.gov.in/Service78/Login.aspx");
                captchaImageResponse.EnsureSuccessStatusCode();

                var htmlContent = await captchaImageResponse.Content.ReadAsStringAsync();
                var htmlDocument = new HtmlDocument();
                htmlDocument.LoadHtml(htmlContent);

                var captchaImageNode = htmlDocument.GetElementbyId("imgCaptcha");
                if (captchaImageNode == null)
                {
                    Console.WriteLine("Captcha image node not found.");
                    return Array.Empty<string>();
                }

                var captchaImageUrl = captchaImageNode.GetAttributeValue("src", "");
                // Combine with base URL if needed
                if (!Uri.IsWellFormedUriString(captchaImageUrl, UriKind.Absolute))
                {
                    var baseUrl = "https://rtc.karnataka.gov.in/Service78/Login.aspx";
                    captchaImageUrl = new Uri(new Uri(baseUrl), captchaImageUrl).ToString();
                }

                // Download the captcha image
                var captchaImageBytes = await _httpClient.GetByteArrayAsync(captchaImageUrl);
                var captchaImagePath = Path.Combine(Path.GetTempPath(), $"captcha_{Guid.NewGuid()}.png");
                await System.IO.File.WriteAllBytesAsync(captchaImagePath, captchaImageBytes);

                // Step 2: Decode the captcha
                string captchaValue = string.Empty;
                var tessDataPath = Path.Combine(_env.WebRootPath, "tessdata"); // Path to tessdata in wwwroot

                if (!Directory.Exists(tessDataPath))
                {
                    Console.WriteLine($"Tessdata directory does not exist: {tessDataPath}");
                    return Array.Empty<string>();
                }

                using (var engine = new TesseractEngine(tessDataPath, "eng", EngineMode.Default))
                {
                    using (var img = Pix.LoadFromFile(captchaImagePath))
                    {
                        using (var page = engine.Process(img))
                        {
                            captchaValue = page.GetText().Trim();
                        }
                    }
                }

                // Check if we obtained the captcha value
                if (string.IsNullOrWhiteSpace(captchaValue))
                {
                    Console.WriteLine("Failed to decode the captcha.");
                    return Array.Empty<string>();
                }

                // Step 3: Log in
                var loginData = new Dictionary<string, string>
        {
            { "txtUname", username },
            { "txtPwd", password },
            { "txtCaptcha", captchaValue }
        };

                var loginContent = new FormUrlEncodedContent(loginData);
                var loginResponse = await _httpClient.PostAsync("https://rtc.karnataka.gov.in/Service78/RTC.aspx", loginContent);
                loginResponse.EnsureSuccessStatusCode();

             
                // Fetch the HTML content again after login
                var response = await _httpClient.GetAsync("https://rtc.karnataka.gov.in/Service78/RTC.aspx");
                response.EnsureSuccessStatusCode();

                var dropdownHtmlContent = await response.Content.ReadAsStringAsync();
                var dropdownHtmlDocument = new HtmlDocument();
                dropdownHtmlDocument.LoadHtml(dropdownHtmlContent);

                // Attempt to find the dropdown by ID
                var dropdown = dropdownHtmlDocument.GetElementbyId("MainContent_ddlCDistrict");
                if (dropdown != null)
                {
                    var options = dropdown.SelectNodes(".//option");
                    return options.Select(option => option.InnerText).ToArray();
                }

                // If the dropdown isn't found, log a message for debugging
                Console.WriteLine("Dropdown not found or is empty.");
                return Array.Empty<string>();
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP Request error: {httpEx.Message}");
                return Array.Empty<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                return Array.Empty<string>();
            }
        }

    }
}
