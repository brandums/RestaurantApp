using System.Globalization;

namespace RestauranteMap.Models
{
    public class TimeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OrdenPorUser pedido)
            {
                var timeElapsed = DateTime.Now - pedido.Fecha;

                if (pedido.Estado == "Entregado" || pedido.Estado == "Pagado")
                {
                    return Colors.Cyan;
                }

                if (timeElapsed.TotalMinutes <= 10)
                {
                    return Colors.Green;
                }
                else if (timeElapsed.TotalMinutes > 10 && timeElapsed.TotalMinutes <= 15)
                {
                    return Colors.Yellow;
                }
                else
                {
                    return Colors.Red;
                }
            }
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
