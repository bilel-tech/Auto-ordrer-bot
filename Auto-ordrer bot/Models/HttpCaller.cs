using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace scrapingTemplateV51.Models
{
    public class HttpCaller
    {
        HttpClient _httpClient;
        public HttpClientHandler _httpClientHandler = new HttpClientHandler()
        {
            CookieContainer = new CookieContainer(),
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };
        public HttpCaller()
        {
            _httpClient = new HttpClient(_httpClientHandler);
            _httpClient.DefaultRequestHeaders.Add("Accep", "*/*");
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36");
        }
        public async Task<HtmlDocument> GetDoc(string url, int maxAttempts = 1)
        {
            var html = await GetHtml(url, maxAttempts);
            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(html);
            return doc;
        }
        public async Task<string> GetHtml(string url, int maxAttempts = 1)
        {
            int tries = 0;
            do
            {
                try
                {
                    if (url.Contains("api"))
                    {
                        _httpClient.DefaultRequestHeaders.Add("X-User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36 FKUA/website/42/website/Desktop");
                    }
                    var response = await _httpClient.GetAsync(url);
                    string html = await response.Content.ReadAsStringAsync();
                    if (url.Contains("api") && html.Contains("ERROR_CODE"))
                    {
                        _httpClient.DefaultRequestHeaders.Remove("X-User-Agent");
                        await Task.Delay(1000);
                        continue;
                    }
                    IEnumerable<string> cookies;
                    if (response.Headers.TryGetValues("Set-Cookie", out cookies))
                    {
                        AddCookieToCookieContainer(cookies);
                    }
                    if (_httpClient.DefaultRequestHeaders.Contains("X-User-Agent"))
                    {
                        _httpClient.DefaultRequestHeaders.Remove("X-User-Agent");
                    }
                    return html;
                }
                catch (WebException ex)
                {
                    var errorMessage = "";
                    try
                    {
                        errorMessage = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception)
                    {
                    }
                    tries++;
                    if (tries == maxAttempts)
                    {
                        throw new Exception(ex.Status + " " + ex.Message + " " + errorMessage);
                    }
                    await Task.Delay(2000);
                }
            } while (true);
        }
        public async Task<string> PostJson(string url, string json, int maxAttempts = 1)
        {
            int tries = 0;
            do
            {
                try
                {
                    _httpClient.DefaultRequestHeaders.Add("X-User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/95.0.4638.69 Safari/537.36 FKUA/website/42/website/Desktop");
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var r = await _httpClient.PostAsync(url, content);
                    var s = await r.Content.ReadAsStringAsync();
                    //Console.WriteLine("cookies from response aurthentication");
                    IEnumerable<string> cookies;
                    if (r.Headers.TryGetValues("Set-Cookie", out cookies))
                    {
                        //var cccc = r.Headers?.GetValues("Set-Cookie")?.AsEnumerable().ToList();
                        AddCookieToCookieContainer(cookies);
                    }
                    _httpClient.DefaultRequestHeaders.Remove("X-User-Agent");
                    return s;
                }
                catch (WebException ex)
                {
                    var errorMessage = "";
                    try
                    {
                        errorMessage = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception)
                    {
                    }
                    tries++;
                    if (tries == maxAttempts)
                    {
                        throw new Exception(ex.Status + " " + ex.Message + " " + errorMessage);
                    }
                    await Task.Delay(2000);
                }
            } while (true);

        }

        private void AddCookieToCookieContainer(IEnumerable<string> cookies)
        {
            foreach (var c in cookies)
            {
                var cookie = new Cookie();
                var array = c.Split(';').ToList();
                foreach (var nn in array)
                {
                    if (nn.Contains("SN="))
                    {
                        var array2 = nn.Split('=');
                        cookie.Name = array2[0];
                        cookie.Value = array2[1];

                    }
                    if (nn.Contains("T="))
                    {
                        var array2 = nn.Split('=');
                        cookie.Name = array2[0];
                        cookie.Value = array2[1];
                    }
                    if (nn.Contains("Domain="))
                    {
                        var array2 = nn.Split('=');
                        cookie.Domain = array2[1];
                    }
                }
                if (!string.IsNullOrEmpty(cookie.Name))
                {
                    _httpClientHandler.CookieContainer.Add(cookie);
                }
            }
        }

        public async Task<HtmlDocument> PostFormData(string url, List<KeyValuePair<string, string>> formData, int maxAttempts = 1)
        {
            var formContent = new FormUrlEncodedContent(formData);
            int tries = 0;
            do
            {
                try
                {
                    var doc = new HtmlDocument();
                    var response = await _httpClient.PostAsync(url, formContent);
                    string html = await response.Content.ReadAsStringAsync();
                    doc.LoadHtml(html);
                    return doc;
                }
                catch (WebException ex)
                {
                    var errorMessage = "";
                    try
                    {
                        errorMessage = new StreamReader(ex.Response.GetResponseStream()).ReadToEnd();
                    }
                    catch (Exception)
                    {
                    }
                    tries++;
                    if (tries == maxAttempts)
                    {
                        throw new Exception(ex.Status + " " + ex.Message + " " + errorMessage);
                    }
                    await Task.Delay(2000);
                }
            } while (true);
        }
    }
}
