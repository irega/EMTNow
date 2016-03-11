using GalaSoft.MvvmLight;

namespace EMTNow.Models
{
    /// <summary>
    /// Modelo de datos la posición de una parada.
    /// </summary>
    public class PosicionParada : ObservableObject
    {
        public int IdStop { get; set; }
        public double CoordinateX { get; set; }
        public double CoordinateY { get; set; }
    }
}
