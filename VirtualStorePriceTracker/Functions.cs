using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Microsoft.Playwright;
using File = System.IO.File;
using System.Text.RegularExpressions;
using Database;
using System.Diagnostics;
using API;
using System.Linq.Expressions;

namespace Functions
{

    public class Functions : PageObjects.PageObjects
    {

        private static string AplicationFolder = string.Empty;

        public enum Store
        {
            Amazon = 1,
            Kabum = 2,
            PSStore = 3,
            Steam = 4,
            Pichau = 5,
            Magazine_Luiza = 6,
            Terabyte_Shop = 7,
            Nuuvem = 8,
            GreenManGaming = 9,
            GOG = 10,
            Epic = 11,
            Xbox = 12
        }

        public enum LogType
        {
            ProductNameUpdate = 1,
            ProductPriceUpdate = 2,
            EmailSent = 3,
            WhatsappMessageSent = 5
        }

        public static async Task<double?> GetProductPrice(IPage page, string url, Store store)
        {
            double? price = null;

            if (store == Store.Amazon)
            {
                price = await GetPriceAmazon(page, url);
                return price;
            }

            else if (store == Store.Kabum)
            {
                price = await GetPriceKabum(page, url);
                return price;
            }

            else if (store == Store.PSStore)
            {
                price = await GetPricePlaystation(page, url);
                return price;
            }

            else if (store == Store.Steam)
            {
                int? AppId = returnSteamIdapp(url);
                string FormatedPrice = null;
                if (AppId != null)
                {
                    price = await API.StemAPI.GetSteamPrice(AppId.ToString());
                    if (price != null)
                    {
                        FormatedPrice = price.ToString();
                    }
                    if (double.TryParse(FormatedPrice, out double DecimalPrice))
                    {
                        return DecimalPrice;
                    }
                    return null;
                }
            }

            else if (store == Store.Pichau)
            {
                price = await GetPricePichau(page, url);
                return price;
            }

            else if (store == Store.Magazine_Luiza)
            {
                price = await GetPriceMagazineLuiza(page, url);
                return price;
            }

            else if (store == Store.Terabyte_Shop)
            {
                price = await GetPriceTeraByteShop(page, url);
                return price;
            }

            else if (store == Store.Nuuvem)
            {
                price = await GetPriceNuuvem(page, url);
                return price;
            }

            else if (store == Store.GreenManGaming)
            {
                price = await GetPriceGreenManGaming(page, url);
                return price;
            }

            else if (store == Store.GOG)
            {
                price = await GetPriceGOG(page, url);
                return price;
            }

            else if (store == Store.Epic)
            {
                price = await GetPriceEpic(page, url);
                return price;
            }

            else if (store == Store.Xbox)
            {
                price = await GetPriceXbox(page, url);
                return price;
            }

            return null;
        }

        public static async Task<string> GetProductName(IPage page, string productUrl, Store store)
        {
            string? textProductName;
            if (store == Store.Amazon)
            {
                textProductName = await page.Locator(LabelAmazonProductName).TextContentAsync();
                return textProductName;
            }
            else if (store == Store.Kabum)
            {
                int elements = await page.Locator(LabelKabumProductName).CountAsync();
                if (elements > 0)
                {
                    textProductName = await page.Locator(LabelKabumProductName).TextContentAsync();
                    return textProductName;
                }
                else
                {
                    return null;
                }
            }
            else if (store == Store.PSStore)
            {
                textProductName = await page.Locator(LabelPlaystationProductName).TextContentAsync();
                return textProductName;
            }
            else if (store == Store.Steam)
            {
                int? AppID = returnSteamIdapp(productUrl);
                if (AppID != null)
                {
                    textProductName = await API.StemAPI.GetSteamAppName(AppID.ToString());
                    return textProductName;
                }
            }
            else if (store == Store.Pichau)
            {
                textProductName = await page.Locator(LabelPichauProductName).TextContentAsync();
                return textProductName;
            }

            else if (store == Store.Magazine_Luiza)
            {
                textProductName = await page.Locator(LabelMagazineLuizaProductName).TextContentAsync();
                return textProductName;
            }

            else if (store == Store.Terabyte_Shop)
            {
                textProductName = await page.Locator(LabelTerabyteProductName).TextContentAsync();
                return textProductName;
            }

            else if (store == Store.Nuuvem)
            {
                textProductName = await page.Locator(LabelNuuvemProductName).TextContentAsync();
                return textProductName;
            }

            else if (store == Store.GreenManGaming)
            {
                int elements = await page.Locator(LabelGreenManGamingProductName).CountAsync();
                if (elements > 0)
                {
                    elements = elements - 1;
                    LabelGreenManGamingProductName = LabelGreenManGamingProductName + " >> nth = " + elements.ToString();
                    textProductName = await page.Locator(LabelGreenManGamingProductName).TextContentAsync();
                    return textProductName;
                }
            }

            else if (store == Store.GOG)
            {
                textProductName = await page.Locator(LabelGOGProductName).TextContentAsync();
                return textProductName;
            }

            else if (store == Store.Epic)
            {
                var scriptContent = await page.Locator(ScriptEpic).InnerTextAsync();
                var jsonObject = JsonDocument.Parse(scriptContent).RootElement;
                textProductName = jsonObject.GetProperty("name").GetString();
                return textProductName;
            }

            else if (store == Store.GOG)
            {
                textProductName = await page.Locator(LabelGOGProductName).TextContentAsync();
                return textProductName;
            }

            else if (store == Store.Xbox)
            {
                textProductName = await page.Locator(LabelXboxProductName).TextContentAsync();
                return textProductName;
            }

            return null;
        }

