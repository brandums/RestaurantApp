using System.Globalization;

namespace RestauranteMap.Models
{
    public class ExistenciasColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double existencias && parameter is double stock)
            {
                double porcentaje = (existencias / stock) * 100;

                if (porcentaje <= 30)
                    return Colors.Red;
                else if (porcentaje <= 50)
                    return Colors.Yellow;
                else
                    return Colors.Green;
            }

            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
