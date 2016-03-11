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
using Windows.Services.Maps;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace EMTNow.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class RutaCalculada : Page
    {
        private GestorWS _gestorWs;
        private DatosCalculoRuta _datosCalculo;

        public RutaCalculada()
        {
            InitializeComponent();
            _gestorWs = new GestorWS();
            ctrlMapa.MapTapped += CtrlMapa_MapTapped;
        }

        /// <summary>
        /// Oculta los detalles de los iconos del mapa.
        /// </summary>
        /// <param name="sender">Objeto que desencadena el evento.</param>
        /// <param name="args">Argumentos del evento.</param>
        private void CtrlMapa_MapTapped(MapControl sender, MapInputEventArgs args)
        {
            foreach (var child in ctrlMapa.Children)
            {
                var paradaIcon = child as BusRutaIcon;
                if (paradaIcon != null)
                {
                    paradaIcon.DetalleVisible = Visibility.Collapsed;
                }
                else
                {
                    var andandoIcon = child as AndandoRutaIcon;
                    if (andandoIcon != null)
                    {
                        andandoIcon.DetalleVisible = Visibility.Collapsed;
                    }
                }
            }
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed += App.HardwareButtons_BackPressed;
            _datosCalculo = e.Parameter as DatosCalculoRuta;
            CalcularRuta();
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

        /// <summary>
        /// Calcula la ruta con los datos proporcionados.
        /// </summary>
        private async void CalcularRuta()
        {
            if (_datosCalculo != null)
            {
                borderCargando.Visibility = Visibility.Visible;
                var nodos = await _gestorWs.GetRutaCalculada(_datosCalculo);
                if (nodos == null)
                {
                    borderCargando.Visibility = Visibility.Collapsed;
                    var errorCalculo = EMTNow.Resources.ResourceLoader.GetResourceString("ErrorCalculoRutaText");
                    MuestraError(errorCalculo);
                    return;
                }

                var nodosOrdenados = nodos.OrderBy(n1 => n1.Orden);
                var primerNodo = nodosOrdenados.FirstOrDefault();
                if (primerNodo != null)
                {
                    var primerNodoGeo = new BasicGeoposition
                    {
                        Longitude = primerNodo.PosX,
                        Latitude = primerNodo.PosY
                    };
                    ctrlMapa.Center = new Geopoint(primerNodoGeo);
                    ctrlMapa.ZoomLevel = 15;
                }

                var pasos = nodos.Select(n => n.Orden).Distinct();
                foreach (var p in pasos)
                {
                    var polyline = new MapPolyline();
                    var nodosPaso = nodos.Where(n => n.Orden == p);
                    var geoPasos = new List<BasicGeoposition>();

                    NodoRutaCalculada primerNodoPaso = null;
                    var color = Color.FromArgb(255, 255, 255, 0);
                    foreach (var g in nodosPaso)
                    {
                        var basicGeo = new BasicGeoposition
                        {
                            Longitude = g.PosX,
                            Latitude = g.PosY
                        };
                        geoPasos.Add(basicGeo);

                        if (primerNodoPaso == null)
                        {
                            primerNodoPaso = g;
                            Control customIcon = null;
                            if (primerNodoPaso.EsAndando)
                            {
                                var andandoIcon = new AndandoRutaIcon();
                                andandoIcon.MinutosRuta = primerNodoPaso.MinutosRuta;
                                andandoIcon.Tapped += CustomIcon_Tapped;
                                customIcon = andandoIcon;
                            }
                            else
                            {
                                color = Color.FromArgb(255, 50, 121, 193);
                                var busRutaIcon = new BusRutaIcon();
                                busRutaIcon.MinutosRuta = primerNodoPaso.MinutosRuta;
                                busRutaIcon.IdLinea = primerNodoPaso.IdLinea.HasValue ? primerNodoPaso.IdLinea.Value : -1;
                                busRutaIcon.IdParada = primerNodoPaso.IdParada.HasValue ? primerNodoPaso.IdParada.Value : -1;
                                busRutaIcon.Tapped += CustomIcon_Tapped;
                                customIcon = busRutaIcon;
                            }
                            MapControl.SetLocation(customIcon, new Geopoint(basicGeo));
                            ctrlMapa.Children.Add(customIcon);
                        }
                    }

                    polyline.StrokeThickness = 5;
                    polyline.StrokeColor = color;
                    polyline.Path = new Geopath(geoPasos);
                    ctrlMapa.MapElements.Add(polyline);
                }
                borderCargando.Visibility = Visibility.Collapsed;
            }
        }

        /// <summary>
        /// Hace visible el detalle del icono al pulsar sobre él.
        /// </summary>
        /// <param name="sender">Objeto que desencadena el evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void CustomIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var customIcon = sender as BusRutaIcon;
            if (customIcon != null)
            {
                customIcon.DetalleVisible = Visibility.Visible;
            }
            else
            {
                var andandoIcon = sender as AndandoRutaIcon;
                if (andandoIcon != null)
                {
                    andandoIcon.DetalleVisible = Visibility.Visible;
                }
            }
            e.Handled = true;
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