        public static async Task<double?> GetPriceAmazon(IPage page, string url)
        {
            var priceElementInteger = await page.QuerySelectorAsync(LabelAmazonPriceInteger);
            var priceElementDecimal = await page.QuerySelectorAsync(LabelAmazonPriceDecimal);

            if (priceElementInteger != null)
            {
                var priceTextInteger = await priceElementInteger.InnerTextAsync();
                var priceTextDecimal = await priceElementDecimal.InnerTextAsync();
                var priceText = priceTextInteger.ToString() + priceTextDecimal.ToString();
                priceText = priceText.Replace("\n", "").Replace(",", "");

                if (double.TryParse(priceText, out double price))
                {
                    price = price / 100;
                    return price;
                }
            }
            return null;
        }

        public static async Task<double?> GetPriceKabum(IPage page, string url)
        {
            string? priceElement = null;
            int elements = await page.Locator(LabelKabumPrice).CountAsync();
            if (elements > 0)
            {
                priceElement = await page.Locator(LabelKabumPrice).TextContentAsync();
            }

            if (priceElement != null)
            {
                priceElement = priceElement.Replace("R$", "").Replace(".", "").Trim();

                if (double.TryParse(priceElement, out double price))
                {
                    return price;
                }
            }
            return null;
        }

        public static async Task<double?> GetPricePlaystation(IPage page, string url)
        {
            await page.WaitForTimeoutAsync(2000);
            string? priceElement = null;
            var locator = page.Locator(LabelPlaystationPrice);
            if (await locator.IsVisibleAsync())
            {
                try
                {
                    priceElement = await locator.InnerTextAsync();                    
                }
                catch (Exception ex)
                {
                    await TakeScreenshot(page);
                    throw;
                }
            }

            if (priceElement != null)
            {
                priceElement = priceElement.Replace("R$", "").Trim();

                if (double.TryParse(priceElement, out double price))
                {
                    return price;
                }
            }
            return null;
        }

        public static async Task<double?> GetPricePichau(IPage page, string url)
        {            
            int elements404 = await page.Locator(LabelPichauErro404).CountAsync();
            if (elements404 > 0)
            {
                return null;
            }
            else
            {
                string? priceElement = null;
                var previousElementXPath = LabelPichauPrice;
                var previousElement = page.Locator(previousElementXPath);
                var priceElementXPath = previousElementXPath + "/following-sibling::div >> nth = 0";
                var priceElementPichau = page.Locator(priceElementXPath);
                int elements = await priceElementPichau.CountAsync();
                if (elements > 0)
                {
                    priceElement = await priceElementPichau.TextContentAsync();
                }

                if (priceElement != null)
                {
                    priceElement = priceElement.Replace("R$", "").Trim();

                    if (double.TryParse(priceElement, out double price))
                    {
                        return price;
                    }
                }
            }
            return null;
        }

