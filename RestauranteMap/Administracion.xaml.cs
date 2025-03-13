namespace RestauranteMap;

public partial class Administracion : ContentPage
{
    public Administracion()
    {
        InitializeComponent();
        ContentContainer.Children.Add(new AdminPedidos());
    }

    private void OnLabelTapped(object sender, EventArgs e)
    {
        ContentContainer.Children.Clear();

        Pedidos.TextColor = Colors.Black;
        cocina.TextColor = Colors.Black;
        personal.TextColor = Colors.Black;
        clientes.TextColor = Colors.Black;
        ingresos.TextColor = Colors.Black;
        almacen.TextColor = Colors.Black;
        egresos.TextColor = Colors.Black;
        platos.TextColor = Colors.Black;
        categorias.TextColor = Colors.Black;

        if (sender is Label tappedLabel)
        {
            string labelText = tappedLabel.Text;

            tappedLabel.TextColor = Colors.Blue;

            switch (labelText)
            {
                case "Pedidos":
                    ContentContainer.Children.Add(new AdminPedidos());
                    break;
                case "Cocina":
                    ContentContainer.Children.Add(new CocinaAdmin());
                    break;
                case "Personal":
                    ContentContainer.Children.Add(new AdminUsers());
                    break;
                case "Clientes":
                    ContentContainer.Children.Add(new AdminClients());
                    break;
                case "Almacen":
                    ContentContainer.Children.Add(new Almacenes());
                    break;
                case "Ingresos":
                    ContentContainer.Children.Add(new Ingresos());
                    break;
                case "Egresos":
                    ContentContainer.Children.Add(new Egresos());
                    break;
                case "Platos":
                    ContentContainer.Children.Add(new formularioPlato());
                    break;
                case "Categorias":
                    ContentContainer.Children.Add(new Categorias());
                    break;
            }
        }
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("///MainPage");
        return true;
    }
}