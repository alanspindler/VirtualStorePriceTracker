using RestSharp;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.Text;
using Database;

namespace API
{
    public class PriceOverview
    {
        public int final { get; set; }
        public string currency { get; set; }
    }

    public class Data
    {
        public PriceOverview price_overview { get; set; }

        public string name { get; set; }
    }

    public class AppData
    {
        public bool success { get; set; }
        public Data data { get; set; }
    }

    public class TokenService
    {
        private AppDbContext _context;
        public static string GetDecryptedAccessToken()
        {
            var _context = new AppDbContext();

            var configEntry = _context.Config
                                      .Where(c => c.Key == "AccessToken")
                                      .Select(c => c.Value)
                                      .FirstOrDefault();

            if (!string.IsNullOrEmpty(configEntry))
            {
                return DecryptToken(configEntry);
            }

            throw new Exception("Access token not found or is empty in the database.");
        }

        private static string DecryptToken(string encryptedToken)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Encoding.UTF8.GetBytes("7295544213221451");
                aesAlg.IV = Encoding.UTF8.GetBytes("1377442112234433");

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (var msDecrypt = new System.IO.MemoryStream(Convert.FromBase64String(encryptedToken)))
                {
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (var srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }

    public static class StemAPI
    {
        public static async Task<double?> GetSteamPrice(string appId)
        {
            var options = new RestClientOptions($"https://store.steampowered.com/api/appdetails?appids={appId}&cc=bra");
            var client = new RestClient(options);
            var request = new RestRequest("", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            if (response != null)
            {
                var steamResponse = JsonConvert.DeserializeObject<Dictionary<string, AppData>>(response.Content);

                if (steamResponse != null)
                {
                    foreach (var item in steamResponse)
                    {
                        appId = item.Key;
                        var appData = item.Value;
                        if (appData != null && appData.success)
                        {
                            var currency = appData.data.price_overview.currency;
                            if (currency != "BRL")
                            {
                                return null;
                            }
                            else
                            {
                                double? finalPrice = appData.data.price_overview.final;
                                finalPrice = finalPrice / 100;
                                return finalPrice;
                            }
                        }
                        return null;
                    }
                    return null;
                }
                return null;
            }
            return null;
        }

        public static async Task<string?> GetSteamAppName(string appId)
        {
            var options = new RestClientOptions($"https://store.steampowered.com/api/appdetails?appids={appId}");
            var client = new RestClient(options);
            var request = new RestRequest("", Method.Get);
            RestResponse response = await client.ExecuteAsync(request);
            if (response != null)
            {
                var steamResponse = JsonConvert.DeserializeObject<Dictionary<string, AppData>>(response.Content);
                if (steamResponse != null)
                {
                    foreach (var item in steamResponse)
                    {
                        appId = item.Key;
                        var appData = item.Value;

                        if (appData != null && appData.success)
                        {
                            var name = appData.data.name;
                            return name;
                        }
                        return null;
                    }
                    return null;
                }
                return null;
            }
            return null;
        }
    }

    public class WhatsAppApiService(AppDbContext context)
    {
        public static string SendWhatsappMessage(string phoneNumber, string templateName, string parameter1 = null, string parameter2 = null, string parameter3 = null)
        {
            string? AccessToken = TokenService.GetDecryptedAccessToken();
            var client = new RestClient("https://graph.facebook.com/v20.0/424839010708851/messages");
            var request = new RestRequest("", Method.Post);

            request.AddHeader("Authorization", $"Bearer {AccessToken}");
            request.AddHeader("Content-Type", "application/json");

            var body = new
            {
                messaging_product = "whatsapp",
                to = phoneNumber,
                type = "template",
                template = new
                {
                    name = templateName,
                    language = new
                    {
                        code = "pt_BR"
                    },
                    components = new List<object>()
                }
            };

            var bodyComponents = new List<object>();

            if (parameter1 != null)
            {
                bodyComponents.Add(new { type = "text", text = parameter1 });
            }

            if (parameter2 != null)
            {
                bodyComponents.Add(new { type = "text", text = parameter2 });
            }

            if (parameter3 != null)
            {
                bodyComponents.Add(new { type = "text", text = parameter3 });
            }

            if (bodyComponents.Any())
            {
                body.template.components.Add(new
                {
                    type = "body",
                    parameters = bodyComponents
                });
            }

            request.AddJsonBody(body);

            var response = client.Execute(request);

            return response.Content;
        }
    }
}


