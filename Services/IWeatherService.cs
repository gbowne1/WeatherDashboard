using System.Collections.Generic;
using System.Threading.Tasks;
using WeatherDashboard.Models;

namespace WeatherDashboard.Services
{
    public interface IWeatherService
    {
        Task<List<WeatherData>> GetWeatherAsync(string location, string apiName);
        Task<List<string>> GetAvailableApisAsync();
    }
}
