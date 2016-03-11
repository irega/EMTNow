namespace EMTNow.Models
{
    /// <summary>
    /// Modelo de datos para el cálculo de una ruta.
    /// </summary>
    public class DatosCalculoRuta
    {
        public string Origen { get; set; }
        public string Destino { get; set; }
        public int TipoCalculoId { get; set; }
        public double OrigenCoordenadaX { get; set; }
        public double OrigenCoordenadaY { get; set; }
        public double DestinoCoordenadaX { get; set; }
        public double DestinoCoordenadaY { get; set; }
    }
}
