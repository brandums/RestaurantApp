using RestauranteMap.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RestauranteMap;

[QueryProperty(nameof(Code), "code")]
public partial class DetallesPedidoPage : ContentPage, INotifyPropertyChanged
{
    private readonly StructureService _structureService;

    private string _code;
    private OrdenPorUser _orden;

    public event PropertyChangedEventHandler PropertyChanged;

    public string Code
    {
        get => _code;
        set
        {
            if (_code != value)
            {
                _code = value;
                OnPropertyChanged();
                _ = LoadOrderAsync();
            }
        }
    }

    public OrdenPorUser Orden
    {
        get => _orden;
        set
        {
            if (_orden != value)
            {
                _orden = value;
                OnPropertyChanged();
            }
        }
    }
    public decimal MontoTotal
    {
        get => Orden?.Platos?.Sum(p => p.Total) ?? 0;
    }

    public DetallesPedidoPage()
    {
        InitializeComponent();
        _structureService = DependencyService.Get<StructureService>();
        BindingContext = this;
    }

    private async Task LoadOrderAsync()
    {
        if (!string.IsNullOrWhiteSpace(Code))
        {
            Orden = await _structureService.GetOrder(Code);
            OnPropertyChanged(nameof(MontoTotal));
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async void redirectPage(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"///MetodoPago?code={Orden.Code}");
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("///Pedidos");
        return true;
    }
}
