using RestauranteMap.Models;

namespace RestauranteMap
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            var structureService = new StructureService();
            DependencyService.RegisterSingleton<StructureService>(structureService);

            MainPage = new AppShell();
        }
    }
}
