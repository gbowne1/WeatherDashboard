using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WeatherDashboard.Models;
using Newtonsoft.Json.Linq;
using System.IO;
using Microsoft.EntityFrameworkCore;

namespace WeatherDashboard.Services
{
    public class WeatherService : IWeatherService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly WeatherDbContext _dbContext;

        public WeatherService(IConfiguration configuration, HttpClient httpClient, WeatherDbContext dbContext)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _dbContext = dbContext;
        }

        public async Task<List<WeatherData>> GetWeatherAsync(string location, string apiName)
        {
            var apiConfig = _configuration.GetSection($"WeatherApis:{apiName}");
            string baseUrl = apiConfig["BaseUrl"];
            string apiKey = apiConfig["ApiKey"];
            string url = BuildApiUrl(baseUrl, location, apiKey);

            try
            {
                var response = await _httpClient.GetStringAsync(url);
                var weatherData = ParseWeatherResponse(response, apiName);
                weatherData.ApiSource = apiName;
                weatherData.Timestamp = DateTime.UtcNow;

                await _dbContext.WeatherData.AddAsync(weatherData);
                await _dbContext.SaveChangesAsync();

                await LogWeatherData(weatherData);
                return new List<WeatherData> { weatherData };
            }
            catch (Exception ex)
            {
                await LogError(ex);
                return new List<WeatherData>();
            }
        }

        public async Task<List<string>> GetAvailableApisAsync()
        {
            var apis = new List<string>();
            var weatherApisSection = _configuration.GetSection("WeatherApis");
            foreach (var api in weatherApisSection.GetChildren())
            {
                apis.Add(api.Key);
            }
            return await Task.FromResult(apis);
        }

        private string BuildApiUrl(string baseUrl, string location, string apiKey)
        {
            // Example for different APIs; extend as needed
            if (baseUrl.Contains("openweathermap.org"))
                return $"{baseUrl}?q={location}&appid={apiKey}&units=imperial";
            if (baseUrl.Contains("weatherapi.com"))
                return $"{baseUrl}?key={apiKey}&q={location}";
            if (baseUrl.Contains("open-meteo.com"))
                return $"{baseUrl}?latitude=52.52&longitude=13.41&current_weather=true"; // Adjust for location
            return baseUrl;
        }

        private WeatherData ParseWeatherResponse(string response, string apiName)
        {
            var weatherData = new WeatherData();
            var json = JObject.Parse(response);

            if (apiName == "OpenWeatherMap")
            {
                weatherData.Location = json["name"]?.ToString();
                weatherData.Temperature = json["main"]?["temp"]?.ToObject<double>() ?? 0;
                weatherData.Description = json["weather"]?[0]?["description"]?.ToString();
                weatherData.Humidity = json["main"]?["humidity"]?.ToObject<double>() ?? 0;
                weatherData.WindSpeed = json["wind"]?["speed"]?.ToObject<double>() ?? 0;
            }
            else if (apiName == "WeatherApi")
            {
                weatherData.Location = json["location"]?["name"]?.ToString();
                weatherData.Temperature = json["current"]?["temp_f"]?.ToObject<double>() ?? 0;
                weatherData.Description = json["current"]?["condition"]?["text"]?.ToString();
                weatherData.Humidity = json["current"]?["humidity"]?.ToObject<double>() ?? 0;
                weatherData.WindSpeed = json["current"]?["wind_mph"]?.ToObject<double>() ?? 0;
            }
            // Add parsing for other APIs as needed
            return weatherData;
        }

        private async Task LogWeatherData(WeatherData data)
        {
            var logPath = _configuration["Logging:LogFilePath"];
            var logMessage = $"{DateTime.UtcNow}: Fetched weather for {data.Location} from {data.ApiSource}: {data.Temperature}°F, {data.Description}\n";
            await File.AppendAllTextAsync(logPath, logMessage);
        }

        private async Task LogError(Exception ex)
        {
            var logPath = _configuration["Logging:LogFilePath"];
            var logMessage = $"{DateTime.UtcNow}: Error - {ex.Message}\n";
            await File.AppendAllTextAsync(logPath, logMessage);
        }
    }
}
