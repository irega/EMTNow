using Windows.Phone.UI.Input;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace EMTNow.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AcercaDe : Page
    {
        public AcercaDe()
        {
            this.InitializeComponent();
            
            var tituloAplicacion = EMTNow.Resources.ResourceLoader.GetResourceString("TituloAplicacion");
            var version = Windows.ApplicationModel.Package.Current.Id.Version;
            var versionActual = string.Format("{0} v{1}.{2}.{3}.{4}", tituloAplicacion, version.Major, version.Minor, version.Build, version.Revision);
            lblVersionAplicacion.Text = versionActual;

            var copyright = EMTNow.Resources.ResourceLoader.GetResourceString("CopyrightText");
            lblCopyright.Text = copyright;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed += App.HardwareButtons_BackPressed;
        }

        /// <summary>
        /// Evento que se dispara al abandonar la vista.
        /// </summary>
        /// <param name="e">Argumentos del evento.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= App.HardwareButtons_BackPressed;
        }
    }
}
