using RestauranteMap.Models;
using System.ComponentModel;
using System.Timers;

namespace RestauranteMap;

[QueryProperty(nameof(ProductCode), "product")]
public partial class ProductView : ContentPage, INotifyPropertyChanged
{
    private Platos plato;
    private System.Timers.Timer _timer;
    private int _currentIndex = 0;

    private string _productCode;
    public string ProductCode
    {
        get => _productCode;
        set
        {
            _productCode = Uri.UnescapeDataString(value);

            plato = DependencyService.Get<StructureService>().AllFoods
                    .FirstOrDefault(f => f.Codigo == _productCode);

            if (plato.Quantity == 0)
            {
                cantidadContainer.IsVisible = false;
            }
            else
            {
                cantidadContainer.IsVisible = true;
            }
            Comentario = plato.Comentario;
            BindingContext = plato;

            StartImageCarousel();
        }
    }

    private string _comentario;

    public string Comentario
    {
        get => _comentario;
        set
        {
            if (_comentario != value)
            {
                _comentario = value;
                OnPropertyChanged(nameof(Comentario));
            }
        }
    }

    public ProductView()
    {
        InitializeComponent();

        BindingContext = this;
    }

    public event PropertyChangedEventHandler PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void StartImageCarousel()
    {
        _timer = new System.Timers.Timer(2000);
        _timer.Elapsed += OnTimerElapsed;
        _timer.AutoReset = true;
        _timer.Start();
    }

    private void IncrementQuantity(object sender, EventArgs e)
    {
        if (plato != null)
        {
            plato.Quantity++;
            UpdateStructureService(plato);
        }
    }

    private void DecrementQuantity(object sender, EventArgs e)
    {
        if (plato != null && plato.Quantity > 0)
        {
            plato.Quantity--;
            UpdateStructureService(plato);
        }
    }

    private void OnCommentCompleted(object sender, EventArgs e)
    {
        if (Comentario != null)
        {
            UpdateStructureService(plato);
        }
    }

    private void UpdateStructureService(Platos updatedPlato)
    {
        var structureService = DependencyService.Get<StructureService>();
        var allFoods = structureService.AllFoods;

        var platoInList = allFoods.FirstOrDefault(f => f.Codigo == updatedPlato.Codigo);
        if (platoInList != null)
        {
            platoInList = updatedPlato;
        }
    }


    private void OnTimerElapsed(object sender, ElapsedEventArgs e)
    {
        Device.BeginInvokeOnMainThread(() =>
        {
            var carouselView = this.FindByName<CarouselView>("carouselView");
            if (carouselView != null && BindingContext is Platos platos)
            {
                var totalImages = platos.Images.Length;
                if (totalImages > 0)
                {
                    if (_currentIndex == totalImages)
                    {
                        _currentIndex = 0;
                    }
                    else
                    {
                        _currentIndex++;
                    }
                    carouselView.ScrollTo(_currentIndex);
                }
            }
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _timer?.Stop();
        _timer?.Dispose();
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("///MainPage");
        return true;
    }

}