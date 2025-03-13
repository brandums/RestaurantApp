using RestauranteMap.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace RestauranteMap;

public partial class AdminClients : ContentView, INotifyPropertyChanged
{
    private readonly StructureService _structureService;
    private User user;
    public ObservableCollection<User> Users { get; set; } = new ObservableCollection<User>();

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

    private bool _isCalificationPopupVisible;
    public bool IsCalificationPopupVisible
    {
        get => _isCalificationPopupVisible;
        set
        {
            _isCalificationPopupVisible = value;
            OnPropertyChanged();
        }
    }

    public ICommand CloseCalificationPopupCommand { get; }
    public ICommand SaveUserCommand { get; }
    public ICommand ClosePopupCommand { get; }

    public AdminClients()
    {
        InitializeComponent();
        _structureService = DependencyService.Get<StructureService>();
        user = _structureService.User;

        SaveUserCommand = new Command(SaveUser);
        ClosePopupCommand = new Command(ClosePopup);
        CloseCalificationPopupCommand = new Command(CloseCalificationPopup);

        BindingContext = this;

        LoadUsers();
    }

    private async Task LoadUsers()
    {
        var users = await _structureService.LoadUsersAsync("clientes");
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

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ObservableCollection<Color> StarColors { get; set; } = new ObservableCollection<Color> { Colors.Gray, Colors.Gray, Colors.Gray, Colors.Gray, Colors.Gray };

    private void UpdateStarColors(int starValue)
    {
        for (int i = 0; i < 5; i++)
        {
            if (i < starValue)
            {
                StarColors[i] = Colors.Gold;
            }
            else
            {
                StarColors[i] = Colors.Gray;
            }
        }
        OnPropertyChanged(nameof(StarColors));
    }

    private async void SaveCalification(object sender, EventArgs e)
    {
        if (sender is Label label)
        {
            var tapGesture = label.GestureRecognizers.OfType<TapGestureRecognizer>().FirstOrDefault();

            if (tapGesture != null)
            {
                if (int.TryParse(tapGesture.CommandParameter?.ToString(), out int starValue))
                {
                    if (SelectedUser != null)
                    {
                        var calification = new Calification
                        {
                            userId = SelectedUser.Id,
                            empleadoId = user.Id,
                            calification = starValue
                        };

                        await _structureService.AddCalificationAsync(calification);
                        UpdateStarColors(starValue);
                        LoadUsers();
                    }
                }
                else
                {
                    Console.WriteLine("Error: CommandParameter no es un valor entero válido.");
                }
            }
        }
    }

    private void CloseCalificationPopup()
    {
        IsCalificationPopupVisible = false;
        SelectedUser = null;
    }

    private async void OpenCalificationPopup(object sender, EventArgs e)
    {
        if (sender is Button button && button.BindingContext is User selectedUser)
        {
            SelectedUser = selectedUser;

            var calification = await _structureService.GetCalificationAsync(SelectedUser.Id, user.Id);

            if (calification != null)
            {
                UpdateStarColors(calification ?? 0);
            }

            IsCalificationPopupVisible = true;
        }
    }
}