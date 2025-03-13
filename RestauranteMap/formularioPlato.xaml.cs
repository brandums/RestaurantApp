using RestauranteMap.Models;
using System.Collections.ObjectModel;

namespace RestauranteMap;

public partial class formularioPlato : ContentView
{
    private readonly StructureService _structureService;
    private ObservableCollection<string> _categorias;
    private string _categoriaSeleccionada;
    private int _cantidadImagenes;
    private Dictionary<int, string> _imagenesSeleccionadas;
    private string _videoSeleccionado;
    private int _cantidadIngredientes;
    private List<Ingredientes> _ingredientesSeleccionados;
    private Receta2 receta = new Receta2();
    private Dictionary<Entry, int> indicesTexto = new Dictionary<Entry, int>();
    private Dictionary<Entry, int> indicesSubtitulo = new Dictionary<Entry, int>();
    private Dictionary<Button, int> indicesImagen = new Dictionary<Button, int>();
    private Platos _platoEditando;
    public ObservableCollection<Platos> AllFoods { get; set; }

    public ObservableCollection<string> Categorias
    {
        get => _categorias;
        set
        {
            _categorias = value;
            OnPropertyChanged(nameof(Categorias));
        }
    }

    public string CategoriaSeleccionada
    {
        get => _categoriaSeleccionada;
        set
        {
            _categoriaSeleccionada = value;
            OnPropertyChanged(nameof(CategoriaSeleccionada));
        }
    }

    public formularioPlato()
    {
        InitializeComponent();
        _structureService = DependencyService.Get<StructureService>();

        _imagenesSeleccionadas = new Dictionary<int, string>();
        _ingredientesSeleccionados = new List<Ingredientes>();
        AllFoods = new ObservableCollection<Platos>();
        Categorias = new ObservableCollection<string>();
        InitializeView();

        BindingContext = this;
    }

    protected async void InitializeView()
    {
        if (CategoriaPicker != null)
        {
            CategoriaPicker.ItemsSource = Categorias;
        }

        await _structureService.CheckAndUpdateData();
        await SolicitarPermisosAsync();

        Categorias.Clear();
        foreach (var category in _structureService.Categories.Select(c => c.Name).ToList())
        {
            Categorias.Add(category);
        }

        AllFoods.Clear();
        foreach (var food in _structureService.AllFoods)
        {
            AllFoods.Add(food);
        }
    }

