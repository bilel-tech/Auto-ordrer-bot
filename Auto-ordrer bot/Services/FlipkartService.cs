using Auto_ordrer_bot.Models;
using Newtonsoft.Json.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using scrapingTemplateV51.Models;
using scrapingTemplateV51;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Auto_ordrer_bot.Services
{

    public class FlipkartService
    {
        public HttpCaller HttpCaller = new HttpCaller();
        public HttpCaller HttpCaller2 = new HttpCaller();
        private Config conf = new Config();
        private MainForm mainForm = new MainForm();
        //public static ChromeDriver _driver;
        public FlipkartService(Config conf, MainForm mainForm)
        {
            this.conf = conf;
            this.mainForm = mainForm;
        }
        public async Task OrderInFlipkart()
        {
            #region selenium work
            //var service = ChromeDriverService.CreateDefaultService();
            //service.HideCommandPromptWindow = true;
            //ChromeOptions options = new ChromeOptions();
            ////options.AddArgument("headless");
            //_driver = new ChromeDriver(service, options);
            //_driver.Navigate().GoToUrl(conf.ECommerceSite);
            //await Task.Delay(2000);
            //_driver.FindElement(By.XPath("//span[contains(text(),'Email')]/../../input")).SendKeys(conf.ECommerceSiteUserName);
            //_driver.FindElement(By.XPath("//span[contains(text(),'Password')]/../../input")).SendKeys(conf.ECommerceSitePassWord);
            //_driver.FindElement(By.XPath("//span[text()='Login']/parent::button")).Click();
            //await Task.Delay(5000);
            //_driver.Navigate().GoToUrl(conf.ProductLink);
            //await Task.Delay(4000);
            ////var zipCodeStatus = _driver.FindElement(By.XPath("//span[@class='_2P_LDn']/text()")).Text.Trim();
            ////if (zipCodeStatus=="")
            ////{

            ////}
            //var elmnt = _driver.FindElement(By.XPath("//input[@id='pincodeInputId']"));
            //_driver.FindElement(By.XPath("//input[@id='pincodeInputId']")).Clear();
            ////new Actions(_driver).MoveToElement(elmnt).Perform();
            //_driver.FindElement(By.XPath("//input[@id='pincodeInputId']")).SendKeys(5.ToString());
            //_driver.Quit();
            ////_driver.FindElement(By.XPath("//span[@class='_2P_LDn']/text()"));
            //_driver.FindElement(By.XPath("//button[text()='GO TO CART']")).Click();
            //await Task.Delay(4000);
            //_driver.FindElement(By.XPath("//span[text()='Place Order']/parent::button")).Click();
            //await Task.Delay(5000);
            //_driver.FindElement(By.XPath("//button[text()='CONTINUE']")).Click();
            //await Task.Delay(10000);
            //_driver.FindElement(By.XPath("//label[@for='CREDIT']")).Click();
            //await Task.Delay(5000);
            //_driver.FindElement(By.XPath("//input[@name='cardNumber']")).SendKeys(conf.CardNbr);
            //_driver.FindElement(By.XPath("//input[@name='cvv']")).SendKeys(conf.CardCVV.ToString());
            //_driver.FindElement(By.XPath("//select[@name='month']")).SendKeys(conf.CardExPiry.ToString("MM"));
            //_driver.FindElement(By.XPath("//select[@name='year']")).SendKeys(conf.CardExPiry.ToString("yy"));
            //await Task.Delay(2000);
            //_driver.FindElement(By.XPath("//button[contains(text(),'PAY')]")).Click();
            //string otpRef;
            //do
            //{
            //    try
            //    {
            //        otpRef = _driver.FindElement(By.XPath("//span[@id='otp-reference-no']"))?.Text;
            //        if (otpRef != null || otpRef == "")
            //        {
            //            break;
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex);
            //    }
            //} while (true);
            //await Task.Delay(3000);
            //var otp = await OtpSerVice.GetOtp(conf.EmailUserName, conf.EmailPassword, otpRef);
            //var js = (IJavaScriptExecutor)_driver;
            //var shaDowInput = (IWebElement)js.ExecuteScript("return document.querySelector('#content > zwe-cipher-authentication-controls').shadowRoot.getElementById('indusind_otp')");
            //shaDowInput.SendKeys(otp); 
            #endregion
            var IsloggedIn = await LogIn();
            if (!IsloggedIn)
            {
                mainForm.ErrorLog($"login failed for this account: {conf.ECommerceSiteUserName}");
                return;
            }
            mainForm.SuccessLog($"login succed for this account: {conf.ECommerceSiteUserName}");

            for (int i = 0; i < conf.RepeatCount; i++)
            {
                var products = await HttpCaller.PostJson("https://1.rome.api.flipkart.com/api/4/page/fetch", "{\"pageUri\":\"/viewcart?otracker=Cart_Icon_Click\",\"pageContext\":{\"fetchSeoData\":true}}");
                var productsInfo = await GetProductsInfo(products);
                var RemoveFromBasket = await HttpCaller.PostJson("https://1.rome.api.flipkart.com/api/1/action/view", "{\"actionRequestContext\":{\"pageUri\":\"/viewcart\",\"type\":\"CART_REMOVE\",\"pageNumber\":1,\"items\":[" + productsInfo.listgsAndIds + "]}}");
                mainForm.SuccessLog($"product(s) removed successfully from the basket in this account: {conf.ECommerceSiteUserName}");
                await AddProductsToBasket(conf.UrlProduct);
                mainForm.SuccessLog($"this product product added successfully in the basket in this account: {conf.ECommerceSiteUserName}");
                var checkOutLogin = await HttpCaller.PostJson("https://1.rome.api.flipkart.com/api/5/checkout?loginFlow=false", "{\"checkoutType\":\"PHYSICAL\"}");
                var getToken = await HttpCaller.GetHtml("https://1.rome.api.flipkart.com/api/3/checkout/paymentToken");
                var objt = JObject.Parse(getToken);
                var token = (string)objt.SelectToken("..token");
                var json = "{\"token\":\"" + token + "\"," + "\"payment_instrument\":\"CREDIT\",\"card_number\":\"" + conf.CardNbr + "\"}";
                var instr = await HttpCaller.PostJson("https://1.pay.payzippy.com/fkpay/api/v3/payments/instrumentcheck?token=" + token, json);
                var otpJson = await GetOtpFormat(token);
                await GetOtpPage(otpJson);
            }

        }
        private async Task GetOtpPage(string jsonOfOtpRequest)
        {
            var obj = JObject.Parse(jsonOfOtpRequest);
            var mD = (string)obj.SelectToken("..MD");
            var paReq = ((string)obj.SelectToken("..PaReq")).Replace("\n", "");
            var termUrl = (string)obj.SelectToken("..TermUrl");

            var formData = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("MD", mD),
                new KeyValuePair<string, string>("PaReq", paReq),
                new KeyValuePair<string, string>("TermUrl", termUrl),
                new KeyValuePair<string, string>("TermUrl", termUrl),
                new KeyValuePair<string, string>("pgMsg", "Success"),
            };
            var doc = await HttpCaller2.PostFormData("https://indusindbank-mas102-cipher2-mum.gw.zetapay.in/mastercardcipher/pareq", formData);
            //doc.Save("otp page.html");
            //Process.Start("otp page.html");
            //var authContextID = doc.DocumentNode.SelectSingleNode("//input[@id='auth-context-id']").GetAttributeValue("value", "").Trim();
            var otpRef = doc.DocumentNode.SelectSingleNode("//span[@id='otp-reference-no']").GetAttributeValue("value", "").Trim();
            //var otp = await OtpSerVice.GetOtp(conf.EmailUserName, conf.EmailPassword, "ECOM_632063474876");
            var otp = await OtpSerVice.GetOtp(conf.EmailUserName, conf.EmailPassword, otpRef);
            mainForm.NormalLog($"OTP Reference:{otpRef} ==> OTP: {otp}");
            //var json = "{\"challengeResponse\":\"123456\",\"authContextID\":\"" + authContextID + "\"," + "\"factorNumber\":\"1\",\"authPlanID\":\"4\",\"authType\":\"indusind_otp\"}";
            //var json = "{\"challengeResponse\":\"" + authContextID + "\"," + "\"authContextID\":\"" + authContextID + "\"," + "\"factorNumber\":\"1\",\"authPlanID\":\"4\",\"authType\":\"indusind_otp\"}";
            //var verification = await HttpCaller2.PostJson("https://edith-cipher2.gw.zetapay.in/v1.0/web/authenticate/verify", json);
        }
        private async Task<string> GetOtpFormat(string token)
        {
            var json = "{\"auth_mode\":\"_3DS\",\"card_number\":\"5376521033171026\",\"payment_instrument\":\"CREDIT\",\"cvv\":\"123\",\"expiry_month\":\"12\",\"expiry_year\":\"21\",\"token\":\"" + token + "\"}";
            do
            {
                var otpJson = await HttpCaller.PostJson("https://1.pay.payzippy.com/fkpay/api/v3/payments/paywithdetails?token=" + token, json);
                var obj = JObject.Parse(otpJson);
                var paReq = (string)obj?.SelectToken("..PaReq");
                if (paReq == null)
                {
                    await Task.Delay(300);
                    continue;
                }
                return otpJson;
            } while (true);
        }
        private async Task<bool> LogIn()
        {
            var tries = 0;
            do
            {
                var doc = await HttpCaller.GetDoc("https://www.flipkart.com/");
                var formData = "{\"loginId\":\"" + conf.ECommerceSiteUserName + "\"," + "\"password\":\"" + conf.ECommerceSitePassWord + "\"}";
                var authenticatioResponse = await HttpCaller.PostJson("https://2.rome.api.flipkart.com/api/4/user/authenticate", formData);
                if (authenticatioResponse.Contains("ERROR_CODE"))
                {
                    HttpCaller = new HttpCaller();
                    await Task.Delay(200);
                    tries++;
                    if (tries == 10)
                    {

                        return false;
                    }
                    continue;
                }
                return true;
            } while (true);
        }
        private async Task AddProductsToBasket(string urlProduct)
        {
            var pidIndex = urlProduct.IndexOf("pid=") + 4;
            var lidIndex = urlProduct.IndexOf("&lid=");
            var marketplaceIndex = urlProduct.IndexOf("&marketplace");
            var pid = urlProduct.Substring(pidIndex, lidIndex - pidIndex);
            var lid = urlProduct.Substring(lidIndex + 5, marketplaceIndex - (lidIndex + 5));
            //foreach (var listingId in listingIds)
            //{
            do
            {
                var addFormat = "{\"cartContext\":{\"" + lid + "\":{\"quantity\":1}}}";
                //{\"cartContext\":{\"LSTPHTG7CNNZANSNZBYYYGDQH\":{\"quantity\":1}}}
                var addToBasket = await HttpCaller.PostJson("https://1.rome.api.flipkart.com/api/5/cart", addFormat);
                var obj = JObject.Parse(addToBasket);
                var requestId = obj.SelectToken("REQUEST-ID");
                if (requestId != null)
                    break;
                await Task.Delay(200);
            } while (true);
            //}
        }
        private async Task<(string listgsAndIds, List<string> listings)> GetProductsInfo(string products)
        {
            var obj = JObject.Parse(products);
            var items = obj.SelectTokens("..marketplaceData..items").First();
            var listings = new StringBuilder();
            var listings2 = new List<string>();

            foreach (var item in items)
            {
                var productId = (string)item.SelectToken("productId");
                var listingId = (string)item.SelectToken("listingId");
                var listing = "{\"listingId\":\"" + listingId + "\"," + "\"productId\":\"" + productId + "\"},";
                var lid = "{\"" + listingId + "\":{\"quantity\":1}}";
                listings.Append(listing);
                listings2.Add(lid);
            }
            var listgsAndIds = listings.ToString().Remove(listings.ToString().Length - 1);
            return (listgsAndIds, listings2);
        }
    }
}
