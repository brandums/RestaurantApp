using RestauranteMap.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RestauranteMap;

public partial class Categorias : ContentView, INotifyPropertyChanged
{
    private readonly StructureService _structureService;

    private ObservableCollection<Category> _categories;
    public ObservableCollection<Category> Categories
    {
        get => _categories;
        set
        {
            _categories = value;
            OnPropertyChanged(nameof(Categories)); // Notificar cambio
        }
    }

    private Category _categoriaSeleccionada;
    public Category CategoriaSeleccionada
    {
        get => _categoriaSeleccionada;
        set
        {
            _categoriaSeleccionada = value;
            OnPropertyChanged(nameof(CategoriaSeleccionada));
        }
    }


    private string _logoPath;
    public string LogoPath
    {
        get => _logoPath;
        set
        {
            _logoPath = value;
            OnPropertyChanged(nameof(LogoPath)); // Notificar cambio
        }
    }

    private string _imagePath;
    public string ImagePath
    {
        get => _imagePath;
        set
        {
            _imagePath = value;
            OnPropertyChanged(nameof(ImagePath)); // Notificar cambio
        }
    }

    public Categorias()
    {
        InitializeComponent();
        _structureService = DependencyService.Get<StructureService>();
        BindingContext = this;

        Categories = new ObservableCollection<Category>();
        CargarCategorias();
    }

    private async void CargarCategorias()
    {
        await _structureService.CheckAndUpdateData();
        var categorias = _structureService.Categories;

        Categories.Clear();
        foreach (var categoria in categorias)
        {
            Categories.Add(categoria);
        }
    }

    private async Task SolicitarPermisosAsync()
    {
        if (DeviceInfo.Platform == DevicePlatform.Android)
        {
            var statusRead = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
            var statusWrite = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            if (statusRead != PermissionStatus.Granted || statusWrite != PermissionStatus.Granted)
            {
                statusRead = await Permissions.RequestAsync<Permissions.StorageRead>();
                statusWrite = await Permissions.RequestAsync<Permissions.StorageWrite>();
            }

            if (statusRead != PermissionStatus.Granted || statusWrite != PermissionStatus.Granted)
            {
                //await DisplayAlert("Permiso necesario", "Debes otorgar permisos de almacenamiento para cargar imágenes.", "OK");
            }
        }
    }

    public void EditarCategoria(object sender, EventArgs e)
    {
        if (sender is Image img && img.BindingContext is Category categoria)
        {
            CategoriaSeleccionada = categoria;
            AbrirPopup(null, null);
        }
    }

    public async void AbrirPopup(object sender, EventArgs e)
    {
        popupCreate.IsVisible = true;
        await SolicitarPermisosAsync();

        if (CategoriaSeleccionada != null)
        {
            Nombre.Text = CategoriaSeleccionada.Name;
            LogoPath = CategoriaSeleccionada.Logo;
            ImagePath = CategoriaSeleccionada.Image;

            LogoPreview.Source = ImageSource.FromFile(LogoPath);
            ImagePreview.Source = ImageSource.FromFile(ImagePath);

            buttonCreate.Text = "Actualizar Categoria";
        }
        else
        {
            buttonCreate.Text = "Crear Categoria";
        }
    }

    public void CerrarPopup(object sender, EventArgs e)
    {
        LimpiarForm();
    }

    private async void SeleccionarLogo(object sender, EventArgs e)
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync();

            if (result != null)
            {
                LogoPath = result.FullPath;
                LogoPreview.Source = ImageSource.FromFile(LogoPath);
            }
        }
        catch (Exception ex)
        {
            //await DisplayAlert("Error", "No se pudo seleccionar la imagen: " + ex.Message, "OK");
        }
    }

    private async void SeleccionarImagen(object sender, EventArgs e)
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync();

            if (result != null)
            {
                ImagePath = result.FullPath;
                ImagePreview.Source = ImageSource.FromFile(ImagePath);
            }
        }
        catch (Exception ex)
        {
            //await DisplayAlert("Error", "No se pudo seleccionar la imagen: " + ex.Message, "OK");
        }
    }

    private async void CrearCategoria(object sender, EventArgs e)
    {
        var result = false;
        var newCategoria = new Category()
        {
            Name = Nombre.Text,
            Logo = await SubirImagen(LogoPath),
            Image = await SubirImagen(ImagePath),
        };

        if (_categoriaSeleccionada != null)
        {
            result = await _structureService.CreateOrUpdateCategory(ObtenerIndiceCategoria().ToString(), newCategoria);
        }
        else
        {
            result = await _structureService.CreateOrUpdateCategory("create", newCategoria);
        }

        if (result)
        {
            LimpiarForm();
            CargarCategorias();
        }
    }

    private async void EliminarCategoria(object sender, EventArgs e)
    {
        var result = false;
        var index = ObtenerIndiceCategoria();
        if (sender is Image img && img.BindingContext is Category categoria)
        {
            CategoriaSeleccionada = categoria;
            result = await _structureService.DeleteCategoryAsync(index);
        }

        if (result)
        {
            LimpiarForm();
            Categories.RemoveAt(index);
        }
    }

    private async Task<string> SubirImagen(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Console.WriteLine($"El archivo no existe: {path}");
            return "";
        }

        using var stream = File.OpenRead(path);
        try
        {
            return await _structureService.UploadImageAsync(stream, Path.GetFileName(path));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error al subir imagen de la receta {path}: {ex.Message}");
            return "";
        }
    }

    private bool VerificarNombre(string nombre)
    {
        return Categories.Any(c => c.Name.Equals(nombre, StringComparison.OrdinalIgnoreCase));
    }

    private int ObtenerIndiceCategoria()
    {
        return Categories.IndexOf(_categoriaSeleccionada);
    }


    private async void CerrarForm(object sender, EventArgs e)
    {
        LimpiarForm();
    }

    private void LimpiarForm()
    {
        ImagePath = null;
        LogoPath = null;
        ImagePreview.Source = "";
        LogoPreview.Source = "";
        Nombre.Text = "";
        CategoriaSeleccionada = null;
        popupCreate.IsVisible = false;
    }
}
