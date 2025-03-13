using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace RestauranteMap;

public partial class Map : ContentPage
{
    public Map()
    {
        InitializeComponent();
        LoadMap();
    }

    private async void LoadMap()
    {
        try
        {
            var location = await Geolocation.GetLastKnownLocationAsync();

            if (location == null)
            {
                location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.High,
                    Timeout = TimeSpan.FromSeconds(30)
                });
            }

            if (location != null)
            {
                var currentPosition = new Location(location.Latitude, location.Longitude);
                MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(currentPosition, Distance.FromKilometers(1)));

                var pin = new Pin
                {
                    Label = "Tu ubicaci�n actual",
                    Address = "Aqu� est�s",
                    Type = PinType.Place,
                    Location = currentPosition
                };
                MyMap.Pins.Add(pin);
            }
            else
            {
                await DisplayAlert("Error", "No se pudo obtener la ubicaci�n.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al obtener la ubicaci�n: {ex.Message}", "OK");
        }
    }
}