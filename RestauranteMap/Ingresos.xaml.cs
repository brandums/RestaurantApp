using RestauranteMap.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RestauranteMap;

public partial class Ingresos : ContentView, INotifyPropertyChanged
{
    private readonly StructureService _structureService;

    public ObservableCollection<OrdenPorUser> Orders
    {
        get => _orders;
        set
        {
            _orders = value;
            OnPropertyChanged();
        }
    }
    private ObservableCollection<OrdenPorUser> _orders;

    public ObservableCollection<OrdenPorUser> FilteredOrders
    {
        get => _filteredOrders;
        set
        {
            _filteredOrders = value;
            OnPropertyChanged();
            UpdateTotal();
        }
    }
    private ObservableCollection<OrdenPorUser> _filteredOrders;

    public string SelectedFilter
    {
        get => _selectedFilter;
        set
        {
            _selectedFilter = value;
            OnPropertyChanged();
            ApplyFilter();
        }
    }
    private string _selectedFilter;

    public decimal Total
    {
        get => _total;
        set
        {
            _total = value;
            OnPropertyChanged();
        }
    }
    private decimal _total;

    public List<string> Filters { get; set; }

    public Ingresos()
    {
        InitializeComponent();
        _structureService = DependencyService.Get<StructureService>();
        Orders = new ObservableCollection<OrdenPorUser>();
        FilteredOrders = new ObservableCollection<OrdenPorUser>();

        Filters = new List<string> { "Día", "Semana", "Mes" };
        SelectedFilter = "Mes";

        LoadOrders();
        BindingContext = this;
    }

    public async void LoadOrders()
    {
        try
        {
            var orders = await _structureService.GetOrders(4);
            Orders = new ObservableCollection<OrdenPorUser>(orders);

            ApplyFilter();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al cargar los pedidos: {ex.Message}");
        }
    }

    public void ApplyFilter()
    {
        if (Orders == null || string.IsNullOrEmpty(SelectedFilter))
        {
            FilteredOrders = new ObservableCollection<OrdenPorUser>(Orders);
            return;
        }

        DateTime now = DateTime.Now;
        DateTime startDate = now;

        if (SelectedFilter == "Día")
        {
            startDate = now.Date;
        }
        else if (SelectedFilter == "Semana")
        {
            startDate = now.Date.AddDays(-(int)now.DayOfWeek);
        }
        else if (SelectedFilter == "Mes")
        {
            startDate = now.Date.AddDays(-30);
        }

        var filtered = Orders.Where(order => order.Fecha >= startDate && order.Fecha <= now);
        FilteredOrders = new ObservableCollection<OrdenPorUser>(filtered);

        foreach (var order in FilteredOrders)
        {
            var total = order.Platos?.Sum(plato => plato.Total) ?? 0;
            order.Phone = total.ToString("F2");
        }
    }

    private void UpdateTotal()
    {
        Total = FilteredOrders.SelectMany(order => order.Platos)
                              .Sum(plato => plato.Total);
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
