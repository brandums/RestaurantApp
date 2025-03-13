namespace RestauranteMap.Models
{
    public class User
    {
        public string Code { get; set; } = "";
        public int Id { get; set; }
        public string AccountNumber { get; set; } = "";
        public string Name { get; set; }
        public string? CI { get; set; }
        public string? City { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string? Direccion { get; set; }
        public string? Rol { get; set; } = "";
        public string? ImageCarnet { get; set; } = "";
        public string? ImageFactura { get; set; } = "";
        public string? ImageSanitario { get; set; } = "";
        public string? RazonSocial { get; set; } = "";
        public string? Nit { get; set; } = "";
        public string[]? Asistencia { get; set; }
        public string[]? Rendimiento { get; set; }
        public Calification[]? Calificaciones { get; set; }
        public int? RendimientoCount { get; set; }
        public string? Calificacion { get; set; }
    }
}
