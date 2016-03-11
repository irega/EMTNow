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
    #region Destino

    /// <summary>
    /// Converter para el destino del bus.
    /// </summary>
    public class DestinoConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var texto = ResourceLoader.GetResourceString("DestinoText");
            return string.Format("{0} {1}", texto, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region IdLinea

    /// <summary>
    /// Converter para el ID de línea del bus.
    /// </summary>
    public class IdLineaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var texto = ResourceLoader.GetResourceString("LineaText");
            return string.Format("{0} {1}", texto, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    #endregion

    #region TiempoRestante

    /// <summary>
    /// Converter para el tiempo de espera del bus.
    /// </summary>
    public class TiempoRestanteConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int segundos;
            if (int.TryParse(value.ToString(), out segundos))
            {
                string textoMinutos; 
                if (segundos == 0)
                {
                    var textoEnParada = ResourceLoader.GetResourceString("EnParadaText");
                    return textoEnParada;
                }
                else if (segundos == 999999)
                {
                    textoMinutos= ResourceLoader.GetResourceString("MinutosText");
                    return string.Format(">20 {0}", textoMinutos);
                }

                textoMinutos = ResourceLoader.GetResourceString("MinutosText");
                var textoMinuto = ResourceLoader.GetResourceString("MinutoText");

                var minutos = segundos / 60;
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

    #region Distancia 

    /// <summary>
    /// Converter para la distancia a la parada del bus.
    /// </summary>
    public class DistanciaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var texto = ResourceLoader.GetResourceString("MetrosText");
            return string.Format("{0} {1}", value, texto);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    #endregion
}
