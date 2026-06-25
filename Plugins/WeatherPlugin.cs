using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace WeatherAI.Plugins
{
    public class WeatherPlugin
    {
        private readonly HttpClient _http;//http client request atmak için
        private readonly string _apiKey;//api keyi tutmak için

        public WeatherPlugin(IHttpClientFactory factory, IConfiguration config)
        {
            // Factory'den bir HttpClient örneği alıyoruz
            _http = factory.CreateClient();

            // appsettings.json içindeki "OpenWeather:ApiKey" değerini okuyoruz
            _apiKey = config["OpenWeather:ApiKey"]!;
        }

        // güncel hava kontrolü fonksiyonu
        [KernelFunction, Description("Belirtilen şehir için güncel hava durumunu getirir")]
        public async Task<string> GetCurrentWeatherAsync(
            // LLM kullanıcının mesajından şehri çıkarıp buraya yazar
            [Description("Şehir adı, örn: İstanbul, İzmir, Manisa")] string city)
        {
            // API isteği için URL oluşturuyoruz
            // q şehir adına göre sorgula
            // appid kimlik doğrulama için API key
            // units sıcaklık Celsius cinsinden gelsin
            // lang=tr  açıklamalar Türkçe gelsin 
            var url = $"https://api.openweathermap.org/data/2.5/weather" +
                      $"?q={Uri.EscapeDataString(city)}&appid={_apiKey}&units=metric&lang=tr";

            // GET isteği at, cevabı string olarak al
            var json = await _http.GetStringAsync(url);

            // gelen JSON string'i parse ediyoruz
            var data = JsonDocument.Parse(json).RootElement;

            //  verileri JSON 'dan çekiyoruz
            var desc = data.GetProperty("weather")[0].GetProperty("description").GetString();
            var temp = data.GetProperty("main").GetProperty("temp").GetDouble();
            var feels = data.GetProperty("main").GetProperty("feels_like").GetDouble();
            var humidity = data.GetProperty("main").GetProperty("humidity").GetInt32();
            var wind = data.GetProperty("wind").GetProperty("speed").GetDouble();

            // Tüm verileri birleştirip tek string olarak döndürüyoruz
            // Bu string LLM'e gidecek, o da bunu kullanıcıya Türkçe yorumlayacak
            return $"{city}: {desc}, sıcaklık {temp}°C (hissedilen {feels}°C), " +
                   $"nem %{humidity}, rüzgar {wind} m/s";
        }

        // 24 saat içinde hava nasıl fonksiyonu
        [KernelFunction, Description("Belirtilen şehir için 24 saatlik hava tahmini getirir")]
        public async Task<string> GetForecastAsync(
            [Description("Şehir adı")] string city)
        {
            // cnt=8 8*3 saatlik dilim 24 saatlik tahmin demek
            var url = $"https://api.openweathermap.org/data/2.5/forecast" +
                      $"?q={Uri.EscapeDataString(city)}&appid={_apiKey}&units=metric&lang=tr&cnt=8";

            var json = await _http.GetStringAsync(url);
            var data = JsonDocument.Parse(json).RootElement;

            // StringBuilder: string'leri tek tek birleştirmek yerine daha verimli şekilde biriktirmek için kullanılır
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"{city} için tahmin:");

            // list dizisindeki her eleman bir zaman dilimi
            foreach (var item in data.GetProperty("list").EnumerateArray())
            {
                var time = item.GetProperty("dt_txt").GetString();
                var desc = item.GetProperty("weather")[0].GetProperty("description").GetString();
                var temp = item.GetProperty("main").GetProperty("temp").GetDouble();

                sb.AppendLine($"  {time}: {desc}, {temp}°C");
            }

            // Tüm satırları birleştirip LLM'e gönderiyoruz
            return sb.ToString();



        }
    }
}
