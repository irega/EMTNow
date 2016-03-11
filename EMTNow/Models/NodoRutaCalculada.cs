namespace EMTNow.Models
{
    public class NodoRutaCalculada
    {
        public int Orden{ get; set; }
        public double PosX{ get; set; }
        public double PosY { get; set; }
        public bool EsAndando { get; set; }
        public int MinutosRuta{ get; set; }
        public int? IdParada { get; set; }
        public int? IdLinea { get; set; }
    }
}
