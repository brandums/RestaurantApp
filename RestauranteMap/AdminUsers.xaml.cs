using RestauranteMap.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RestauranteMap;

public partial class AdminUsers : ContentView, INotifyPropertyChanged
{
    private readonly StructureService _structureService;
    public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();
    public ObservableCollection<string> Roles { get; set; } = new ObservableCollection<string> { "AdminM", "Admin", "Cocinero", "Cajero", "Cliente", "Mesero", "Ayudante" };

    public string TempImageCarnet { get; set; }
    public string TempImageFactura { get; set; }
    public string TempImageSanitario { get; set; }

    private User _selectedUser;
    public User SelectedUser
    {
        get => _selectedUser;
        set
        {
            _selectedUser = value;
            OnPropertyChanged();
            IsPopupVisible = value != null;
        }
    }

    private bool _isPopupVisible;
    public bool IsPopupVisible
    {
        get => _isPopupVisible;
        set
        {
            _isPopupVisible = value;
            OnPropertyChanged();
        }
    }

    private string _searchText;
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value;
            OnPropertyChanged();
            OnSearchTextChanged(_searchText);
        }
    }

    public ICommand SaveUserCommand { get; }
    public ICommand ClosePopupCommand { get; }
    public ICommand SelectImageCommand { get; }

    public AdminUsers()
    {
        InitializeComponent();
        _structureService = DependencyService.Get<StructureService>();

        SelectImageCommand = new Command<string>(async (imageProperty) => await SelectImageAsync(imageProperty));
        SaveUserCommand = new Command(SaveUser);
        ClosePopupCommand = new Command(ClosePopup);

        BindingContext = this;

        LoadUsers();
    }

    private async Task LoadUsers()
    {
        var users = await _structureService.LoadUsersAsync("empleados");
        Users.Clear();
        foreach (var user in users)
        {
            Users.Add(user);
        }
    }

    private void ClosePopup()
    {
        SelectedUser = null;
        IsPopupVisible = false;
    }

    private void OnSearchTextChanged(string searchText)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            LoadUsers();
        }
        else
        {
            var filteredUsers = Users.Where(user => user.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase)).ToList();

            Users.Clear();
            foreach (var user in filteredUsers)
            {
                Users.Add(user);
            }
        }
    }

    private async Task SelectImageAsync(string imageProperty)
    {
        try
        {
            var pickResult = await FilePicker.PickAsync(new PickOptions
            {
                FileTypes = FilePickerFileType.Images,
                PickerTitle = "Selecciona una imagen"
            });

            if (pickResult != null)
            {
                using var stream = await pickResult.OpenReadAsync();
                var tempPath = Path.Combine(FileSystem.CacheDirectory, pickResult.FileName);

                using (var fileStream = File.Create(tempPath))
                {
                    await stream.CopyToAsync(fileStream);
                }

                if (imageProperty == nameof(TempImageCarnet))
                {
                    TempImageCarnet = tempPath;
                    OnPropertyChanged(nameof(TempImageCarnet));
                }
                else if (imageProperty == nameof(TempImageFactura))
                {
                    TempImageFactura = tempPath;
                    OnPropertyChanged(nameof(TempImageFactura));
                }
                else if (imageProperty == nameof(TempImageSanitario))
                {
                    TempImageSanitario = tempPath;
                    OnPropertyChanged(nameof(TempImageSanitario));
                }

            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void SaveUser()
    {
        if (SelectedUser != null)
        {
            //if (!string.IsNullOrEmpty(TempImageCarnet))
            //{
            //    using var stream = File.OpenRead(TempImageCarnet);
            //    SelectedUser.ImageCarnet = await _structureService.UploadImageAsync(stream, Path.GetFileName(TempImageCarnet));
            //}

            //if (!string.IsNullOrEmpty(TempImageFactura))
            //{
            //    using var stream = File.OpenRead(TempImageFactura);
            //    SelectedUser.ImageFactura = await _structureService.UploadImageAsync(stream, Path.GetFileName(TempImageFactura));
            //}

            //if (!string.IsNullOrEmpty(TempImageSanitario))
            //{
            //    using var stream = File.OpenRead(TempImageSanitario);
            //    SelectedUser.ImageSanitario = await _structureService.UploadImageAsync(stream, Path.GetFileName(TempImageSanitario));
            //}

            await _structureService.UpdateUserInfo(SelectedUser);

            ClosePopup();
            LoadUsers();
        }
    }

    private async void OnLabelTapped(object sender, EventArgs e)
    {
        Asistencia.IsVisible = false;
        Rendimiento.IsVisible = false;
        Editar.IsVisible = false;

        LabelAsistencia.TextColor = Colors.Black;
        LabelRendimiento.TextColor = Colors.Black;
        LabelEditar.TextColor = Colors.Black;

        if (sender is Label tappedLabel)
        {
            string labelText = tappedLabel.Text;

            tappedLabel.TextColor = Colors.Blue;

            switch (labelText)
            {
                case "Asistencia":
                    await LoadUsers();
                    Asistencia.IsVisible = true;
                    break;
                case "Rendimiento":
                    await LoadUsers();
                    Rendimiento.IsVisible = true;

                    break;
                case "Editar":
                    await LoadUsers();
                    Editar.IsVisible = true;
                    break;
            }
        }
    }


    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}