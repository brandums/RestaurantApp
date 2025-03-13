using RestauranteMap.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RestauranteMap;

public partial class Cocina : ContentPage, INotifyPropertyChanged
{
    private int userId;
    private readonly StructureService _structureService;
    private ObservableCollection<OrdenPorUser> _carouselOrders;
    public ObservableCollection<OrdenPorUser> CarouselOrders
    {
        get => _carouselOrders;
        set
        {
            if (_carouselOrders != value)
            {
                _carouselOrders = value;
                OnPropertyChanged(nameof(CarouselOrders));
            }
        }
    }

    private ObservableCollection<Platos> _platosList;
    public ObservableCollection<Platos> platosList
    {
        get => _platosList;
        set
        {
            if (_platosList != value)
            {
                _platosList = value;
                OnPropertyChanged(nameof(platosList));
            }
        }
    }

    private Platos _plato;
    public Platos plato
    {
        get => _plato;
        set
        {
            if (_plato != value)
            {
                _plato = value;
                OnPropertyChanged(nameof(plato));
            }
        }
    }

    public Cocina()
    {
        InitializeComponent();
        _structureService = new StructureService();
        CarouselOrders = new ObservableCollection<OrdenPorUser>();
        platosList = new ObservableCollection<Platos>();
        plato = new Platos();

        StartLabelAnimation();

        BindingContext = this;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadOrders();
    }


    private async void LoadOrders()
    {
        try
        {
            var orders = await _structureService.GetOrders(1);
            CarouselOrders.Clear();
            CarouselOrders = new ObservableCollection<OrdenPorUser>(orders);

            platosList = new ObservableCollection<Platos>(orders[0].Platos);
            plato = platosList[0];
            userId = orders[0].UserId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar pedidos: {ex.Message}");
        }
    }

    private void selectPlatosListTapped(object sender, EventArgs e)
    {
        if (sender is Frame frame && frame.BindingContext is OrdenPorUser selectedOrder)
        {
            platosList = new ObservableCollection<Platos>(selectedOrder.Platos);
            plato = platosList[0];
            userId = selectedOrder.UserId;
        }
    }

    private async void sendOrder_Clicked(object sender, EventArgs e)
    {
        var orden = CarouselOrders.FirstOrDefault(o => o.UserId == userId);

        if (orden.Platos == null || !orden.Platos.Any())
        {
            await DisplayAlert("Error", "No hay platos para actualizar.", "OK");
            return;
        }

        bool success = await _structureService.UpdatePlatosStatus(2, orden);

        if (success)
        {
            LoadOrders();
            await DisplayAlert("Éxito", "El estado de los platos se actualizó correctamente.", "OK");
        }
        else
        {
            await DisplayAlert("Error", "Hubo un problema al actualizar el estado de los platos.", "OK");
        }
    }

    private async void StartLabelAnimation()
    {
        while (true)
        {
            await animatedLabel.ScaleTo(1.2, 1000, Easing.Linear);
            await animatedLabel.ScaleTo(1.0, 1000, Easing.Linear);
        }
    }

    private void selectPlatoTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is Platos platoSeleccionado)
        {
            plato = platoSeleccionado;
        }
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("///MainPage");
        return true;
    }







    private void CerrarPopup(object sender, TappedEventArgs e)
    {
        PopupReceta.IsVisible = false;
    }

    private void MostrarReceta(object sender, TappedEventArgs e)
    {
        var receta = plato.Receta;
        DynamicContent.Children.Clear();

        for (int i = 0; i < receta.Orden.Length; i++)
        {
            string item = receta.Orden[i];

            if (item.StartsWith("txt"))
            {
                int index = int.Parse(item.Substring(3)) - 1;
                if (index >= 0 && index < receta.Texto.Length)
                {
                    DynamicContent.Children.Add(new Label
                    {
                        Text = receta.Texto[index],
                        FontSize = 14,
                        Margin = new Thickness(0)
                    });
                }
            }
            else if (item.StartsWith("sub"))
            {
                int index = int.Parse(item.Substring(3)) - 1;
                if (index >= 0 && index < receta.Subtitulo.Length)
                {
                    DynamicContent.Children.Add(new Label
                    {
                        Text = receta.Subtitulo[index],
                        FontSize = 16,
                        FontAttributes = FontAttributes.Bold,
                        Margin = new Thickness(0, 0, 0, 5)
                    });
                }
            }
            else if (item.StartsWith("img"))
            {
                int index = int.Parse(item.Substring(3)) - 1;
                if (index >= 0 && index < receta.Imagen.Length)
                {
                    DynamicContent.Children.Add(new Image
                    {
                        Source = receta.Imagen[index],
                        HeightRequest = 150,
                        Margin = new Thickness(10, 5)
                    });
                }
            }
        }

        PopupReceta.IsVisible = true;
    }

}