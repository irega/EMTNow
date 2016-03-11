using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace EMTNow.Views.UserControls
{
    public sealed class ParadaMapIcon : Control
    {
        public ParadaMapIcon()
        {
            this.DefaultStyleKey = typeof(ParadaMapIcon);
        }

        public double CoordenadaX { get; set; }
        public double CoordenadaY { get; set; }
        public int IdParada { get; set; }
    }
}
