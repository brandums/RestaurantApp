using Mopups.Services;
using RestauranteMap.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace RestauranteMap;

[QueryProperty(nameof(MesaSeleccionada), "mesa")]
public partial class Pedidos : ContentPage, INotifyPropertyChanged
{
    public OrdenPorUser Orden { get; set; }
    private readonly StructureService _structureService;
    public ICommand IncrementCommand { get; private set; }
    public ICommand DecrementCommand { get; private set; }

    private Models.User currentUser;
    private int NumeroDeMesas;

    private int _mesaSeleccionada;
    private bool _isSettingMesaSeleccionada = false;

    public int MesaSeleccionada
    {
        get => _mesaSeleccionada;
        set
        {
            if (_isSettingMesaSeleccionada) return;
            _isSettingMesaSeleccionada = true;

            if (int.TryParse(value.ToString(), out var result))
            {
                if (result != 0)
                {
                    _mesaSeleccionada = result;
                    OnMesaSelected(_mesaSeleccionada);
                }
                else
                {
                    _mesaSeleccionada = 0;
                }
            }
            else
            {
                _mesaSeleccionada = 0;
            }

            _isSettingMesaSeleccionada = false;
        }
    }



    private PrincipalStruct currentPrincipalStruct;
    private string metodoEntrega;

    public ObservableCollection<Category> _categories { get; set; }
    public ObservableCollection<Category> categories
    {
        get => _categories;
        set
        {
            if (_categories != value)
            {
                _categories = value;
                OnPropertyChanged(nameof(categories));
            }
        }
    }
    public ObservableCollection<Platos> _shopsList { get; set; }
    public ObservableCollection<Platos> shopsList
    {
        get => _shopsList;
        set
        {
            if (_shopsList != value)
            {
                if (_shopsList != null)
                    _shopsList.CollectionChanged -= OnShopsListChanged;

                _shopsList = value;

                if (_shopsList != null)
                    _shopsList.CollectionChanged += OnShopsListChanged;

                OnPropertyChanged(nameof(shopsList));
                UpdatePedidosList();
            }
        }
    }

    private ObservableCollection<Platos> _pedidosList = new ObservableCollection<Platos>();
    public ObservableCollection<Platos> pedidosList
    {
        get => _pedidosList;
        private set
        {
            _pedidosList = value;
            OnPropertyChanged(nameof(pedidosList));
        }
    }

    public ObservableCollection<Platos> _carouselFood { get; set; }
    public ObservableCollection<Platos> carouselFood
    {
        get => _carouselFood;
        set
        {
            if (_carouselFood != value)
            {
                _carouselFood = value;
                OnPropertyChanged(nameof(carouselFood));
            }
        }
    }
    private bool _isEmpty = false;
    public bool IsEmpty
    {
        get => _isEmpty;
        set
        {
            _isEmpty = value;
            OnPropertyChanged(nameof(IsEmpty));
        }
    }
    private bool _isClient = true;
    public bool IsClient
    {
        get => _isClient;
        set
        {
            _isClient = value;
            OnPropertyChanged(nameof(IsClient));
        }
    }

    private decimal total = 0;
    public decimal Total
    {
        get => total;
        set
        {
            total = value;
            OnPropertyChanged(nameof(Total));
        }
    }

    private decimal tiempo = 0;
    public decimal Tiempo
    {
        get => tiempo;
        set
        {
            tiempo = value;
            OnPropertyChanged(nameof(Tiempo));
        }
    }

