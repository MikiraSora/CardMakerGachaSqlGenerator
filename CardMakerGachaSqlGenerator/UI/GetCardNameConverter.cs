using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace CardMakerGachaSqlGenerator.UI
{
    internal class GetCardNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int id && (CardManager.CardInfoMap?.TryGetValue(id, out var info) ?? false))
                return info.Name;

            return "<ID-NOT-EXIST>";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
