using System.ComponentModel;

namespace RestauranteMap.Models
{
    public class Platos : INotifyPropertyChanged
    {
        public string? Nombre { get; set; }
        public string? Precio { get; set; }
        public string? Descripcion { get; set; }
        public string? Codigo { get; set; }
        public string? Categoria { get; set; }
        public string? Image { get; set; }
        public string[]? Images { get; set; }
        public bool Especial { get; set; }
        public string[]? Video { get; set; } = [];
        public string Comentario { get; set; } = "";
        public Receta Receta { get; set; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
                OnPropertyChanged(nameof(Quantity));
            }
        }

        public decimal Total
        {
            get
            {
                if (decimal.TryParse(Precio, out var precioDecimal))
                {
                    return precioDecimal * Quantity;
                }
                return 0;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


    public class PlatoDTO
    {
        public string? Nombre { get; set; }
        public string? Precio { get; set; }
        public string? Descripcion { get; set; }
        public string? Codigo { get; set; } = "";
        public string? Categoria { get; set; }
        public string? Image { get; set; } = "";
        public string[]? Images { get; set; }
        public bool Especial { get; set; }
        public string[]? Video { get; set; } = [];
        public string Comentario { get; set; } = "";
        public Receta Receta { get; set; } = new Receta();

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set
            {
                _quantity = value;
            }
        }
    }
}
