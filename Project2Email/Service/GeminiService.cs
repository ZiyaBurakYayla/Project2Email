using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Project2Email.Dtos;
using System.Text;

namespace Project2Email.Services
{
    public class GeminiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        public GeminiService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<MailAnalysisResult> AnalyzeEmailAsync(string subject, string content)
        {
            var apiKey = _configuration["Gemini:ApiKey"];

            if (string.IsNullOrEmpty(apiKey))
            {
                throw new Exception("API Anahtarı okunamadı! Lütfen secret.json dosyasını ve Program.cs ayarını kontrol edin.");
            }

            var prompt = $@"Aşağıdaki e-postayı analiz et:
            Konu: {subject}
            İçerik: {content}

            Senden şunları bekliyorum:
            1. Bu maili şu kategorilerden birine ata: [İş, Sosyal, Finans, Tanıtım, Önemli].
            2. Mail içeriğini en fazla 10 kelime ile özetle.
            3. Çıktıyı SADECE aşağıdaki JSON formatında ver:
            {{ ""CategoryName"": ""..."", ""Summary"": ""..."" }}";

            var requestBody = new
            {
                contents = new[] { new { parts = new[] { new { text = prompt } } } }
            };

            var jsonRequest = JsonConvert.SerializeObject(requestBody);
            var contentString = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"{_apiUrl}?key={apiKey}", contentString);

            if (!response.IsSuccessStatusCode)
            {
                var errorDetail = await response.Content.ReadAsStringAsync();
                throw new Exception($"API Hatası ({response.StatusCode}): {errorDetail}");
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            dynamic result = JsonConvert.DeserializeObject(jsonResponse);

            string rawJson = result?.candidates?[0]?.content?.parts?[0]?.text;

            if (string.IsNullOrEmpty(rawJson))
            {
                return new MailAnalysisResult { CategoryName = "Genel", Summary = "AI cevap veremedi." };
            }

            rawJson = rawJson.Replace("```json", "").Replace("```", "").Trim();
            return JsonConvert.DeserializeObject<MailAnalysisResult>(rawJson);
        }
    }
}