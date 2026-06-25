namespace WeatherApp.Models
{
    public class WeatherData
    {
        public string City { get; set; } = "";
        public string Description { get; set; } = "";
        public double Temp { get; set; }
        public double FeelsLike { get; set; }
        public int Humidity { get; set; }
        public double WindSpeed { get; set; }
        public DateTime Date { get; set; }
    }
}