using RestauranteMap.Models;
using System.Collections.ObjectModel;

namespace RestauranteMap;

public partial class Almacenes : ContentView
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

    public Almacenes()
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
        if (_selectedAlmacen != null && int.TryParse(CantidadEntry.Text, out int cantidadUsada))
        {
            var nuevoRegistro = new RegistroAlmacen
            {
                Codigo = new[] { _selectedAlmacen.Codigo },
                Fecha = new[] { "" },
                CantidadUsada = new[] { cantidadUsada.ToString() }
            };

            await _structureService.AddRegistroAlmacen(nuevoRegistro);

            await LoadData();

            PopupContainer.IsVisible = false;
        }
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        PopupContainer.IsVisible = false;
    }
}