        public static async Task<double?> GetPriceMagazineLuiza(IPage page, string url)
        {
            string? priceElement = null;
            int elementoNaoDisponivel = await page.Locator(LabelMagazineLuizaProdutoNaoDisponivel).CountAsync();
            if (elementoNaoDisponivel == 0)
            {
                int elements = await page.Locator(LabelMagazineLuizaPrice).CountAsync();
                if (elements > 0)
                {                    
                    ILocator priceElementFiltrado = page.Locator(LabelMagazineLuizaPrice);
                    priceElement = await priceElementFiltrado.InnerTextAsync();
                    int startIndex = priceElement.IndexOf("R$") + 3; 
                    int endIndex = priceElement.IndexOf(" no Pix");
                    priceElement = priceElement.Substring(startIndex, endIndex - startIndex).Trim();
                }
            }

            if (priceElement != null)
            {                
                priceElement = priceElement.Replace(".", "").Trim();                

                if (double.TryParse(priceElement, out double price))
                {
                    return price;
                }
            }
            return null;
        }

        public static async Task<double?> GetPriceTeraByteShop(IPage page, string url)
        {
            string? priceElement = null;
            int elements = await page.Locator(LabelTerabytePrice).CountAsync();
            if (elements > 0)
            {
                priceElement = await page.Locator(LabelTerabytePrice).TextContentAsync();
            }

            if (priceElement != null)
            {
                priceElement = priceElement.Replace("R$", "").Trim();

                if (double.TryParse(priceElement, out double price))
                {
                    if (price == 0)
                    {
                        return null;
                    }
                    return price;
                }
            }
            return null;
        }

        public static async Task<double?> GetPriceNuuvem(IPage page, string url)
        {
            string? priceElement = null;
            string? priceElementInteiro = null;
            string? priceElementDecimal = null;
            int elements = await page.Locator(LabelNuuvemPriceInteger).CountAsync();
            if (elements > 0)
            {
                priceElementInteiro = await page.Locator(LabelNuuvemPriceInteger).TextContentAsync();
                priceElementDecimal = await page.Locator(LabelNuuvemPriceDecimal).TextContentAsync();
                priceElement = priceElementInteiro + "." + priceElementDecimal;
            }

            if (priceElement != null)
            {
                if (double.TryParse(priceElement, out double price))
                {
                    if (price == 0)
                    {
                        return null;
                    }
                    return price;
                }
            }
            return null;
        }

        public static async Task<double?> GetPriceGreenManGaming(IPage page, string url)
        {
            string? priceElement = null;
            int elements = await page.Locator(LabelGreenManGamingPrice).CountAsync();
            if (elements > 0)
            {
                elements = elements - 1;
                LabelGreenManGamingPrice = LabelGreenManGamingPrice + " >> nth = " + elements.ToString();
                priceElement = await page.Locator(LabelGreenManGamingPrice).TextContentAsync();
            }

            if (priceElement != null)
            {
                priceElement = priceElement.Replace("R$", "").Replace(",", "").Trim();

                if (double.TryParse(priceElement, out double price))
                {
                    price = price / 100;
                    return price;
                }
            }
            return null;
        }

        public static async Task<double?> GetPriceGOG(IPage page, string url)
        {
            string? priceElement = null;
            int elements = await page.Locator(LabelGOGPrice).CountAsync();
            if (elements > 0)
            {
                priceElement = await page.Locator(LabelGOGPrice).TextContentAsync();
            }

            if (priceElement != null)
            {
                priceElement = priceElement.Replace("R$", "").Trim();

                if (double.TryParse(priceElement, out double price))
                {
                    price = price / 100;
                    return price;
                }
            }
            return null;
        }

        public static async Task<double?> GetPriceEpic(IPage page, string url)
        {
            var scriptContent = await page.Locator(ScriptEpic).InnerTextAsync();
            var jsonObject = JsonDocument.Parse(scriptContent).RootElement;
            var firstOffer = jsonObject.GetProperty("offers")[0];
            var price = firstOffer.GetProperty("priceSpecification").GetProperty("price").GetDouble();
            return price;
        }

        public static async Task<double?> GetPriceXbox(IPage page, string url)
        {
            string? priceElement = null;
            Thread.Sleep(2000);
            await page.WaitForLoadStateAsync(LoadState.Load);
            int elements = await page.Locator(LabelXboxPrice).CountAsync();
            if (elements == 0)
            {
                Thread.Sleep(6000);
                elements = await page.Locator(LabelXboxPrice).CountAsync();
                if (elements > 0)
                {
                    priceElement = await page.Locator(LabelXboxPrice).TextContentAsync();
                }
            }
            else
            {
                priceElement = await page.Locator(LabelXboxPrice).TextContentAsync();
            }

            if (priceElement != null)
            {
                priceElement = priceElement.Replace("R$", "").Trim();

                if (double.TryParse(priceElement, out double price))
                {
                    return price;
                }
            }
            return null;
        }

