using GalaSoft.MvvmLight;

namespace EMTNow.Models
{
    /// <summary>
    /// Modelo de datos para representar una dirección.
    /// </summary>
    public class Direccion : ObservableObject
    {
        public string Descripcion { get; set; }
        public string TipoCalle { get; set; }
        public string TipoNumero { get; set; }
        public string Numero { get; set; }
        public string CodigoPostal { get; set; }
        public double CoordenadaX { get; set; }
        public double CoordenadaY { get; set; }

        public string DescripcionCompleta
        {
            get
            {
                return string.Format("{0} {1}, {2} - {3}", TipoCalle, Descripcion, Numero, CodigoPostal);
            }
            private set { }
        }
    }
}
