namespace RestauranteMap.Models
{
    public class PrincipalStruct
    {
        public int Id { get; set; }
        public string? Canal { get; set; }
        public Substruct[]? Substructs { get; set; }
        public string[]? Data1 { get; set; } // numero de paginas
        public string[]? Data2 { get; set; } // codigo de los productos visibles
        public string[]? Data3 { get; set; } // codigo de los productos promocionales
        public string[]? Data4 { get; set; }
        public string[]? Data5 { get; set; }
        public string[]? Data6 { get; set; } // category
        public string[]? Data7 { get; set; } // logo
        public string[]? Data8 { get; set; } // banner (promociones)
        public string[]? Data9 { get; set; }
        public double[]? Data10 { get; set; }
        public double[]? Data11 { get; set; }
        public double[]? Data12 { get; set; }
        public int[]? Data13 { get; set; }
        public int[]? Data14 { get; set; }
        public int[]? Data15 { get; set; }
        public int[]? Data16 { get; set; }
        public int[]? Data17 { get; set; }
        public int[]? Data18 { get; set; }
    }
}
