namespace RestauranteMap.Models
{
    public class Substruct
    {
        public string Date { get; set; }
        public string[]? Nombre { get; set; } // id del producto en el carrito
        public string[]? Precio { get; set; }
        public string[]? Descripcion { get; set; }
        public string[]? Codigo { get; set; }
        public string[][]? Color { get; set; }
        public string[][]? Talla { get; set; }
        public string[]? Categoria { get; set; }
        public string[][]? Images { get; set; }
        public string[]? Extra1 { get; set; } // stock                        
        public string[]? Extra2 { get; set; } // medida: peso, talla
        public string[]? Extra3 { get; set; } // estado de visibilidad
        public string[]? Extra4 { get; set; } // pagina
        public string[]? Extra5 { get; set; } // posicion en pagina
        public string[]? Extra6 { get; set; } // promocion portada                //verificar compra 1 o 0
        public string[]? Extra7 { get; set; } // precio de oferta                 //favoritos
        public string[]? Extra8 { get; set; }
    }
}
