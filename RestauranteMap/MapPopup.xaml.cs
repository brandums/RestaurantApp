using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using Mopups.Pages;
using Mopups.Services;


namespace RestauranteMap;

public partial class MapPopup : PopupPage
{
    public event Action<string> OnLocationSelected;

    public MapPopup()
    {
        InitializeComponent();
        InitializeMap();
    }

    private async void InitializeMap()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status == PermissionStatus.Granted)
            {
                var location = await Geolocation.GetLastKnownLocationAsync();

                if (location != null)
                {
                    MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                        new Location(location.Latitude, location.Longitude),
                        Distance.FromMiles(1)));
                }
                else
                {
                    MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                        new Location(-16.5000, -68.1193),
                        Distance.FromMiles(5)));
                }
            }
            else
            {
                MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                    new Location(-16.5000, -68.1193),
                    Distance.FromMiles(5)));
            }
        }
        catch (Exception ex)
        {
            MyMap.MoveToRegion(MapSpan.FromCenterAndRadius(
                new Location(-16.5000, -68.1193),
                Distance.FromMiles(5)));
        }

        MyMap.MapClicked += OnMapClicked;
    }

    private void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        MyMap.Pins.Clear();
        var pin = new Pin
        {
            Label = "Ubicación seleccionada",
            Location = e.Location
        };
        MyMap.Pins.Add(pin);
    }

    private void OnAcceptClicked(object sender, EventArgs e)
    {
        if (MyMap.Pins.Count > 0)
        {
            var selectedPin = MyMap.Pins[0];
            string location = $"{selectedPin.Location.Latitude}, {selectedPin.Location.Longitude}";

            OnLocationSelected?.Invoke(location);
            MopupService.Instance.PopAsync();
        }
    }
}