        public static int? returnSteamIdapp(string url)
        {
            int? idApp = null;
            var uri = new Uri(url);

            var match = Regex.Match(uri.AbsolutePath, @"\/app\/(\d+)\/");

            if (match.Success)
            {
                idApp = int.Parse(match.Groups[1].Value);
                return idApp;
            }
            return null;
        }

        public static (string Email, string Password) ReadEmailCredentials()
        {
            var json = File.ReadAllText("email_credentials.json");
            var credentials = JsonSerializer.Deserialize<Dictionary<string, string>>(json);

            if (credentials == null)
            {
                Log("Falha ao deserializar as credenciais.");
                return (string.Empty, string.Empty);
            }
            if (!credentials.ContainsKey("email") || !credentials.ContainsKey("password"))
            {
                Log("Faltando 'email' ou 'password' nas credenciais.");
                return (string.Empty, string.Empty);
            }
            return (credentials["email"], credentials["password"]);
        }

        public static async Task<string?> SendEmail(string subject, string body, int user_id)
        {
            string erro = null;
            using var dbcontext = new AppDbContext();
            var (email, password) = ReadEmailCredentials();
            var productRepository = new ProductRepository(dbcontext);
            string recipient = productRepository.ReturnEmailRecipient(user_id);
            try
            {
                using MailMessage mailMessage = new()
                {
                    From = new MailAddress(email),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(new MailAddress(recipient));

                using SmtpClient smtpClient = new SmtpClient("smtp.office365.com", 587)
                {
                    Credentials = new NetworkCredential(email, password),
                    EnableSsl = true
                };
                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                    string recipientsList = string.Join(", ", recipient);
                    Log($"E-mail enviado para: {recipientsList}");
                    return erro;
                }
                catch (SmtpException ex)
                {
                    string recipientsList = string.Join(", ", recipient);
                    Log($"Erro ao enviar e-mail para {recipientsList}:\nCódigo de status: {ex.StatusCode}\nMensagem de erro: {ex.Message}\nMensagem de erro interna: {ex.InnerException?.Message}");
                    erro = ex.ToString();
                    return erro;
                }
            }
            catch (Exception ex)
            {
                Log("Erro ao enviar e-mail:");
                Log($"Mensagem de erro: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Log($"Mensagem de erro interna: {ex.InnerException.Message}");
                }
                erro = ex.ToString();
                return erro;
            }
        }

        public static void Log(string message)
        {
            File.AppendAllText("log.txt", $"{DateTime.Now}: {message}\n");
        }

        public static async Task FillEmptyProductNames()
        {
            using var dbcontext = new AppDbContext();
            var productRepository = new ProductRepository(dbcontext);
            var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.4692.99 Safari/537.36"
            });

            await context.RouteAsync("**/*", async route =>
            {
                var request = route.Request;
                var url = request.Url;

                if (url.EndsWith(".png") || url.EndsWith(".jpg") || url.EndsWith(".jpeg") || url.EndsWith(".gif") ||
    url.EndsWith(".js") || url.EndsWith(".css") || url.EndsWith(".woff") || url.EndsWith(".woff2") ||
    url.EndsWith(".ttf") || url.EndsWith(".eot") || url.EndsWith(".mp4") || url.EndsWith(".webm") ||
    url.EndsWith(".ogg"))
                {
                    await route.AbortAsync();
                    return;
                }

                await route.ContinueAsync();
            });

            var groupedProducts = productRepository.GetProductsGroupedByStoreIdWithNullNames();

