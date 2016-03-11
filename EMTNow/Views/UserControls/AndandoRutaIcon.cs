using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace EMTNow.Views.UserControls
{
    public sealed class AndandoRutaIcon : Control
    {
        public AndandoRutaIcon()
        {
            this.DefaultStyleKey = typeof(AndandoRutaIcon);
        }

        public int MinutosRuta { get; set; }

        public Visibility DetalleVisible
        {
            get { return (Visibility)GetValue(DetalleVisibleProperty); }
            set { SetValue(DetalleVisibleProperty, value); }
        }

        public static readonly DependencyProperty DetalleVisibleProperty =
            DependencyProperty.Register("DetalleVisible", typeof(Visibility), typeof(AndandoRutaIcon), new PropertyMetadata(Visibility.Collapsed));
    }
}
