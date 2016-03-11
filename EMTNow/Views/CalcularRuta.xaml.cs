using EMTNow.Gestores;
using EMTNow.Models;
using EMTNow.Views.UserControls;
using System.Collections.Generic;
using System.Linq;
using Windows.Phone.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using System;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace EMTNow.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CalcularRuta : Page
    {
        private Popup _direccionesPopup;
        private ValidarDireccionUc _controlValidarDireccion;
        private GestorWS _gestorWs;
        private bool _validandoOrigen = true;
        private bool _validadoOrigen = false;
        private bool _validadoDestino = false;
        private string _ultimoOrigen = string.Empty;
        private string _ultimoDestino = string.Empty;
        private double _origenCoordenadaX;
        private double _origenCoordenadaY;
        private double _destinoCoordenadaX;
        private double _destinoCoordenadaY;

        public CalcularRuta()
        {
            InitializeComponent();

            _gestorWs = new GestorWS();
            rbMinTiempo.IsChecked = true;
            btnCalcularRuta.IsEnabled = false;
            btnCalcularRuta.Click += BtnCalcularRuta_Click;
            txtOrigenRuta.LostFocus += TxtOrigenRuta_LostFocus;
            txtDestinoRuta.LostFocus += TxtDestinoRuta_LostFocus;
            txtOrigenRuta.GotFocus += TxtOrigenRuta_GotFocus;
            txtDestinoRuta.GotFocus += TxtDestinoRuta_GotFocus;
            txtOrigenRuta.TextChanged += TxtOrigenRuta_TextChanged;
            txtDestinoRuta.TextChanged += TxtDestinoRuta_TextChanged;
        }

        private void TxtDestinoRuta_TextChanged(object sender, TextChangedEventArgs e)
        {
            _validadoDestino = false;
            GestionarHabilitarBtnCalcular();
        }

        private void TxtOrigenRuta_TextChanged(object sender, TextChangedEventArgs e)
        {
            _validadoOrigen = false;
            GestionarHabilitarBtnCalcular();
        }

        private void TxtDestinoRuta_GotFocus(object sender, RoutedEventArgs e)
        {
            txtOrigenRuta.IsEnabled = false;
        }

        private void TxtOrigenRuta_GotFocus(object sender, RoutedEventArgs e)
        {
            txtDestinoRuta.IsEnabled = false;
        }

        private void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            if (_direccionesPopup != null && _direccionesPopup.IsOpen)
            {
                _direccionesPopup.IsOpen = false;
                txtOrigenRuta.IsEnabled = true;
                txtDestinoRuta.IsEnabled = true;
            }
            else
            {
                Frame rootFrame = Window.Current.Content as Frame;
                if (rootFrame != null && rootFrame.CanGoBack)
                {
                    rootFrame.GoBack();
                }
            }
        }

        /// <summary>
        /// Muestra un mensaje de error en la vista.
        /// </summary>
        /// <param name="error">Mensaje de error.</param>
        private async void MuestraError(string error)
        {
            var tituloText = EMTNow.Resources.ResourceLoader.GetResourceString("TituloErrorText");
            var dialog = new MessageDialog(error, tituloText);
            await dialog.ShowAsync();
        }

        private async void TxtDestinoRuta_LostFocus(object sender, RoutedEventArgs e)
        {
            borderCargando.Visibility = Visibility.Visible;
            _validandoOrigen = false;

            if (string.IsNullOrEmpty(txtDestinoRuta.Text) || string.IsNullOrWhiteSpace(txtDestinoRuta.Text))
            {
                var direccionObligatoria = EMTNow.Resources.ResourceLoader.GetResourceString("DireccionObligatoriaText");
                txtOrigenRuta.IsEnabled = true;
                MuestraError(direccionObligatoria);
            }
            else
            {
                var direcciones = await _gestorWs.GetDirecciones(txtDestinoRuta.Text);
                if (direcciones == null || !direcciones.Any())
                {
                    var direccionNoEncontrada = EMTNow.Resources.ResourceLoader.GetResourceString("DireccionNoEncontradaText");
                    txtOrigenRuta.IsEnabled = true;
                    MuestraError(direccionNoEncontrada);
                }
                else
                {
                    MostrarListaDirecciones(direcciones);
                }
            }
            borderCargando.Visibility = Visibility.Collapsed;
        }

        private async void TxtOrigenRuta_LostFocus(object sender, RoutedEventArgs e)
        {
            borderCargando.Visibility = Visibility.Visible;
            _validandoOrigen = true;

            if (string.IsNullOrEmpty(txtOrigenRuta.Text) || string.IsNullOrWhiteSpace(txtOrigenRuta.Text))
            {
                var direccionObligatoria = EMTNow.Resources.ResourceLoader.GetResourceString("DireccionObligatoriaText");
                txtDestinoRuta.IsEnabled = true;
                MuestraError(direccionObligatoria);
            }
            else
            {
                var direcciones = await _gestorWs.GetDirecciones(txtOrigenRuta.Text);
                if (direcciones == null || !direcciones.Any())
                {
                    var direccionNoEncontrada = EMTNow.Resources.ResourceLoader.GetResourceString("DireccionNoEncontradaText");
                    txtDestinoRuta.IsEnabled = true;
                    MuestraError(direccionNoEncontrada);
                }
                else
                {
                    MostrarListaDirecciones(direcciones);
                }
            }
            borderCargando.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// Muestra el popup de validación de dirección.
        /// </summary>
        /// <param name="direcciones">Direcciones sugeridas.</param>
        private void MostrarListaDirecciones(IList<Direccion> direcciones)
        {
            //Creamos el popup.
            if (_direccionesPopup == null)
            {
                _direccionesPopup = new Popup()
                {
                    Height = Window.Current.Bounds.Height,
                    IsLightDismissEnabled = false,
                    Width = Window.Current.Bounds.Width
                };

                //Cargamos el control de usuario.
                _controlValidarDireccion = new ValidarDireccionUc
                {
                    Height = _direccionesPopup.Height,
                    Width = _direccionesPopup.Width
                };
                _controlValidarDireccion.lstDirecciones.ItemClick += LstDirecciones_ItemClick;

                //Cargamos el control dentro del popup.
                _direccionesPopup.Child = _controlValidarDireccion;

                //Posicionamos el popup.
                _direccionesPopup.SetValue(Canvas.LeftProperty, 0);
                _direccionesPopup.SetValue(Canvas.TopProperty, 0);
            }

            //Pasamos al control las direcciones sugeridas.
            _controlValidarDireccion.Direcciones = direcciones.ToList();

            //Abrimos el popup.
            _direccionesPopup.IsOpen = true;

            //Le damos el foco.
            _controlValidarDireccion.Focus(FocusState.Programmatic);
        }

        /// <summary>
        /// Controla el evento de selección de una dirección propuesta.
        /// </summary>
        /// <param name="sender">Objeto que desencadena el evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void LstDirecciones_ItemClick(object sender, ItemClickEventArgs e)
        {
            var direccionSelected = e.ClickedItem as Direccion;
            if (direccionSelected != null)
            {
                if (_validandoOrigen)
                {
                    txtOrigenRuta.TextChanged -= TxtOrigenRuta_TextChanged;
                    txtOrigenRuta.Text = direccionSelected.DescripcionCompleta;
                    txtOrigenRuta.TextChanged += TxtOrigenRuta_TextChanged;
                    _origenCoordenadaX = direccionSelected.CoordenadaX;
                    _origenCoordenadaY = direccionSelected.CoordenadaY;

                    txtDestinoRuta.IsEnabled = true;
                    _validadoOrigen = true;
                }
                else
                {
                    txtDestinoRuta.TextChanged -= TxtDestinoRuta_TextChanged;
                    txtDestinoRuta.Text = direccionSelected.DescripcionCompleta;
                    txtDestinoRuta.TextChanged += TxtDestinoRuta_TextChanged;
                    _destinoCoordenadaX = direccionSelected.CoordenadaX;
                    _destinoCoordenadaY = direccionSelected.CoordenadaY;

                    txtOrigenRuta.IsEnabled = true;
                    _validadoDestino = true;
                }
            }
            GestionarHabilitarBtnCalcular();
            _direccionesPopup.IsOpen = false;
        }

        private void GestionarHabilitarBtnCalcular()
        {
            if (_validadoOrigen && _validadoDestino)
            {
                btnCalcularRuta.IsEnabled = true;
            }
            else
            {
                btnCalcularRuta.IsEnabled = false;
            }
        }

        private void BtnCalcularRuta_Click(object sender, RoutedEventArgs e)
        {
            var tipoCalculoId = 1;
            if (rbMinRecorridoPie.IsChecked.HasValue && rbMinRecorridoPie.IsChecked.Value)
            {
                tipoCalculoId = 4;
            }
            else if (rbMinTransbordos.IsChecked.HasValue && rbMinTransbordos.IsChecked.Value)
            {
                tipoCalculoId = 3;
            }

            Frame.Navigate(typeof(RutaCalculada), new DatosCalculoRuta
            {
                Origen = txtOrigenRuta.Text,
                Destino = txtDestinoRuta.Text,
                TipoCalculoId = tipoCalculoId,
                OrigenCoordenadaX = _origenCoordenadaX,
                OrigenCoordenadaY = _origenCoordenadaY,
                DestinoCoordenadaX = _destinoCoordenadaX,
                DestinoCoordenadaY = _destinoCoordenadaY,
            });
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        /// <summary>
        /// Evento que se dispara al abandonar la vista.
        /// </summary>
        /// <param name="e">Argumentos del evento.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }
    }
}