            foreach (var storeGroup in groupedProducts)
            {
                var storeGroupLocal = storeGroup;
                var page = await context.NewPageAsync();

                try
                {
                    foreach (var product in storeGroupLocal.Value)
                    {
                        string Url = product.Url;
                        await page.GotoAsync(Url);
                        Store store = (Store)product.Store_Id;
                        string Name = await GetProductName(page, Url, store);
                        bool Unavaliable = false;
                        if (Name != null)
                        {
                            Name = Name.Trim();
                        }
                        if (Name is null)
                        {
                            Unavaliable = true;
                        }
                        productRepository.AlterProduct(product.Id, Name, product.Url, product.Store_Id, product.Current_Price, Unavaliable, DateTime.Now);
                        if (product.Name != null)
                        {
                            string logMessage = $"O Produto de ID {product.Id} foi atualizado com o nome {product.Name}";
                            await AddLogEntryAsync(LogType.ProductNameUpdate, logMessage);
                        }
                    }
                }
                finally
                {
                    await page.CloseAsync();
                }
            }
            await browser.CloseAsync();
            await browser.DisposeAsync();
        }

        public static async Task<string> GetFullUrlFromShortenedAmazonUrl(string shortenedUrl)
        {
            var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.4692.99 Safari/537.36"
            });

            var page = await context.NewPageAsync();

            try
            {
                var response = await page.GotoAsync(shortenedUrl);

                await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

                string fullUrl = page.Url;

                return fullUrl;
            }
            finally
            {
                await page.CloseAsync();
                await browser.CloseAsync();
                await browser.DisposeAsync();
            }
        }

        public static async Task UpdateProductPrices()
        {
            var playwright = await Playwright.CreateAsync();
            var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true});
            var context = await browser.NewContextAsync(new BrowserNewContextOptions
            {
                UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.4692.99 Safari/537.36"
            });

            await context.RouteAsync("**/*", async route =>
            {
                var request = route.Request;
                var url = request.Url;

                if (url.EndsWith(".png") || url.EndsWith(".jpg") || url.EndsWith(".jpeg") || url.EndsWith(".gif") ||
                    url.EndsWith(".woff") || url.EndsWith(".woff2") || url.EndsWith(".ttf") || url.EndsWith(".eot") ||
                    url.EndsWith(".mp4") || url.EndsWith(".webm") || url.EndsWith(".ogg"))
                {
                    await route.AbortAsync();
                    return;
                }
                await route.ContinueAsync();
            });

            using var dbcontext = new AppDbContext();
            var productRepository = new ProductRepository(dbcontext);
            var products = productRepository.GetProductsGroupedByStoreIdPendingPriceUpdate()
                                            .SelectMany(g => g.Value)
                                            .OrderBy(p => p.Last_Checked_Date)
                                            .Take(10)
                                            .ToList();

            int totalProducts = products.Count;
            int threads = 3;
            int productsPerThread = totalProducts / threads;
            int remainder = totalProducts % threads;

            var tasks = new List<Task>();

            for (int i = 0; i < threads; i++)
            {
                int start = i * productsPerThread;
                int count = productsPerThread + (i == threads - 1 ? remainder : 0); // A última thread pega o restante

                var productsSubset = products.Skip(start).Take(count).ToList();

                var task = Task.Run(async () =>
                {
                    using var dbcontext = new AppDbContext();
                    var productRepository = new ProductRepository(dbcontext);
                    var page = await context.NewPageAsync();

                    try
                    {
                        foreach (var product in productsSubset)
                        {
                            string url = product.Url;
                            await page.GotoAsync(url);
                            Store store = (Store)product.Store_Id;
                            double? price = await GetProductPrice(page, url, store);
                            bool unavailable = price == null;

                            productRepository.AlterProduct(product.Id, product.Name, product.Url, product.Store_Id, price, unavailable, DateTime.Now);

                            if (price != null)
                            {
                                string logMessage = $"O Produto de ID {product.Id} foi atualizado com o preço {price}";
                                await AddLogEntryAsync(LogType.ProductPriceUpdate, logMessage);
                            }
                        }
                    }
                    finally
                    {
                        await page.CloseAsync();
                    }
                });

                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            await browser.CloseAsync();
            await browser.DisposeAsync();
        }

        public static async Task CheckAndNotifyUsersAsync()
        {
            using (var context = new AppDbContext())
            {
                var query = (from p in context.Product
                             join up in context.User_Product on p.Id equals up.Product_id
                             join u in context.User on up.User_id equals u.Id
                             where p.Current_Price < up.Price && p.Active
                             select new
                             {
                                 Product = p,
                                 User_Product = up,
                                 User = u
                             }).ToList();

                foreach (var item in query)
                {
                    var userProduct = item.User_Product;
                    var user = item.User;

                    if (!userProduct.Last_notification.HasValue || userProduct.Last_notification < DateTime.Now.AddHours(-24))
                    {

                        var subject = $"Alerta de preço: Produto {item.Product.Name} abaixo de R${item.User_Product.Price}";
                        var body = $"O produto {item.Product.Name} na URL <a href='{item.Product.Url}' target='_blank'>{item.Product.Url}</a> está com um preço de R${item.Product.Current_Price}.";
                        string? erroEnvioEmail = await SendEmail(subject, body, item.User.Id);

                        string logMessage = null;
                        if (erroEnvioEmail == null)
                        {
                            logMessage = $"Email enviado referente ao produto {item.Product.Name} foi enviado para o e-mail {item.User.Email}";
                            await AddLogEntryAsync(LogType.EmailSent, logMessage);
                        }

                        else
                        {
                            await AddLogEntryAsync(LogType.EmailSent, $"Erro ao enviar e-mail para o e-mail {item.User.Email.ToString()}" + erroEnvioEmail.ToString());
                        }

                        string formattedPrice = string.Format(new System.Globalization.CultureInfo("pt-BR"), "{0:C}", item.Product.Current_Price);
                        string WhastappResponse = WhatsAppApiService.SendWhatsappMessage(user.Phone, "preco_abaixo_de", item.Product.Name.ToString(), item.Product.Url.ToString(), formattedPrice);
                        if (WhastappResponse.Contains("\"message_status\":\"accepted\""))
                        {
                            logMessage = $"Mensagem enviada referente ao produto {item.Product.Name} foi enviado para o whatsapp {item.User.Phone}";
                            await AddLogEntryAsync(LogType.WhatsappMessageSent, logMessage);
                        }
                        else
                        {
                            await AddLogEntryAsync(LogType.WhatsappMessageSent, WhastappResponse);
                        }

                        userProduct.Last_notification = DateTime.Now;

                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        public static async Task NotifyUsersWithWelcomeEmailAsync()
        {
            using (var context = new AppDbContext())
            {
                var usersWithoutWelcomeEmail = context.User
                    .Where(u => !u.WelcomeEmailSent)
                    .ToList();

                foreach (var user in usersWithoutWelcomeEmail)
                {
                    try
                    {
                        var subject = "Bem-vindo ao Quer Pagar Quanto!";
                        var body = $"Olá {user.Email},<br/><br/>" +
               "Bem-vindo ao Quer Pagar Quanto! Estamos felizes em tê-lo conosco.<br/><br/>" +
               "Para garantir que você receba os avisos de preços do nosso sistema, por favor, marque este e-mail como \"Não lixo eletrônico\".<br/>" +
               "Não se preocupe, nós não enviaremos e-mails de promoção ou similares, e seus dados não serão repassados a terceiros.<br/><br/>" +
               "Atenciosamente,<br/>" +
               "Equipe Quer Pagar Quanto";

                        await SendEmail(subject, body, user.Id);

                        string logMessage = $"E-mail de boas-vindas enviado para {user.Email}";
                        await AddLogEntryAsync(LogType.EmailSent, logMessage);
                        user.WelcomeEmailSent = true;
                        await context.SaveChangesAsync();
                    }
                    catch (Exception ex)
                    {
                        string logMessage = $"Erro ao enviar e-mail para {user.Email} {ex.ToString()}";
                        await AddLogEntryAsync(LogType.EmailSent, logMessage);
                        await context.SaveChangesAsync();
                    }
                }
            }
        }

        public static async Task NotifyUsersWithWelcomeWhatsappAsync()
        {
            using (var context = new AppDbContext())
            {
                var usersWithoutWelcomeEmail = context.User
                    .Where(u => !u.WelcomeWhatsappSent && u.Phone != null)
                    .ToList();

                foreach (var user in usersWithoutWelcomeEmail)
                {
                    string returnMessage = WhatsAppApiService.SendWhatsappMessage(user.Phone, "cadastro_aplicativo");
                    user.WelcomeWhatsappSent = true;
                    await context.SaveChangesAsync();
                    await AddLogEntryAsync(LogType.WhatsappMessageSent, "Mensagem de Cadastro no Aplicativo" + returnMessage);
                }
            }
        }

        public static async Task AddLogEntryAsync(LogType logType, string description)
        {
            using (var context = new AppDbContext())
            {
                var logEntry = new Log
                {
                    LogTypeId = (int)logType,
                    Description = description,
                    DateTime = DateTime.Now
                };
                context.Log.Add(logEntry);
                await context.SaveChangesAsync();
            }
        }

        public static int InsertExecutionLogStart()
        {
            using (var context = new AppDbContext())
            {
                var log = new ExecutionLog
                {
                    StartDate = DateTime.Now,
                    Status = "START",
                    ErrorMessage = null,
                    EndDate = null
                };

                context.ExecutionLog.Add(log);
                context.SaveChanges();

                return log.Id;
            }
        }

        public static void UpdateExecutionLogSuccess(int id)
        {
            using (var context = new AppDbContext())
            {
                var log = context.ExecutionLog.FirstOrDefault(l => l.Id == id);

                if (log != null)
                {
                    log.EndDate = DateTime.Now;
                    log.Status = "SUCCESS";

                    context.SaveChanges();
                }
            }
        }
        public static void UpdateExecutionLogError(int id, string errorMessage)
        {
            using (var context = new AppDbContext())
            {
                var log = context.ExecutionLog.FirstOrDefault(l => l.Id == id);

                if (log != null)
                {
                    log.EndDate = DateTime.Now;
                    log.Status = "ERROR";
                    log.ErrorMessage = errorMessage;

                    context.SaveChanges();
                }
            }
        }

        public async static Task ExpandAndSaveShortenedUrls()
        {
            using var dbcontext = new AppDbContext();
            var productRepository = new ProductRepository(dbcontext);
            using (var _context = new AppDbContext())
            {
                var shortenedUrls = productRepository.GetShortenedAmazonUrls();
                if (shortenedUrls.Count > 0)
                {
                    var playwright = await Playwright.CreateAsync();
                    var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
                    var context = await browser.NewContextAsync(new BrowserNewContextOptions
                    {
                        UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/126.0.4692.99 Safari/537.36"
                    });

                    try
                    {
                        var page = await context.NewPageAsync();

                        foreach (var shortenedUrl in shortenedUrls)
                        {
                            string expandedUrl = await GetFullUrlFromShortenedAmazonUrl(shortenedUrl);
                            var product = _context.Product.FirstOrDefault(p => p.Url == shortenedUrl);
                            if (product != null)
                            {
                                productRepository.AlterProduct(product.Id, product.Name, expandedUrl, product.Store_Id, product.Current_Price, product.Unavailable, DateTime.Now);
                            }
                        }
                    }

                    finally
                    {
                        await context.CloseAsync();
                        await browser.DisposeAsync();
                    }
                }
            }
        }

        public static void CloseChromiumAndNodeProcesses()
        {
            try
            {
                foreach (var process in Process.GetProcessesByName("chromium"))
                {
                    process.Kill();
                }

                foreach (var process in Process.GetProcessesByName("node"))
                {
                    process.Kill();
                }
            }
            catch (Exception ex)
            {
                Log(ex.InnerException.ToString());
            }
        }

        public static async Task CheckLogsAndRestartIfNeeded()
        {
            using var dbcontext = new AppDbContext();
            var executionLogs = dbcontext.ExecutionLog
                .OrderByDescending(log => log.Id)
                .Take(5)
                .ToList();

            if (executionLogs.All(log => log.Status == "ERROR"))
            {
                RestartApplication();
            }
        }

        // Método para reiniciar a aplicação
        private static void RestartApplication()
        {

            using (var context = new AppDbContext())
            {
                var log = new ExecutionLog
                {
                    StartDate = DateTime.Now,
                    Status = "RESTART",
                    ErrorMessage = null,
                    EndDate = DateTime.Now
                };

                context.ExecutionLog.Add(log);
                context.SaveChanges();
            }

            var processStartInfo = new ProcessStartInfo
            {
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                UseShellExecute = true,
                CreateNoWindow = false
            };

            Process.Start(processStartInfo);
            Process.GetCurrentProcess().Kill();
        }
        public static string Aplication_Folder
        {
            get
            {
                if (string.IsNullOrEmpty(AplicationFolder))
                {                   
                    AplicationFolder = Path.GetFullPath(@"..\..\..\bin\Release");
                }
                return AplicationFolder;
            }
        }

        public static string CreateFolder(string nomePasta)
        {            
            var nomeArquivo = Aplication_Folder + $"\\{nomePasta}";
            if (!Directory.Exists(nomeArquivo))
            {
                Directory.CreateDirectory(nomeArquivo);
            }
            return nomeArquivo;
        }

        public static async Task TakeScreenshot(IPage page)
        {
            CreateFolder("TestScreenShots");
                        
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            string screenshotPath = $"{Aplication_Folder}\\TestScreenShots\\screenshot_{timestamp}.png";

            await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath });
        }

    }
}



