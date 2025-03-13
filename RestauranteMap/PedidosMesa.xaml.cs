using RestauranteMap.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace RestauranteMap;

public partial class PedidosMesa : ContentPage, INotifyPropertyChanged
{
    public ICommand RefreshCommand { get; }

    private readonly StructureService _structureService;
    private int NumeroDeMesas;
    private ObservableCollection<OrdenPorUser> _Orders;
    public ObservableCollection<OrdenPorUser> Orders
    {
        get => _Orders;
        set
        {
            if (_Orders != value)
            {
                _Orders = value;
                OnPropertyChanged(nameof(Orders));
            }
        }
    }

    private bool isRefreshing;
    public bool IsRefreshing
    {
        get => isRefreshing;
        set
        {
            if (isRefreshing != value)
            {
                isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }
    }

    public PedidosMesa()
    {
        InitializeComponent();

        _structureService = new StructureService();
        Orders = new ObservableCollection<OrdenPorUser>();
        NumeroDeMesas = _structureService.Mesas;

        RefreshCommand = new Command(RefreshOrders);
        LoadInitialOrders();
        GenerarMesas();

        BindingContext = this;
    }

    protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public event PropertyChangedEventHandler PropertyChanged;

    private async void LoadInitialOrders()
    {
        try
        {
            var ordersList = await _structureService.GetOrders(2);
            Orders.Clear();
            Orders = new ObservableCollection<OrdenPorUser>(ordersList);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar pedidos: {ex.Message}");
        }
    }

    private void RefreshOrders()
    {
        IsRefreshing = true;

        LoadInitialOrders();

        IsRefreshing = false;
    }

    private void GenerarMesas()
    {
        MesasContainer.Children.Clear();

        MesasContainer.ColumnDefinitions.Clear();
        for (int i = 0; i < 4; i++)
        {
            MesasContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Star });
        }

        int filas = (int)Math.Ceiling((double)NumeroDeMesas / 4);

        MesasContainer.RowDefinitions.Clear();
        for (int i = 0; i < filas; i++)
        {
            MesasContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        }

        for (int i = 0; i < NumeroDeMesas; i++)
        {
            var label = new Label
            {
                Text = (i + 1).ToString(),
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center,
                FontSize = 18,
                TextColor = Colors.Black
            };

            var frame = new Frame
            {
                BackgroundColor = Colors.White,
                BorderColor = Colors.Black,
                CornerRadius = 5,
                Margin = new Thickness(5),
                Content = label
            };

            var tapGesture = new TapGestureRecognizer();
            tapGesture.Tapped += (s, e) =>
            {
                OnMesaSelected(int.Parse(label.Text));
            };

            frame.GestureRecognizers.Add(tapGesture);

            MesasContainer.Children.Add(frame);
            Grid.SetColumn(frame, i % 4);
            Grid.SetRow(frame, i / 4);
        }
    }

    private async void OnMesaSelected(int mesaNumero)
    {
        await Shell.Current.GoToAsync($"///Pedidos?mesa={mesaNumero}");
    }

    private async void UpdateStateOrder(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var orden = (OrdenPorUser)button.CommandParameter;
        orden.DeliveryId = _structureService.User.Id;

        if (orden.Platos == null || !orden.Platos.Any())
        {
            await DisplayAlert("Error", "No hay platos para actualizar.", "OK");
            return;
        }

        bool success = await _structureService.UpdatePlatosStatus(3, orden);

        if (success)
        {
            LoadInitialOrders();
            await DisplayAlert("Éxito", "El estado de los platos se actualizó correctamente.", "OK");
        }
        else
        {
            await DisplayAlert("Error", "Hubo un problema al actualizar el estado de los platos.", "OK");
        }
    }

    private void OnSearchTextChanged(object sender, TextChangedEventArgs e)
    {
        var searchText = e.NewTextValue?.ToLower().Trim() ?? string.Empty;

        foreach (var child in MesasContainer.Children)
        {
            if (child is Frame frame && frame.Content is Label label)
            {
                frame.BorderColor = Colors.Black;
                label.TextColor = Colors.Black;
            }
        }

        if (string.IsNullOrWhiteSpace(searchText)) return;

        var matchingOrders = Orders
            .Where(order => order.NameMesa?.ToLower().Contains(searchText) == true)
            .ToList();

        foreach (var order in matchingOrders)
        {
            var matchingFrames = MesasContainer.Children.OfType<Frame>()
                .Where(frame => frame.Content is Label label && label.Text == order.NumeroMesa.ToString());

            foreach (var frame in matchingFrames)
            {
                if (frame.Content is Label label)
                {
                    frame.BorderColor = Colors.Cyan;
                    label.TextColor = Colors.Cyan;
                }
            }
        }
    }


    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("///MainPage");
        return true;
    }
}