using System.Globalization;

namespace RestauranteMap.Models
{
    public class TypeToButtonColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var tipo = value.ToString();
            if (tipo == "Delivery")
            {
                return Color.FromHex("#FFD700");
            }
            else if (tipo == "Recoger")
            {
                return Color.FromHex("#ADD8E6");
            }
            return Colors.Blue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