    public Pedidos()
    {
        InitializeComponent();
        _structureService = DependencyService.Get<StructureService>();

        metodoEntrega = "Delivery";
        IncrementCommand = new Command<Platos>(IncrementQuantity);
        DecrementCommand = new Command<Platos>(DecrementQuantity);

        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        currentUser = _structureService.User;
        NumeroDeMesas = _structureService.Mesas;

        categories = new ObservableCollection<Category>(_structureService.Categories);
        shopsList = new ObservableCollection<Platos>(_structureService.AllFoods);
        carouselFood = new ObservableCollection<Platos>(_structureService.AllFoods);

        if (currentUser != null)
        {
            nombre.Text = currentUser.Name;
            direccion.Text = currentUser.Direccion;
            telefono.Text = currentUser.Phone;
            Tiempo = _structureService.TiempoEntrega;

            if (currentUser.Rol == "Cliente")
            {
                IsClient = true;
                nombreCliente.IsVisible = false;
            }
            else
            {
                IsClient = false;
                nombreCliente.IsVisible = true;
                GenerarMesas();
                metodoEntrega = "Mesa";
                ResetQuantity();
            }
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

    private async Task updateTableView(int mesaNumero)
    {
        EstadoMesa.IsVisible = true;
        List<string> lista = await _structureService.GetTableStatesAsync();
        if (lista[mesaNumero - 1] == "0")
        {
            MesaLibre.BackgroundColor = Colors.LightGray;
            MesaOcupada.BackgroundColor = Colors.White;
        }
        else
        {
            MesaOcupada.BackgroundColor = Colors.LightGray;
            MesaLibre.BackgroundColor = Colors.White;
        }
    }

    private async void OnMesaSelected(int mesaNumero)
    {
        MesaSeleccionada = mesaNumero;
        await updateTableView(mesaNumero);
        ResetQuantity();

        foreach (var child in MesasContainer.Children)
        {
            if (child is Frame frame)
            {
                if (frame.Content is Label label)
                {
                    label.TextColor = Colors.Black;
                    frame.BorderColor = Colors.Black;
                }
            }
        }

        foreach (var child in MesasContainer.Children)
        {
            if (child is Frame frame && frame.Content is Label label && label.Text == mesaNumero.ToString())
            {
                frame.BorderColor = Colors.Cyan;
                label.TextColor = Colors.Cyan;
            }
        }

        OrdenPorUser orden = _structureService.OrdenesPorMesa.Find(c => c.NumeroMesa == mesaNumero);
        if (orden != null)
        {
            Orden = orden;
            nombre.Text = orden.Name;
            nombreCliente.Text = orden.NameMesa;
            pedidosList = new ObservableCollection<Platos>(orden.Platos);
            FillQuantity();
            Ordenar.IsVisible = false;
            Pagar.IsVisible = true;

            var calification = await _structureService.GetCalificationAsync(orden.UserId, _structureService.User.Id);

            if (calification != null)
            {
                UpdateStarColors(calification ?? 0);
            }
            CalificacionContainer.IsVisible = true;
            nit.IsVisible = true;
            razonSocial.IsVisible = true;
            nit.Text = orden.Nit;
            razonSocial.Text = orden.RazonSocial;
        }
        else
        {
            pedidosList = new ObservableCollection<Platos>();
            nombreCliente.Text = "";
            Total = 0;
            Ordenar.IsVisible = true;
            Pagar.IsVisible = false;
            CalificacionContainer.IsVisible = false;
            nit.IsVisible = false;
            razonSocial.IsVisible = false;
            nit.Text = string.Empty;
            nit.Text = string.Empty;
        }
        UpdatePedidosList();
    }

    private void UpdatePedidosList()
    {
        pedidosList = new ObservableCollection<Platos>(shopsList.Where(c => c.Quantity != 0));
        if (pedidosList.Count > 0)
        {
            Total = pedidosList.Sum(p => p.Total);
            IsEmpty = true;
        }
        else
        {
            Total = 0;
            IsEmpty = false;
        }
    }

    private void OnShopsListChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdatePedidosList();
    }

    private void IncrementQuantity(Platos plato)
    {
        if (plato == null) return;
        plato.Quantity++;
        UpdatePedidosList();
    }

    private void DecrementQuantity(Platos plato)
    {
        if (plato == null || plato.Quantity <= 0) return;
        plato.Quantity--;
        UpdatePedidosList();
    }

    private void FillQuantity()
    {
        foreach (var plato in shopsList)
        {
            var platoPedido = pedidosList.FirstOrDefault(pedido => pedido.Codigo == plato.Codigo);

            if (platoPedido != null)
            {
                plato.Quantity = platoPedido.Quantity;
            }
        }
    }
    private void ResetQuantity()
    {
        Total = 0;

        pedidosList.Clear();
        for (int i = 0; i < shopsList.Count; i++)
        {
            shopsList[i].Quantity = 0;
        }
        nombreCliente.Text = string.Empty;
    }

    private async void filterCategory(object sender, EventArgs e)
    {
        var tappedElement = (Frame)sender;
        var category = tappedElement.BindingContext as Category;

        if (category == null)
            return;

        shopsList = new ObservableCollection<Platos>(_structureService.AllFoods.Where(c => c.Categoria == category.Name));
    }

    private void OnCarouselItemTapped(object sender, EventArgs e)
    {
        var image = sender as Image;

        if (image != null)
        {
            var codigo = (image.BindingContext as Platos)?.Codigo;
            if (codigo != null)
            {
                var item = shopsList.FirstOrDefault(p => p.Codigo == codigo);
                if (item != null)
                {
                    ShopCollectionView.ScrollTo(item, position: ScrollToPosition.Center, animate: true);
                }
            }
        }
    }

    private async void MapMaker_Tapped(object sender, EventArgs e)
    {
        var popup = new MapPopup();
        popup.OnLocationSelected += (location) =>
        {
            direccion.Text = location;
        };

        await MopupService.Instance.PushAsync(popup);
    }
    private async void redirectTapped(object sender, EventArgs e)
    {
        var tappedElement = (Frame)sender;
        var selectedProduct = tappedElement.BindingContext as Platos;

        if (selectedProduct != null)
        {
            await Shell.Current.GoToAsync($"///ProductView?product={selectedProduct.Codigo}");
        }
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("///MainPage");
        return true;
    }

    private async void sendOrder_Clicked(object sender, EventArgs e)
    {
        if (currentUser == null)
        {
            await Shell.Current.GoToAsync("///Login");
            return;
        }

        var order = new OrdenPorUser();
        order.UserId = currentUser.Id;
        order.PedidoPorId = currentUser.Id;
        order.NumeroMesa = MesaSeleccionada;
        order.NameMesa = nombreCliente.Text ?? string.Empty;
        order.Name = nombre.Text;
        order.Direccion = direccion.Text;
        order.Phone = telefono.Text;
        order.Tipo = metodoEntrega;
        order.Fecha = DateTime.Now;
        order.Platos = pedidosList.ToList();

        try
        {
            bool enviadoConExito = await _structureService.SendPlatosWithQuantity(order);

            if (enviadoConExito)
            {
                ResetQuantity();
                await Application.Current.MainPage.DisplayAlert("Éxito", "Pedido enviado exitosamente.", "OK");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Advertencia", "Error al realizar el pedido.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al enviar el pedido: {ex.Message}", "OK");
        }
    }

    private async void PayOrder(object sender, EventArgs e)
    {
        if (nit.Text == null || nit.Text == string.Empty || razonSocial.Text == null || razonSocial.Text == string.Empty)
        {
            await DisplayAlert("Error", "Debe llenar los datos de facturacion.", "OK");
            return;
        }

        var button = (Button)sender;

        Orden.RazonSocial = razonSocial.Text;
        Orden.Nit = nit.Text;

        var OrdenAct = await _structureService.UpdateFacturacionAsync(Orden);

        await Shell.Current.GoToAsync($"///DetallesPedidoPage?code={Orden.Code}");
    }

    private void DeliveryButton_Clicked(object sender, EventArgs e)
    {
        metodoEntrega = "Delivery";
        deliveryButton.BackgroundColor = Colors.LightGray;
        pickupButton.BackgroundColor = Colors.White;
        direccionContainer.IsVisible = true;
    }

    private void PickupButton_Clicked(object sender, EventArgs e)
    {
        metodoEntrega = "Recoger";
        pickupButton.BackgroundColor = Colors.LightGray;
        deliveryButton.BackgroundColor = Colors.White;
        direccionContainer.IsVisible = false;
    }

    private void MesaLibre_Clicked(object sender, EventArgs e)
    {
        _structureService.ToggleTableStateAsync(MesaSeleccionada, "0");
        MesaLibre.BackgroundColor = Colors.LightGray;
        MesaOcupada.BackgroundColor = Colors.White;
    }

    private void MesaOcupada_Clicked(object sender, EventArgs e)
    {
        _structureService.ToggleTableStateAsync(MesaSeleccionada, "1");
        MesaOcupada.BackgroundColor = Colors.LightGray;
        MesaLibre.BackgroundColor = Colors.White;
    }


    public ObservableCollection<Color> StarColors { get; set; } = new ObservableCollection<Color> { Colors.Gray, Colors.Gray, Colors.Gray, Colors.Gray, Colors.Gray };

    private void UpdateStarColors(int starValue)
    {
        for (int i = 0; i < 5; i++)
        {
            if (i < starValue)
            {
                StarColors[i] = Colors.Gold;
            }
            else
            {
                StarColors[i] = Colors.Gray;
            }
        }
        OnPropertyChanged(nameof(StarColors));
    }

    private async void SaveCalification(object sender, EventArgs e)
    {
        if (sender is Label label)
        {
            var tapGesture = label.GestureRecognizers.OfType<TapGestureRecognizer>().FirstOrDefault();

            if (tapGesture != null)
            {
                if (int.TryParse(tapGesture.CommandParameter?.ToString(), out int starValue))
                {
                    if (Orden.UserId != null)
                    {
                        var calification = new Calification
                        {
                            userId = Orden.UserId,
                            empleadoId = _structureService.User.Id,
                            calification = starValue
                        };

                        await _structureService.AddCalificationAsync(calification);
                        UpdateStarColors(starValue);
                    }
                }
                else
                {
                    Console.WriteLine("Error: CommandParameter no es un valor entero válido.");
                }
            }
        }
    }
}