    private void OnCantidadImagenesChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(e.NewTextValue, out int cantidad) && cantidad > 0)
        {
            _cantidadImagenes = cantidad;
            if (ImagenesContainer != null)
            {
                GenerarBotonesSeleccionImagen(cantidad);
            }
        }
        else
        {
            ImagenesContainer?.Children.Clear();
        }
    }

    private void GenerarBotonesSeleccionImagen(int cantidad)
    {
        ImagenesContainer.Children.Clear();
        _imagenesSeleccionadas.Clear();

        for (int i = 0; i < cantidad; i++)
        {
            var grid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition { Width = GridLength.Auto },
                    new ColumnDefinition { Width = GridLength.Star }
                },
                ColumnSpacing = 8,
            };

            var image = new Image
            {
                HeightRequest = 50,
                WidthRequest = 50,
                Aspect = Aspect.AspectFill,
                BackgroundColor = Colors.Gray
            };

            if (_platoEditando != null && i < _platoEditando.Images.Length && !string.IsNullOrEmpty(_platoEditando.Images[i]))
            {
                image.Source = ImageSource.FromFile(_platoEditando.Images[i]);
                _imagenesSeleccionadas[i] = _platoEditando.Images[i];
            }

            var button = new Button
            {
                Text = $"Seleccionar Imagen {i + 1}",
                ClassId = $"imagen_{i}",
                Padding = 0,
                HeightRequest = 35,
            };

            int index = i;
            button.Clicked += async (s, e) => await OnSeleccionarImagenAsync(index, image);

            grid.Add(image, 0, 0);
            grid.Add(button, 1, 0);

            ImagenesContainer.Children.Add(grid);
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

    private async Task OnSeleccionarImagenAsync(int index, Image image)
    {
        try
        {
            var result = await MediaPicker.PickPhotoAsync();

            if (result != null)
            {
                string filePath = result.FullPath;
                _imagenesSeleccionadas[index] = filePath;
                image.Source = ImageSource.FromFile(filePath);
            }
        }
        catch (Exception ex)
        {
            //await DisplayAlert("Error", $"No se pudo seleccionar la imagen: {ex.Message}", "OK");
        }
    }

    private async void SeleccionarVideoAsync(object sender, EventArgs e)
    {
        try
        {
            var result = await MediaPicker.PickVideoAsync();
            if (result != null)
            {
                VideoLabel.Text = $"Video seleccionado: {Path.GetFileName(result.FullPath)}";
            }
        }
        catch (Exception ex)
        {
            //await DisplayAlert("Error", $"No se pudo seleccionar el video: {ex.Message}", "OK");
        }
    }



    private void OnCantidadIngredientesChanged(object sender, TextChangedEventArgs e)
    {
        if (int.TryParse(e.NewTextValue, out int cantidad) && cantidad > 0)
        {
            _cantidadIngredientes = cantidad;
            if (IngredientesContainer != null)
            {
                GenerarCamposIngredientes(cantidad);
            }
        }
        else
        {
            IngredientesContainer?.Children.Clear();
            _ingredientesSeleccionados.Clear();
        }
    }

    private async void GenerarCamposIngredientes(int cantidad)
    {
        IngredientesContainer.Children.Clear();
        _ingredientesSeleccionados.Clear();

        var almacenList = await _structureService.GetAlmacenes();

        if (almacenList == null || almacenList.Nombre == null)
        {
            return;
        }

        for (int i = 0; i < cantidad; i++)
        {
            var grid = new Grid
            {
                ColumnDefinitions =
            {
                new ColumnDefinition { Width = GridLength.Star },
                new ColumnDefinition { Width = GridLength.Auto },
                new ColumnDefinition { Width = GridLength.Star },
            },
                ColumnSpacing = 5,
            };

            var picker = new Picker
            {
                Title = $"Seleccionar Ingrediente {i + 1}",
                ItemsSource = almacenList.Nombre.ToList()
            };

            var cantidadEntry = new Entry
            {
                Placeholder = "Cantidad",
                Keyboard = Keyboard.Numeric
            };

            var apuntesEntry = new Entry
            {
                Placeholder = "Apuntes",
                HorizontalTextAlignment = TextAlignment.Start,
                VerticalTextAlignment = TextAlignment.Start
            };

            var ingrediente = new Ingredientes
            {
                Numero = i + 1
            };

            _ingredientesSeleccionados.Add(ingrediente);

            if (_platoEditando != null && i < _platoEditando.Receta.Ingredientes.Length)
            {
                var ingredienteExistente = _platoEditando.Receta.Ingredientes[i];
                picker.SelectedItem = ingredienteExistente.Nombre;
                cantidadEntry.Text = ingredienteExistente.Cantidad;
                apuntesEntry.Text = ingredienteExistente.Apuntes;

                ingrediente.Nombre = ingredienteExistente.Nombre;
                ingrediente.Unidad = ingredienteExistente.Unidad;
                ingrediente.Cantidad = ingredienteExistente.Cantidad;
                ingrediente.Apuntes = ingredienteExistente.Apuntes;
            }

            picker.SelectedIndexChanged += (s, e) =>
            {
                int selectedIndex = picker.SelectedIndex;

                if (selectedIndex >= 0)
                {
                    ingrediente.Nombre = almacenList.Nombre[selectedIndex];
                    ingrediente.Unidad = almacenList.Unidad[selectedIndex];
                }
            };

            cantidadEntry.TextChanged += (s, e) =>
            {
                ingrediente.Cantidad = e.NewTextValue;
            };

            apuntesEntry.TextChanged += (s, e) =>
            {
                ingrediente.Apuntes = e.NewTextValue;
            };

            grid.Add(picker, 0, 0);
            grid.Add(cantidadEntry, 1, 0);
            grid.Add(apuntesEntry, 2, 0);

            IngredientesContainer.Children.Add(grid);
        }
    }


    private void OnAgregarContenido(object sender, EventArgs e)
    {
        string tipoContenido = TipoContenidoPicker.SelectedItem?.ToString();

        if (string.IsNullOrEmpty(tipoContenido))
        {
            //DisplayAlert("Error", "Por favor, seleccione un tipo de contenido.", "OK");
            return;
        }

        if (tipoContenido == "Texto")
        {
            AgregarTexto();
        }
        else if (tipoContenido == "Imagen")
        {
            AgregarImagen();
        }
        else if (tipoContenido == "Subtitulo")
        {
            AgregarSubtitulo();
        }
    }

    private void AgregarTexto()
    {
        var entryTexto = new Entry
        {
            Placeholder = "Ingrese el texto"
        };

        var buttonEliminar = new Button
        {
            Text = "X",
            BackgroundColor = Colors.Red,
            TextColor = Colors.White,
            WidthRequest = 30,
            HeightRequest = 30,
            Padding = 0,
            HorizontalOptions = LayoutOptions.End
        };

        var grid = new Grid
        {
            ColumnDefinitions =
        {
            new ColumnDefinition { Width = GridLength.Star },
            new ColumnDefinition { Width = new GridLength(30) }
        }
        };

        grid.Add(entryTexto, 0, 0);
        grid.Add(buttonEliminar, 1, 0);

        CamposDinamicosContainer.Children.Add(grid);

        int index = receta.Texto.Count;
        receta.Texto.Add("");
        receta.Orden.Add($"txt{index + 1}");
        indicesTexto[entryTexto] = index;

        entryTexto.Completed += (s, e) =>
        {
            int idx = indicesTexto[entryTexto];
            receta.Texto[idx] = entryTexto.Text;
        };

        buttonEliminar.Clicked += (s, e) =>
        {
            EliminarElemento(entryTexto, receta.Texto, indicesTexto, "txt");
            CamposDinamicosContainer.Children.Remove(grid);
        };
    }

    private void AgregarImagen()
    {
        var buttonSeleccionarImagen = new Button
        {
            Text = "Seleccionar Imagen"
        };

        var buttonEliminar = new Button
        {
            Text = "X",
            BackgroundColor = Colors.Red,
            TextColor = Colors.White,
            WidthRequest = 30,
            HeightRequest = 30,
            Padding = 0,
            HorizontalOptions = LayoutOptions.End
        };

        var grid = new Grid
        {
            ColumnDefinitions =
        {
            new ColumnDefinition { Width = GridLength.Star },
            new ColumnDefinition { Width = new GridLength(30) }
        }
        };

        grid.Add(buttonSeleccionarImagen, 0, 0);
        grid.Add(buttonEliminar, 1, 0);

        CamposDinamicosContainer.Children.Add(grid);


        int index = receta.Imagen.Count;
        receta.Imagen.Add("");
        receta.Orden.Add($"img{index + 1}");
        indicesImagen[buttonSeleccionarImagen] = index;

        buttonSeleccionarImagen.Clicked += async (s, e) =>
        {
            var result = await MediaPicker.PickPhotoAsync();
            if (result != null)
            {
                string filePath = result.FullPath;
                int idx = indicesImagen[buttonSeleccionarImagen];
                receta.Imagen[idx] = filePath;
            }
        };

        buttonEliminar.Clicked += (s, e) =>
        {
            EliminarElemento(buttonSeleccionarImagen, receta.Imagen, indicesImagen, "img");
            CamposDinamicosContainer.Children.Remove(grid);
        };
    }

    private void AgregarSubtitulo()
    {
        var entrySubtitulo = new Entry
        {
            Placeholder = "Ingrese el subtitulo"
        };

        var buttonEliminar = new Button
        {
            Text = "X",
            BackgroundColor = Colors.Red,
            TextColor = Colors.White,
            WidthRequest = 30,
            HeightRequest = 30,
            Padding = 0,
            HorizontalOptions = LayoutOptions.End
        };

        var grid = new Grid
        {
            ColumnDefinitions =
        {
            new ColumnDefinition { Width = GridLength.Star },
            new ColumnDefinition { Width = new GridLength(30) }
        }
        };

        grid.Add(entrySubtitulo, 0, 0);
        grid.Add(buttonEliminar, 1, 0);

        CamposDinamicosContainer.Children.Add(grid);

        int index = receta.Subtitulo.Count;
        receta.Subtitulo.Add("");
        receta.Orden.Add($"sub{index + 1}");
        indicesSubtitulo[entrySubtitulo] = index;

        entrySubtitulo.Completed += (s, e) =>
        {
            int idx = indicesSubtitulo[entrySubtitulo];
            receta.Subtitulo[idx] = entrySubtitulo.Text;
        };

        buttonEliminar.Clicked += (s, e) =>
        {
            EliminarElemento(entrySubtitulo, receta.Subtitulo, indicesSubtitulo, "sub");
            CamposDinamicosContainer.Children.Remove(grid);
        };
    }

    private void EliminarElemento<T>(T elemento, List<string> lista, Dictionary<T, int> indices, string tipo)
    {
        if (indices.ContainsKey(elemento))
        {
            int index = indices[elemento];

            lista.RemoveAt(index);

            indices.Remove(elemento);

            receta.Orden.RemoveAt(index);

            int contadorTxt = 1, contadorSub = 1, contadorImg = 1;
            for (int i = 0; i < receta.Orden.Count; i++)
            {
                if (receta.Orden[i].StartsWith("txt"))
                {
                    receta.Orden[i] = $"txt{contadorTxt++}";
                }
                else if (receta.Orden[i].StartsWith("sub"))
                {
                    receta.Orden[i] = $"sub{contadorSub++}";
                }
                else if (receta.Orden[i].StartsWith("img"))
                {
                    receta.Orden[i] = $"img{contadorImg++}";
                }
            }

            var claves = indices.Keys.ToList();
            for (int i = 0; i < claves.Count; i++)
            {
                indices[claves[i]] = i;
            }
        }
    }


    public async void CrearPlato(object sender, EventArgs e)
    {
        var plato = new PlatoDTO()
        {
            Codigo = _platoEditando?.Codigo ?? "",
            Nombre = NombrePlato.Text,
            Descripcion = DescripcionPlato.Text,
            Precio = Precio.Text,
            Categoria = CategoriaPicker.SelectedItem.ToString(),
            Images = await SubirImagenes(_imagenesSeleccionadas.Values.ToArray()),
            Video = [],
            Especial = PlatoEspecialCheckBox.IsChecked,
        };

        var newReceta = new Receta()
        {
            Nombre = NombreRecetaEntry.Text,
            Ingredientes = _ingredientesSeleccionados.ToArray(),
            Subtitulo = receta.Subtitulo.ToArray(),
            Texto = receta.Texto.ToArray(),
            Imagen = await SubirImagenes(receta.Imagen.ToArray()),
            Orden = receta.Orden.ToArray(),
        };

        string recetaImageUrl = string.Empty;

        if (receta.Imagen != null && receta.Imagen.Count > 0)
        {
            string primeraImagen = receta.Imagen[0];

            using (var stream = File.OpenRead(primeraImagen))
            {
                try
                {
                    recetaImageUrl = await _structureService.UploadImageAsync(stream, Path.GetFileName(primeraImagen));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error al subir imagen de la receta {primeraImagen}: {ex.Message}");
                }
            }
        }

        plato.Receta = newReceta;

        bool exito = false;
        if (_platoEditando != null)
        {
            exito = await _structureService.EditarPlato(plato);
        }
        else
        {
            exito = await _structureService.CrearPlato(plato);
        }

        if (exito)
        {
            var newPlato = new Platos()
            {
                Codigo = _platoEditando?.Codigo ?? "",
                Nombre = NombrePlato.Text,
                Descripcion = DescripcionPlato.Text,
                Precio = Precio.Text,
                Categoria = CategoriaPicker.SelectedItem.ToString(),
                Images = plato.Images,
                Video = [],
                Especial = PlatoEspecialCheckBox.IsChecked,
                Receta = newReceta,
            };

            if (_platoEditando != null)
            {
                var index = AllFoods.IndexOf(_platoEditando);
                AllFoods[index] = newPlato;
            }
            else
            {
                AllFoods.Add(newPlato);
            }

            LimpiarFormulario();
        }
        else
        {
            Console.WriteLine("Hubo un error al crear el plato.");
        }
    }

    private async void EliminarPlato(object sender, EventArgs e)
    {
        var image = sender as Image;
        if (image != null)
        {
            var plato = image.BindingContext as Platos;
            if (plato != null)
            {
                bool exito = await _structureService.EliminarPlato(plato.Codigo);
                if (exito)
                {
                    AllFoods.Remove(plato);
                    //await DisplayAlert("Éxito", "Plato eliminado correctamente.", "OK");
                }
                else
                {
                    //await DisplayAlert("Error", "No se pudo eliminar el plato.", "OK");
                }
            }
        }
    }

    public async Task<string[]> SubirImagenes(string[] images)
    {
        var imageUrls = new List<string>();

        foreach (var kvp in images)
        {
            string primeraImagen = kvp;

            if (!primeraImagen.Contains("https://imagetesteo1.blob.core.windows.net/"))
            {
                using (var stream = File.OpenRead(primeraImagen))
                {
                    try
                    {
                        imageUrls.Add(await _structureService.UploadImageAsync(stream, Path.GetFileName(primeraImagen)));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error al subir imagen de la receta {primeraImagen}: {ex.Message}");
                    }
                }
            }
            else
            {
                imageUrls.Add(primeraImagen);
            }

        }
        return imageUrls.ToArray();
    }


    public void AbrirForm(object sender, EventArgs e)
    {
        PopupCreateForm.IsVisible = true;
    }

    public void CerrarForm(object sender, EventArgs e)
    {
        LimpiarFormulario();
    }

    private void LimpiarFormulario()
    {
        _platoEditando = null;
        NombrePlato.Text = string.Empty;
        DescripcionPlato.Text = string.Empty;
        Precio.Text = string.Empty;
        CategoriaPicker.SelectedItem = null;
        VideoLabel.Text = "No se ha seleccionado ningún video aún";
        _videoSeleccionado = null;
        PlatoEspecialCheckBox.IsChecked = false;
        NombreRecetaEntry.Text = string.Empty;
        _imagenesSeleccionadas.Clear();
        _ingredientesSeleccionados.Clear();
        receta = new Receta2();
        indicesTexto.Clear();
        indicesSubtitulo.Clear();
        indicesImagen.Clear();
        ImagenesContainer.Children.Clear();
        IngredientesContainer.Children.Clear();
        CamposDinamicosContainer.Children.Clear();
        PopupCreateForm.IsVisible = false;
        btnCreate.Text = "Crear Plato";
    }

    private void EditarPlato(object sender, EventArgs e)
    {
        var image = sender as Image;
        if (image != null)
        {
            var plato = image.BindingContext as Platos;
            if (plato != null)
            {
                AbrirFormularioEdicion(plato);
            }
        }
    }

    private void AbrirFormularioEdicion(Platos plato)
    {
        _platoEditando = plato;
        btnCreate.Text = "Actualizar Plato";
        PopupCreateForm.IsVisible = true;

        NombrePlato.Text = plato.Nombre;
        DescripcionPlato.Text = plato.Descripcion;
        Precio.Text = plato.Precio;
        CategoriaPicker.SelectedItem = plato.Categoria;
        PlatoEspecialCheckBox.IsChecked = plato.Especial;
        NombreRecetaEntry.Text = plato.Receta.Nombre;

        _imagenesSeleccionadas.Clear();
        for (int i = 0; i < plato.Images.Length; i++)
        {
            _imagenesSeleccionadas[i] = plato.Images[i];
        }

        GenerarBotonesSeleccionImagen(plato.Images.Length);

        _ingredientesSeleccionados.Clear();
        foreach (var ingrediente in plato.Receta.Ingredientes)
        {
            _ingredientesSeleccionados.Add(ingrediente);
        }

        GenerarCamposIngredientes(plato.Receta.Ingredientes.Length);

        CargarContenidoDinamico(plato.Receta);
    }

    private void CargarContenidoDinamico(Receta receta)
    {
        CamposDinamicosContainer.Children.Clear();
        indicesTexto.Clear();
        indicesSubtitulo.Clear();
        indicesImagen.Clear();

        foreach (var orden in receta.Orden)
        {
            if (orden.StartsWith("txt"))
            {
                AgregarTexto();
            }
            else if (orden.StartsWith("sub"))
            {
                AgregarSubtitulo();
            }
            else if (orden.StartsWith("img"))
            {
                AgregarImagen();
            }
        }

        for (int i = 0; i < receta.Texto.Length; i++)
        {
            var entry = indicesTexto.FirstOrDefault(x => x.Value == i).Key;
            if (entry != null)
            {
                entry.Text = receta.Texto[i];
            }
        }

        for (int i = 0; i < receta.Subtitulo.Length; i++)
        {
            var entry = indicesSubtitulo.FirstOrDefault(x => x.Value == i).Key;
            if (entry != null)
            {
                entry.Text = receta.Subtitulo[i];
            }
        }

        for (int i = 0; i < receta.Imagen.Length; i++)
        {
            var button = indicesImagen.FirstOrDefault(x => x.Value == i).Key;
            if (button != null)
            {
                // Aquí cargar la imagen 
            }
        }
    }
}
