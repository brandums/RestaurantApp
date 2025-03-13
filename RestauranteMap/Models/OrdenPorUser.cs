namespace RestauranteMap.Models
{
    public class OrdenPorUser
    {
        public string Code { get; set; } = "";
        public int UserId { get; set; }
        public int PedidoPorId { get; set; }
        public string NameMesa { get; set; } = "";
        public int NumeroMesa { get; set; }
        public string Name { get; set; }
        public string Phone { get; set; }
        public string Direccion { get; set; } = "";
        public string Tipo { get; set; }
        public string Estado { get; set; } = "";
        public string RazonSocial { get; set; } = "";
        public string Nit { get; set; } = "";
        public int MeseroId { get; set; }
        public int DeliveryId { get; set; }
        public int Calificacion { get; set; }
        public int CantidadPersonas { get; set; }
        public string Hora { get; set; } = "";
        public DateTime Fecha { get; set; }
        public List<Platos> Platos { get; set; }
    }
}
