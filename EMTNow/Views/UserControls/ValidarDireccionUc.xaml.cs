using EMTNow.Models;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace EMTNow.Views.UserControls
{
    public sealed partial class ValidarDireccionUc : UserControl
    {
        public ValidarDireccionUc()
        {
            this.InitializeComponent();
        }

        private List<Direccion> _direcciones;
        public List<Direccion> Direcciones
        {
            get
            {
                return _direcciones;
            }
            set
            {
                _direcciones = value;
                this.lstDirecciones.ItemsSource = _direcciones;
            }
        }
    }
}
