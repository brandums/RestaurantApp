namespace RestauranteMap.Models
{
    public class AlmacenList
    {
        public string[] Codigo { get; set; }
        public string[] Nombre { get; set; }
        public string[] Unidad { get; set; }
        public string[] Stock { get; set; }
        public string[] Precio { get; set; }
        public string[] CantidadUsada { get; set; }
        public string[] Existencias { get; set; }
    }

    public class Almacen
    {
        public string Codigo { get; set; }
        public string Nombre { get; set; }
        public string Unidad { get; set; }
        public string Stock { get; set; }
        public string Precio { get; set; }
        public string CantidadUsada { get; set; }
        public string Existencias { get; set; }

        public Color BackgroundColor => GetBackgroundColor();

        private Color GetBackgroundColor()
        {
            if (int.Parse(Stock) == 0) return Colors.Transparent;

            double porcentaje = (int.Parse(Existencias) / int.Parse(Stock)) * 100;

            if (porcentaje <= 30)
                return Colors.Red;
            else if (porcentaje <= 50)
                return Colors.Yellow;
            else
                return Colors.Green;
        }
    }


    public class RegistroAlmacen
    {
        public string[] Codigo { get; set; }
        public string[] Fecha { get; set; }
        public string[] CantidadUsada { get; set; }
    }
}
