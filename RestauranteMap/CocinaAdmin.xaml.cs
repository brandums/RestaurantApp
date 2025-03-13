using RestauranteMap.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace RestauranteMap;

public partial class CocinaAdmin : ContentView, INotifyPropertyChanged
{
    public ICommand RefreshCommand { get; }

    private readonly StructureService _structureService;
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

    public CocinaAdmin()
    {
        InitializeComponent();

        _structureService = new StructureService();
        Orders = new ObservableCollection<OrdenPorUser>();

        RefreshCommand = new Command(RefreshOrders);
        LoadInitialOrders();

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
            var ordersList = await _structureService.GetOrders(1);
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
}