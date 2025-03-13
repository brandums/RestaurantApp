using Newtonsoft.Json;
using RestauranteMap.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace RestauranteMap;

public partial class Reservas : ContentPage, INotifyPropertyChanged
{
    private readonly StructureService _structureService;
    public ICommand IncrementCommand { get; private set; }
    public ICommand DecrementCommand { get; private set; }

    public TimeSpan SelectedTime { get; set; }
    public ICommand ConfirmTimeCommand { get; private set; }

    private readonly TimeSpan minTime = new TimeSpan(9, 0, 0);
    private readonly TimeSpan maxTime = new TimeSpan(18, 0, 0);

    private User currentUser;
    private PrincipalStruct currentPrincipalStruct;

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

    public Reservas()
    {
        InitializeComponent();

        _structureService = DependencyService.Get<StructureService>();

        IncrementCommand = new Command<Platos>(IncrementQuantity);
        DecrementCommand = new Command<Platos>(DecrementQuantity);

        Calendario.MinimumDate = DateTime.Now;

        ConfirmTimeCommand = new Command(ConfirmTime);

        SelectedTime = new TimeSpan(10, 0, 0);
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
        currentUser = _structureService.User;

        nombre.Text = currentUser.Name;
        telefono.Text = currentUser.Phone;

        categories = new ObservableCollection<Category>(_structureService.Categories);
        shopsList = new ObservableCollection<Platos>(_structureService.AllFoods);
        carouselFood = new ObservableCollection<Platos>(_structureService.AllFoods);
    }

    private void UpdatePedidosList()
    {
        pedidosList = new ObservableCollection<Platos>(shopsList.Where(c => c.Quantity != 0));
        if (pedidosList.Count > 0)
        {
            IsEmpty = true;
        }
        else
        {
            IsEmpty = false;
        }
    }

    private void OnShopsListChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        UpdatePedidosList();
    }

    private void IncrementQuantity(Platos plato)
    {
        if (plato != null)
        {
            plato.Quantity++;
            UpdatePedidosList();
        }
    }

    private void DecrementQuantity(Platos plato)
    {
        if (plato != null && plato.Quantity > 0)
        {
            plato.Quantity--;
            UpdatePedidosList();
        }
    }


    private void OnTimeChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(TimePicker.Time))
        {
            var selectedTime = ((TimePicker)sender).Time;

            if (selectedTime < minTime || selectedTime > maxTime)
            {
                errorLabel.Text = $"Por favor, seleccione una hora entre {minTime:hh\\:mm} y {maxTime:hh\\:mm}.";
                errorLabel.IsVisible = true;
            }
            else
            {
                errorLabel.IsVisible = false;
            }
        }
    }

    private void ConfirmTime()
    {
        if (!errorLabel.IsVisible)
        {
            DisplayAlert("Hora confirmada", $"Has seleccionado la hora: {SelectedTime:hh\\:mm}.", "OK");
        }
        else
        {
            DisplayAlert("Error", "La hora seleccionada no es válida.", "OK");
        }
    }

    private async void filterCategory(object sender, EventArgs e)
    {
        var tappedElement = (Frame)sender;
        var category = tappedElement.BindingContext as Category;

        if (category == null)
            return;

        shopsList = new ObservableCollection<Platos>(_structureService.AllFoods.Where(c => c.Categoria == category.Name));
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
        order.Name = nombre.Text;
        order.Phone = telefono.Text;
        order.Tipo = "Reserva";
        order.Fecha = Calendario.Date;
        order.Hora = timePicker.Time.ToString();
        order.CantidadPersonas = int.Parse(Cantidad.Text);

        try
        {
            await _structureService.SendPlatosWithQuantity(order);
            await Application.Current.MainPage.DisplayAlert("Éxito", "Pedido realizado con éxito.", "OK");
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", $"Error al enviar el pedido: {ex.Message}", "OK");
        }
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

    private async void redirectTapped(object sender, EventArgs e)
    {
        var tappedElement = (Frame)sender;
        var selectedProduct = tappedElement.BindingContext as Platos;

        if (selectedProduct != null)
        {
            selectedProduct.Images = new string[4];
            selectedProduct.Images[0] = selectedProduct.Image;
            selectedProduct.Images[1] = selectedProduct.Image;
            selectedProduct.Images[2] = selectedProduct.Image;
            selectedProduct.Images[3] = selectedProduct.Image;
            var serializedProduct = JsonConvert.SerializeObject(selectedProduct);
            var navigationParameter = Uri.EscapeDataString(serializedProduct);

            await Shell.Current.GoToAsync($"///ProductView?product={navigationParameter}");
        }
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("///MainPage");
        return true;
    }
}