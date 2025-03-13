using RestauranteMap.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RestauranteMap;

public partial class Egresos : ContentView, INotifyPropertyChanged
{
    private readonly StructureService _structureService;
    public ObservableCollection<Almacen> AlmacenList
    {
        get => _almacenList;
        set
        {
            _almacenList = value;
            OnPropertyChanged();
        }
    }
    private ObservableCollection<Almacen> _almacenList;

    private Almacen _selectedAlmacen;

    public Egresos()
    {
        InitializeComponent();
        _structureService = DependencyService.Get<StructureService>();

        LoadData();
        BindingContext = this;
    }

    private async Task LoadData()
    {
        try
        {
            AlmacenList = new ObservableCollection<Almacen>();
            var almacenes = await _structureService.GetAlmacenes();
            if (almacenes != null && almacenes.Nombre != null)
            {
                for (int i = 0; i < almacenes.Nombre.Length; i++)
                {
                    AlmacenList.Add(new Almacen
                    {
                        Nombre = almacenes.Nombre[i],
                        Unidad = almacenes.Unidad[i],
                        Stock = almacenes.Stock[i],
                        Precio = almacenes.Precio[i],
                        CantidadUsada = almacenes.CantidadUsada[i],
                        Existencias = almacenes.Existencias[i],
                        Codigo = almacenes.Codigo[i]
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error", $"No se pudieron cargar los almacenes: {ex.Message}", "OK");
        }
    }

    private void OnItemSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Almacen selectedAlmacen)
        {
            _selectedAlmacen = selectedAlmacen;
            PopupContainer.IsVisible = true;
        }
    }

    private async void OnSaveClicked(object sender, EventArgs e)
    {
        if (_selectedAlmacen != null && int.TryParse(CantidadEntry.Text, out int cantidadComprada))
        {
            await _structureService.UpdateStockAsync(_selectedAlmacen.Codigo, cantidadComprada);

            await LoadData();

            PopupContainer.IsVisible = false;
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        PopupContainer.IsVisible = false;
    }



    private void OnAddProductClicked(object sender, EventArgs e)
    {
        AddProductPopup.IsVisible = true;
    }

    private void OnCancelProductClicked(object sender, EventArgs e)
    {
        AddProductPopup.IsVisible = false;
    }

    private async void OnCreateProductClicked(object sender, EventArgs e)
    {
        try
        {
            var nuevoAlmacen = new AlmacenList
            {
                Codigo = new[] { "" },
                Nombre = new[] { NombreEntry.Text },
                Unidad = new[] { UnidadEntry.Text },
                Stock = new[] { StockEntry.Text.ToString() },
                Precio = new[] { PrecioEntry.Text.ToString() },
                CantidadUsada = new[] { "0" },
                Existencias = new[] { StockEntry.Text.ToString() }
            };

            await _structureService.AddAlmacenAsync(nuevoAlmacen);

            await LoadData();

            AddProductPopup.IsVisible = false;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al agregar producto: {ex.Message}");
        }
    }

}