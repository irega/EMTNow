using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using EMTNow.Comun;
using GalaSoft.MvvmLight;

namespace EMTNow.Models
{
    /// <summary>
    /// Modelo de datos del tiempo de espera de una parada.
    /// </summary>
    public class TiempoEspera : ObservableObject
    {
        public int IdStop { get; set; }
        public int idLine { get; set; }
        public bool IsHead { get; set; }
        public string Destination { get; set; }
        public int TimeLeftBus { get; set; }
        public int DistanceBus { get; set; }
        public Enumerados.PositionTypeBus PositionTypeBus { get; set; }
    }
}
