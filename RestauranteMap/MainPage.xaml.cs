using Newtonsoft.Json;
using RestauranteMap.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace RestauranteMap
{
    public partial class MainPage : ContentPage, INotifyPropertyChanged
    {
        private readonly StructureService _structureService;

        private readonly string UserFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "user4.json");
        private User currentUser;
        private PrincipalStruct currentPrincipalStruct;

        public ObservableCollection<Category> _categories { get; set; } = new ObservableCollection<Category>();
        public ObservableCollection<Category> categories
        {
            get => _categories;
            set
            {
                if (_categories != value)
                {
                    _categories = value;
                    OnPropertyChanged(nameof(categories));
                }
            }
        }

        public ObservableCollection<Platos> _shopsList { get; set; } = new ObservableCollection<Platos>();
        public ObservableCollection<Platos> shopsList
        {
            get => _shopsList;
            set
            {
                if (_shopsList != value)
                {
                    _shopsList = value;
                    OnPropertyChanged(nameof(shopsList));
                }
            }
        }

        public ObservableCollection<Platos> _specialFoods { get; set; } = new ObservableCollection<Platos>();
        public ObservableCollection<Platos> specialFoods
        {
            get => _specialFoods;
            set
            {
                if (_specialFoods != value)
                {
                    _specialFoods = value;
                    OnPropertyChanged(nameof(specialFoods));
                }
            }
        }

        private double textSize1;
        public double TextSize1
        {
            get => textSize1;
            set
            {
                if (textSize1 != value)
                {
                    textSize1 = value;
                    OnPropertyChanged(nameof(TextSize1));
                }
            }
        }

        private double textSize2;
        public double TextSize2
        {
            get => textSize2;
            set
            {
                if (textSize2 != value)
                {
                    textSize2 = value;
                    OnPropertyChanged(nameof(TextSize2));
                }
            }
        }

        public MainPage()
        {
            InitializeComponent();

            _structureService = DependencyService.Get<StructureService>();
            CheckConnectivity();

            TextSize1 = 15;
            TextSize2 = 13;

            BindingContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            currentUser = _structureService.User;
            changeUserView();

            await Task.Delay(100);

            if (GridDescription.IsVisible == true)
            {
                PrincipalPageContainer.TranslationY = this.Height;
                AnimateStackLayout();
            }
        }

        private async void AnimateStackLayout()
        {
            await Task.Delay(5000);

            double screenHeight = PrincipalContainer.Height;

            await Task.WhenAll(
                GridDescription.TranslateTo(0, -screenHeight, 2000, Easing.CubicIn),
                GridDescription.FadeTo(0, 2000, Easing.CubicIn),
                PrincipalPageContainer.TranslateTo(0, 0, 2000, Easing.CubicIn),
                PrincipalPageContainer.FadeTo(1, 2000, Easing.CubicIn)
            );

            GridDescription.IsVisible = false;
        }

        private double ScreenWidth { get; set; }
        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            UpdateLayoutBasedOnWidth(PrincipalContainer.Width, PrincipalContainer.Height);
        }

        private void SeeMoreShopsTapped(object sender, EventArgs e)
        {
            ViewMoreShops.IsVisible = true;
        }

        private void SeeMorePopularShopsTapped(object sender, EventArgs e)
        {
            ViewMorePopularShops.IsVisible = true;
        }

        private async void CloseViewMoreShopsTapped(object sender, EventArgs e)
        {
            ViewMoreShops.IsVisible = false;
        }

        private async void CloseViewMorePopularShopsTapped(object sender, EventArgs e)
        {
            ViewMorePopularShops.IsVisible = false;
        }

        private void UpdateLayoutBasedOnWidth(double width, double heigth)
        {
            if (width > heigth)
            {
                PrincipalPageContainer.RowDefinitions.Clear();
                PrincipalPageContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                PrincipalPageContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Star) });
                PrincipalPageContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(9, GridUnitType.Star) });

                ProductsContainer.RowDefinitions.Clear();
                ProductsContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0, GridUnitType.Star) });
                ProductsContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4, GridUnitType.Star) });
                ProductsContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(6, GridUnitType.Star) });

                ProductsContainer.ColumnDefinitions.Clear();
                ProductsContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4.5, GridUnitType.Star) });
                ProductsContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5.5, GridUnitType.Star) });

                ProductsContainer.ColumnSpacing = 30;

                shops.RowDefinitions.Clear();
                shops.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1.5, GridUnitType.Star) });
                shops.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1.5, GridUnitType.Star) });
                shops.RowDefinitions.Add(new RowDefinition { Height = new GridLength(7, GridUnitType.Star) });

                ProductsContainer.SetRow(shops, 1);
                ProductsContainer.SetRowSpan(shops, 3);
                ProductsContainer.SetColumn(shops, 0);

                TopProductsColumns.ColumnDefinitions.Clear();
                TopProductsColumns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3.5, GridUnitType.Star) });
                TopProductsColumns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) });
                TopProductsColumns.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2.5, GridUnitType.Star) });

                ProductsContainer.SetRow(TopProducts, 1);
                ProductsContainer.SetColumn(TopProducts, 1);

                ProductsViewColums.ColumnDefinitions.Clear();
                ProductsViewColums.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3.5, GridUnitType.Star) });
                ProductsViewColums.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(4, GridUnitType.Star) });
                ProductsViewColums.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2.5, GridUnitType.Star) });

                ProductsContainer.SetRow(ProductsView, 2);
                ProductsContainer.SetColumn(ProductsView, 1);

                ProductsCard.Span = 2;
                TextSize1 = 14;
                TextSize2 = 10;

                shops.RowSpacing = 20;
                CategoriesList.IsVisible = false;
                ShopCollection2.IsVisible = true;
                shopsTitle.IsVisible = true;
                shopsSearch.IsVisible = true;
                TopProductsSearch.IsVisible = true;
                ProductsViewSearch.IsVisible = true;

                ColumnShopCollection.Span = 2;
                ShopCollectionView2.IsVisible = true;
                ShopCollectionView.IsVisible = false;
                //PopularShopCollectionView2.IsVisible = true;
                //PopularShopCollectionView.IsVisible = false;
                ProductsCardViewMore.Span = 2;
            }
            else
            {
                PrincipalPageContainer.RowDefinitions.Clear();
                PrincipalPageContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(0.8, GridUnitType.Star) });
                PrincipalPageContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
                PrincipalPageContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(8.2, GridUnitType.Star) });

                ProductsContainer.RowDefinitions.Clear();
                ProductsContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1.6, GridUnitType.Star) });
                ProductsContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(3.5, GridUnitType.Star) });
                ProductsContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(4.9, GridUnitType.Star) });

                ProductsContainer.ColumnDefinitions.Clear();
                ProductsContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(10, GridUnitType.Star) });
                ProductsContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0, GridUnitType.Star) });

                ProductsContainer.ColumnSpacing = 0;

                ProductsContainer.SetRow(shops, 0);
                ProductsContainer.SetRowSpan(shops, 1);
                ProductsContainer.SetColumn(shops, 0);
                ProductsContainer.SetRow(TopProducts, 1);
                ProductsContainer.SetColumn(TopProducts, 0);
                ProductsContainer.SetRow(ProductsView, 2);
                ProductsContainer.SetColumn(ProductsView, 0);

                ProductsCard.Span = 1;

                shops.RowSpacing = 0;
                CategoriesList.IsVisible = true;
                ShopCollection2.IsVisible = false;
                shopsTitle.IsVisible = false;
                shopsSearch.IsVisible = false;
                TopProductsSearch.IsVisible = false;
                ProductsViewSearch.IsVisible = false;

                ColumnShopCollection.Span = 1;
                ShopCollectionView2.IsVisible = false;
                ShopCollectionView.IsVisible = true;
                //PopularShopCollectionView2.IsVisible = false;
                //PopularShopCollectionView.IsVisible = true;
                ProductsCardViewMore.Span = 1;
            }
        }

        private async void OnMenuIconTapped(object sender, EventArgs e)
        {
            Menu.IsVisible = true;
            await Menu.TranslateTo(0, 0, 250, Easing.SinIn);
        }

        private async void OnMenuBackgroundTapped(object sender, EventArgs e)
        {
            await Menu.TranslateTo(Menu.Width, 0, 250, Easing.SinIn);
            Menu.IsVisible = false;
        }

        private async void OnMenuItemTapped(object sender, EventArgs e)
        {
            var label = sender as Label;
            string route = string.Empty;

            switch (label.Text)
            {
                case "Pedidos":
                    route = "///Pedidos";
                    break;
                case "Lista de Pedidos":
                    route = "///PedidosMesa";
                    break;
                case "Cocina":
                    route = "///Cocina";
                    break;
                case "Mapa":
                    route = "///Map";
                    break;
                case "Login":
                    route = "///Login";
                    break;
                case "Administracion":
                    route = "///Administracion";
                    break;
                case "Número":
                    route = "NumeroPage";
                    break;
                case "Configuración":
                    route = "ConfiguracionPage";
                    break;
                case "Logout":
                    route = "LogoutPage";
                    break;
            }

            if (route == "LogoutPage")
            {
                deleteUserSerialize(route);
            }
            else if (!string.IsNullOrEmpty(route))
            {
                await Shell.Current.GoToAsync(route);
            }

            OnMenuBackgroundTapped(sender, e);
        }

        private async void deleteUserSerialize(string route)
        {
            if (File.Exists(UserFilePath))
            {
                try
                {
                    File.Delete(UserFilePath);
                    _structureService.User = null;
                    await Shell.Current.GoToAsync("///Login");

                    UserImage.IsVisible = false;
                    Logout.IsVisible = false;
                    Login.IsVisible = true;
                }
                catch (Exception ex)
                {

                }
            }
        }

        private void changeUserView()
        {
            if (currentUser != null)
            {
                UserImage.IsVisible = true;
                Logout.IsVisible = true;
                Login.IsVisible = false;
                userName.Text = currentUser.Name;
            }
        }

        private async void CheckConnectivity()
        {
            var isConnected = await _structureService.CheckAndUpdateData();
            if (!isConnected)
            {
                await DisplayAlert("Sin conexión a Internet", "No hay conexión a Internet. Algunos datos pueden no estar actualizados.", "OK");
            }

            currentPrincipalStruct = _structureService.PrincipalStruct;
            currentUser = _structureService.User;
            categories = new ObservableCollection<Category>(_structureService.Categories);
            specialFoods = new ObservableCollection<Platos>(_structureService.SpecialFoods);
            shopsList = new ObservableCollection<Platos>(_structureService.RegularFoods);

            changeUserView();
        }

        private async void filterCategory(object sender, EventArgs e)
        {
            var tappedElement = (Frame)sender;
            var category = tappedElement.BindingContext as Category;

            if (category == null)
                return;

            specialFoods = new ObservableCollection<Platos>(_structureService.SpecialFoods.Where(c => c.Categoria == category.Name));
        }


        private async void OnImageTapped(object sender, EventArgs e)
        {
            var tappedElement = (Image)sender;
            var selectedProduct = tappedElement.BindingContext as Platos;

            if (selectedProduct != null)
            {
                selectedProduct.Images = new string[4];
                selectedProduct.Images[0] = selectedProduct.Image;
                selectedProduct.Images[1] = selectedProduct.Image;
                selectedProduct.Images[2] = selectedProduct.Image;
                selectedProduct.Images[3] = selectedProduct.Image;
                var serializedProduct = JsonConvert.SerializeObject(selectedProduct);
                var navigationParameter = Uri.EscapeDataString(serializedProduct);

                await Shell.Current.GoToAsync($"///ProductView?product={navigationParameter}");
            }
        }

        private async void OnLabelTapped(object sender, EventArgs e)
        {
            var tappedElement = (Label)sender;
            var selectedProduct = tappedElement.BindingContext as Platos;

            if (selectedProduct != null)
            {
                selectedProduct.Images = new string[4];
                selectedProduct.Images[0] = selectedProduct.Image;
                selectedProduct.Images[1] = selectedProduct.Image;
                selectedProduct.Images[2] = selectedProduct.Image;
                selectedProduct.Images[3] = selectedProduct.Image;
                var serializedProduct = JsonConvert.SerializeObject(selectedProduct);
                var navigationParameter = Uri.EscapeDataString(serializedProduct);

                await Shell.Current.GoToAsync($"///ProductView?product={navigationParameter}");
            }
        }

        private async void redirectReserve(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"///Reservas");
        }
    }
}
