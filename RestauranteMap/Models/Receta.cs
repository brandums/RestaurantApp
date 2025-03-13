namespace RestauranteMap.Models
{
    public class Receta
    {
        public string Nombre { get; set; }
        public Ingredientes[] Ingredientes { get; set; }
        public string[] Subtitulo { get; set; } = [];
        public string[] Texto { get; set; } = [];
        public string[] Imagen { get; set; } = [];
        public string[] Orden { get; set; } = [];
    }

    public class Receta2
    {
        public List<string> Texto { get; set; } = new List<string>();
        public List<string> Orden { get; set; } = new List<string>();
        public List<string> Imagen { get; set; } = new List<string>();
        public List<string> Subtitulo { get; set; } = new List<string>();
    }


    public class Ingredientes
    {
        public int Numero { get; set; }
        public string Nombre { get; set; }
        public string Cantidad { get; set; }
        public string Unidad { get; set; }
        public string Apuntes { get; set; }
    }
}
