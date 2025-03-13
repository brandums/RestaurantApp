using RestauranteMap.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace RestauranteMap;

[QueryProperty(nameof(Code), "code")]
public partial class MetodoPago : ContentPage, INotifyPropertyChanged
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

    public MetodoPago()
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
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async void Pay(object sender, EventArgs e)
    {
        bool success = await _structureService.UpdatePlatosStatus(4, Orden);

        if (success)
        {
            await Shell.Current.GoToAsync("///Pedidos");
            await DisplayAlert("Éxito", "El pago de los platos se realizo correctamente.", "OK");
        }
        else
        {
            await DisplayAlert("Error", "Hubo un problema al realizar el pago de los platos.", "OK");
        }
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("///Pedidos");
        return true;
    }
}
