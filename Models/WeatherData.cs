using System;

namespace WeatherDashboard.Models
{
    public class WeatherData
    {
        public int Id { get; set; }
        public string Location { get; set; }
        public double Temperature { get; set; }
        public string Description { get; set; }
        public double Humidity { get; set; }
        public double WindSpeed { get; set; }
        public DateTime Timestamp { get; set; }
        public string ApiSource { get; set; }
    }
}
