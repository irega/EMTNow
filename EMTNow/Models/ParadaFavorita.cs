using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace EMTNow.Models
{
    /// <summary>
    /// Modelo de datos para guardar paradas favoritas.
    /// </summary>
    public class ParadaFavorita : ObservableObject
    {
        public string Nombre { get; set; }
        public int IdParada { get; set; }
    }
}
