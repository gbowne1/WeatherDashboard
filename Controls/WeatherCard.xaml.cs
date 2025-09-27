using System.Windows.Controls;
using WeatherDashboard.Models;

namespace WeatherDashboard.Controls
{
    public partial class WeatherCard : UserControl
    {
        public WeatherCard()
        {
            InitializeComponent();
        }

        public WeatherData Weather
        {
            get => (WeatherData)GetValue(WeatherProperty);
            set => SetValue(WeatherProperty, value);
        }

        public static readonly DependencyProperty WeatherProperty =
            DependencyProperty.Register("Weather", typeof(WeatherData), typeof(WeatherCard), new PropertyMetadata(null));
    }
}