using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestauranteMap.Models;
using System.ComponentModel;
using System.Text;

namespace RestauranteMap;

public partial class Login : ContentPage, INotifyPropertyChanged
{
    private readonly StructureService _structureService;
    private readonly string userFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "user4.json");
    private readonly string apiUrl = "http://nekjoesg-001-site3.dtempurl.com/api";
    private HttpClient httpClient = new HttpClient();

    public Login()
    {
        InitializeComponent();

        _structureService = DependencyService.Get<StructureService>();
    }

    private void OnShowPasswordToggled(object sender, ToggledEventArgs e)
    {
        LoginPassword.IsPassword = !e.Value;
    }

    private void LoginTappedCommand(object sender, EventArgs e)
    {
        LoginLabel.TextColor = Colors.Blue;
        SignInLabel.TextColor = Colors.Gray;

        LoginForm.IsVisible = true;
        SignInForm.IsVisible = false;
    }

    private void SignInTappedCommand(object sender, EventArgs e)
    {
        LoginLabel.TextColor = Colors.Gray;
        SignInLabel.TextColor = Colors.Blue;

        LoginForm.IsVisible = false;
        SignInForm.IsVisible = true;
    }

    private async void LoginButtonClicked(object sender, EventArgs e)
    {
        try
        {
            HttpResponseMessage response = await httpClient.GetAsync($"{apiUrl}/User/login/{LoginEmail.Text}/{LoginPassword.Text}");

            if (response.IsSuccessStatusCode)
            {
                string jsonResponse = await response.Content.ReadAsStringAsync();

                JObject jsonObject = JObject.Parse(jsonResponse);
                User user = jsonObject["user"].ToObject<User>();

                if (user != null)
                {
                    _structureService.User = user;
                    Serialize(user);

                    await Shell.Current.GoToAsync("///MainPage");
                }
                else
                {
                    await DisplayAlert("Error", "No se pudo obtener al usuario", "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "No se encontro al usuario", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }

    private async void SignupButtonClicked(object sender, EventArgs e)
    {
        string password = signinPassword.Text;

        if (!IsPasswordValid(password))
        {
            await DisplayAlert("Contraseña Inválida", "La contraseña debe tener al menos 8 caracteres, un número, un símbolo y una letra mayúscula.", "OK");
            return;
        }

        var user = new User
        {
            Name = signinName.Text,
            Email = signinEmail.Text,
            City = signinCity.Text,
            Phone = signinPhone.Text,
            Direccion = signinAddress.Text,
            Password = signinPassword.Text
        };

        var jsonUser = JsonConvert.SerializeObject(user);

        try
        {
            HttpResponseMessage response = await httpClient.PostAsync($"{apiUrl}/User", new StringContent(jsonUser, Encoding.UTF8, "application/json"));

            if (response.IsSuccessStatusCode)
            {
                LoginLabel.TextColor = Colors.Blue;
                SignInLabel.TextColor = Colors.Gray;

                LoginForm.IsVisible = true;
                SignInForm.IsVisible = false;
                await DisplayAlert("Cuenta creada con exito", "Ahora puedes Iniciar sesion", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error: {ex.Message}", "OK");
        }
    }

    private void Serialize(User user)
    {
        string json = JsonConvert.SerializeObject(user);
        File.WriteAllText(userFilePath, json);
    }

    private bool IsPasswordValid(string password)
    {
        if (password.Length < 5)
        {
            return false;
        }
        if (!password.Any(char.IsDigit))
        {
            return false;
        }
        if (!password.Any(ch => !char.IsLetterOrDigit(ch)))
        {
            return false;
        }
        if (!password.Any(char.IsUpper))
        {
            return false;
        }

        return true;
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("///MainPage");
        return true;
    }
}