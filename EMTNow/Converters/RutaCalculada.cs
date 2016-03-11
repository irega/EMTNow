using EMTNow.Resources;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace EMTNow.Converters
{

    #region MinutosRuta

    /// <summary>
    /// Converter para la duración de un paso de la ruta calculada.
    /// </summary>
    public class MinutosRutaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int minutos;
            if (int.TryParse(value.ToString(), out minutos))
            {
                var textoMinutos = ResourceLoader.GetResourceString("MinutosText");
                var textoMinuto = ResourceLoader.GetResourceString("MinutoText");

                var literalMinutos = minutos <= 1 ? textoMinuto : textoMinutos;
                return string.Format("{0} {1}", minutos, literalMinutos);
            }
            else
            {
                return string.Empty;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region IdParada 

    /// <summary>
    /// Converter para el código de parada
    /// </summary>
    public class IdParadaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var texto = ResourceLoader.GetResourceString("ParadaText");
            return string.Format("{0} {1}", texto, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
