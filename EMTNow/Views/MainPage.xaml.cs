using EMTNow.ViewModels;
using EMTNow.Views.UserControls;
using GalaSoft.MvvmLight.Command;
using System;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Phone.UI.Input;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace EMTNow.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        /// <summary>
        /// Barras de la pantalla.
        /// </summary>
        private CommandBar _tiempoEsperaBar;
        private CommandBar _favoritasBar;
        private CommandBar _favoritasSelecBar;
        private CommandBar _mapaBar;

        /// <summary>
        /// Índice de la vista del mapa.
        /// </summary>
        private int _indiceVistaMapa = 2;

        /// <summary>
        /// Popup para edición de paradas favoritas.
        /// </summary>
        private Popup _favoritaPopup;

        /// <summary>
        /// Copia de la barra actual de la pantalla.
        /// </summary>
        private AppBar _snapshotBar;

        /// <summary>
        /// Modelo de la vista.
        /// </summary>
        private MainViewModel ViewModel
        {
            get
            {
                return this.DataContext as MainViewModel;
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;

            if (ViewModel != null)
            {
                //Suscripción a eventos del modelo de vista.
                ViewModel.NotificarError += MuestraError;
                lstFavoritas.ItemClick += ViewModel.Click_Favorita;

                //Creamos las barras de la pantalla.
                CreaBars(ViewModel);
            }

            App.VisibilidadAppDelegate = OnVisibilidadAppChanged;
        }

        /// <summary>
        /// Acción a ejecutar cuando cambia la visibilidad de la aplicación.
        /// </summary>
        /// <param name="visible">Valor binario que indica si la aplicación se ha vuelto visible o no.</param>
        private void OnVisibilidadAppChanged(bool visible)
        {
            if (!visible && pivot.SelectedIndex == _indiceVistaMapa)
            {
                GestionarTimersMapa(false);
            }
            else if (pivot.SelectedIndex == _indiceVistaMapa)
            {
                GestionarTimersMapa(true);
            }
        }

        #region Mapa

        //Variables para la sección del mapa.
        private Geopoint _centroMapaActual = null;
        private Geolocator _geoLocator = null;
        private DispatcherTimer _mapCenterTimer = null;
        private DispatcherTimer _mapUbicacionTimer = null;

        /// <summary>
        /// Inicializamos el mapa de paradas.
        /// </summary>
        private async void CargarMapa()
        {
            GestionarBotonCalculoRuta(false);

            var cargaInicial = _geoLocator == null;
            _geoLocator = _geoLocator ?? new Geolocator();
            if (cargaInicial)
            {
                await EstablecerPosicionActual(true);
            }

            //Cada segundo comprobamos si se ha movido el mapa para cargar las nuevas paradas.
            _mapCenterTimer = _mapCenterTimer ?? new DispatcherTimer();
            if (cargaInicial)
            {
                _mapCenterTimer.Tick += async (object sender, object e) =>
                {
                    if (_geoLocator.LocationStatus == PositionStatus.Disabled)
                    {
                        borderMapaCargando.Visibility = Visibility.Visible;
                        lblUbicacionDesactivada.Visibility = Visibility.Visible;
                        cnvLoadingMapa.Visibility = Visibility.Collapsed;
                    }
                    else if (_centroMapaActual != ctrlMapa.Center)
                    {
                        GestionarBotonCalculoRuta(false);
                        _centroMapaActual = ctrlMapa.Center;
                        await EstablecerPosicionParadas();
                        GestionarBotonCalculoRuta(true);
                    }
                };
                _mapCenterTimer.Interval = new TimeSpan(0, 0, 1);
            }
            _mapCenterTimer.Start();

            //Cada 35 segundos comprobamos si la posición del usuario ha cambiado.
            _mapUbicacionTimer = _mapUbicacionTimer ?? new DispatcherTimer();
            if (cargaInicial)
            {
                _mapUbicacionTimer.Tick += async (object sender, object e) =>
                {
                    if (_geoLocator.LocationStatus == PositionStatus.Disabled)
                    {
                        borderMapaCargando.Visibility = Visibility.Visible;
                        lblUbicacionDesactivada.Visibility = Visibility.Visible;
                        cnvLoadingMapa.Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        GestionarBotonCalculoRuta(false);
                        await EstablecerPosicionActual();
                        GestionarBotonCalculoRuta(true);
                    }
                };
                _mapUbicacionTimer.Interval = new TimeSpan(0, 0, 35);
            }
            _mapUbicacionTimer.Start();
            GestionarBotonCalculoRuta(true);
        }

        /// <summary>
        /// Dibuja en el mapa la posición actual del usuario.
        /// </summary>
        /// <param name="centrarMapa">Valor binario que indica si hay que posicionar el control del mapa.</param>
        private async Task<bool> EstablecerPosicionActual(bool centrarMapa = false)
        {
            borderMapaCargando.Visibility = Visibility.Visible;
            cnvLoadingMapa.Visibility = Visibility.Visible;
            lblUbicacionDesactivada.Visibility = Visibility.Collapsed;
            Geoposition posicion = null;
            try
            {
                posicion = await _geoLocator.GetGeopositionAsync();
            }
            catch (Exception ex)
            {
                if ((uint)ex.HResult == 0x80004004)
                {
                    lblUbicacionDesactivada.Visibility = Visibility.Visible;
                    cnvLoadingMapa.Visibility = Visibility.Collapsed;
                    return false;
                }
                else
                {
                    throw ex;
                }
            }

            if (posicion != null && posicion.Coordinate != null)
            {
                if (centrarMapa)
                {
                    ctrlMapa.Center = posicion.Coordinate.Point;
                    ctrlMapa.ZoomLevel = 15;
                }
                ctrlMapa.MapElements.Clear();
                var mapIcon = new MapIcon();
                mapIcon.Image = RandomAccessStreamReference.CreateFromUri(
                  new Uri("ms-appx:///Images/appbar.location.circle.png"));
                mapIcon.NormalizedAnchorPoint = new Point(0.15, 0.15);
                mapIcon.Location = posicion.Coordinate.Point;
                var texto = EMTNow.Resources.ResourceLoader.GetResourceString("UbicacionActualText");
                mapIcon.Title = texto;
                ctrlMapa.MapElements.Add(mapIcon);
            }
            borderMapaCargando.Visibility = Visibility.Collapsed;
            return true;
        }

        /// <summary>
        /// Dibuja en el mapa las paradas próximas.
        /// </summary>
        private async Task<bool> EstablecerPosicionParadas()
        {
            borderMapaCargando.Visibility = Visibility.Visible;
            cnvLoadingMapa.Visibility = Visibility.Visible;
            lblUbicacionDesactivada.Visibility = Visibility.Collapsed;
            ctrlMapa.Children.Clear();
            var posicionParadas = await ViewModel.ObtenerPosicionParadas(ctrlMapa.Center);
            foreach (var item in posicionParadas)
            {
                var basicGeo = new BasicGeoposition
                {
                    Longitude = item.CoordinateX,
                    Latitude = item.CoordinateY
                };
                var customIcon = new ParadaMapIcon();
                customIcon.CoordenadaX = item.CoordinateX;
                customIcon.CoordenadaY = item.CoordinateY;
                customIcon.IdParada = item.IdStop;
                customIcon.Tapped += CustomIcon_Tapped;

                MapControl.SetLocation(customIcon, new Geopoint(basicGeo));
                ctrlMapa.Children.Add(customIcon);
            }
            borderMapaCargando.Visibility = Visibility.Collapsed;
            return true;
        }

        /// <summary>
        /// Evento que se lanza al pulsar una parada en el mapa.
        /// </summary>
        /// <param name="sender">Objeto que desencadena el evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void CustomIcon_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            var customIcon = (ParadaMapIcon)sender;
            if (customIcon != null)
            {
                ViewModel.CodigoParada = customIcon.IdParada.ToString();
                ViewModel.IndicePagina = 1;
                ViewModel.ConsultarTiempo();
            }
        }

        /// <summary>
        /// Desactiva los timers utilizados por la sección del mapa.
        /// </summary>
        /// <param name="activar">Valor binario que indica si hay que activar los timers.</param>
        private void GestionarTimersMapa(bool activar)
        {
            if (!activar)
            {
                if (_mapCenterTimer != null)
                {
                    _mapCenterTimer.Stop();
                }
                if (_mapUbicacionTimer != null)
                {
                    _mapUbicacionTimer.Stop();
                }
            }
            else
            {
                if (_mapCenterTimer != null)
                {
                    _mapCenterTimer.Start();
                }
                if (_mapUbicacionTimer != null)
                {
                    _mapUbicacionTimer.Start();
                }
            }
        }

        #endregion

        #region Bars

        /// <summary>
        /// Crea las barras inferiores de la vista.
        /// </summary>
        /// <param name="viewModel">Modelo de la vista principal.</param>
        private void CreaBars(MainViewModel viewModel)
        {
            CreaTiempoEsperaBar();
            CreaFavoritasBars();
            CreaMapaBars();
        }

        private AppBarButton CreaAcercaDeButton()
        {
            var acercaDeButton = new AppBarButton();
            var acercaDeTexto = EMTNow.Resources.ResourceLoader.GetResourceString("AcercaDeCommand");
            acercaDeButton.Label = acercaDeTexto;
            acercaDeButton.Click += _acercaDeButton_Click;
            return acercaDeButton;
        }

        private void _acercaDeButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AcercaDe));
        }

        private void CreaMapaBars()
        {
            //Barra para la sección de mapa de paradas.
            _mapaBar = new CommandBar();

            //Botón posicionar en ubicación actual.
            var ubicacionActualBtn = new AppBarButton();
            var ubicacionActualIcon = new BitmapIcon();
            ubicacionActualIcon.UriSource = new Uri("ms-appx:///Images/appbar.location.circle_white.png");
            ubicacionActualBtn.Icon = ubicacionActualIcon;
            var textoUbicacionActual = EMTNow.Resources.ResourceLoader.GetResourceString("UbicacionActualCommand");
            ubicacionActualBtn.Label = textoUbicacionActual;
            ubicacionActualBtn.Click += UbicacionActualBtn_Click; 
            _mapaBar.PrimaryCommands.Add(ubicacionActualBtn);

            //Cálculo de ruta.
            var calcularRutaBtn = new AppBarButton();
            var calcularRutaIcon = new BitmapIcon();
            calcularRutaIcon.UriSource = new Uri("ms-appx:///Images/appbar.transit.connection.png");
            calcularRutaBtn.Icon = calcularRutaIcon;
            var textoCalcularRuta = EMTNow.Resources.ResourceLoader.GetResourceString("CalcularRutaCommand");
            calcularRutaBtn.Label = textoCalcularRuta;
            calcularRutaBtn.Click += calcularTrayecto_Click;
            _mapaBar.PrimaryCommands.Add(calcularRutaBtn);

            //Acerca de...
            var acercaDeButton = CreaAcercaDeButton();
            _mapaBar.SecondaryCommands.Add(acercaDeButton);
        }

        /// <summary>
        /// Controla el habilitado del botón de cálculo de ruta.
        /// </summary>
        /// <param name="activar"></param>
        private void GestionarBotonCalculoRuta(bool activar)
        {
            var botonCalcularRuta = _mapaBar.PrimaryCommands[1] as AppBarButton;
            if (botonCalcularRuta != null)
            {
                botonCalcularRuta.IsEnabled = activar;
            }
        }

        private void calcularTrayecto_Click(object sender, RoutedEventArgs e)
        {
            GestionarTimersMapa(false);
            Frame.Navigate(typeof(CalcularRuta));
        }

        private async void UbicacionActualBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_geoLocator.LocationStatus != PositionStatus.Disabled)
            {
                await EstablecerPosicionActual(true);
            }
        }

        private void CreaFavoritasBars()
        {
            //Barra para la sección de paradas favoritas.
            _favoritasBar = new CommandBar();

            //Botón selección múltiple de paradas.
            var selecMultipleBtn = new AppBarButton();
            var selecMultipleIcon = new BitmapIcon();
            selecMultipleIcon.UriSource = new Uri("ms-appx:///Images/appbar.list.check.png");
            selecMultipleBtn.Icon = selecMultipleIcon;
            var textoSeleccionar = EMTNow.Resources.ResourceLoader.GetResourceString("SeleccionarCommand");
            selecMultipleBtn.Label = textoSeleccionar;
            selecMultipleBtn.Click += selecMultipleBtn_Click;
            _favoritasBar.PrimaryCommands.Add(selecMultipleBtn);
            //Acerca de...
            var acercaDeButton = CreaAcercaDeButton();
            _favoritasBar.SecondaryCommands.Add(acercaDeButton);

            //Barra para cuando se activa la selección de favoritas.
            _favoritasSelecBar = new CommandBar();

            //Botón eliminación paradas favoritas.
            var deleteBtn = new AppBarButton();
            var deleteIcon = new BitmapIcon();
            deleteIcon.UriSource = new Uri("ms-appx:///Images/delete.png");
            deleteBtn.Icon = deleteIcon;
            var textoEliminar = EMTNow.Resources.ResourceLoader.GetResourceString("EliminarCommand");
            deleteBtn.Label = textoEliminar;
            deleteBtn.Click += deleteFavoritaBtn_Click;
            _favoritasSelecBar.PrimaryCommands.Add(deleteBtn);

            //Botón cancelación selección paradas favoritas.
            var cancelBtn = new AppBarButton();
            var cancelIcon = new BitmapIcon();
            cancelIcon.UriSource = new Uri("ms-appx:///Images/cancel.png");
            cancelBtn.Icon = cancelIcon;
            var textoCancelar = EMTNow.Resources.ResourceLoader.GetResourceString("CancelarText");
            cancelBtn.Label = textoCancelar;
            cancelBtn.Click += cancelarSelecFavoritaBtn_Click;
            _favoritasSelecBar.PrimaryCommands.Add(cancelBtn);
        }

        void cancelarSelecFavoritaBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            lstFavoritas.SelectionMode = ListViewSelectionMode.None;
            lstFavoritas.IsItemClickEnabled = true;
            BottomAppBar = _favoritasBar;
        }

        void deleteFavoritaBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            ViewModel.DeleteFavoritas(lstFavoritas.SelectedItems);
            lstFavoritas.SelectionMode = ListViewSelectionMode.None;
            lstFavoritas.IsItemClickEnabled = true;
            BottomAppBar = _favoritasBar;
        }

        void selecMultipleBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            lstFavoritas.IsItemClickEnabled = false;
            lstFavoritas.SelectionMode = ListViewSelectionMode.Multiple;
            BottomAppBar = _favoritasSelecBar;
        }

        private void CreaTiempoEsperaBar()
        {
            //Barra para la sección de tiempo de espera.
            _tiempoEsperaBar = new CommandBar();

            //Botón añadir parada a favoritas.
            var aniadirFavBtn = new AppBarButton();
            var aniadirFavIcon = new BitmapIcon();
            aniadirFavIcon.UriSource = new Uri("ms-appx:///Images/favs.addto.png");
            aniadirFavBtn.Icon = aniadirFavIcon;
            var favoritaCancelar = EMTNow.Resources.ResourceLoader.GetResourceString("AniadirFavoritaCommand");
            aniadirFavBtn.Label = favoritaCancelar;
            aniadirFavBtn.Click += aniadirFavBtn_Click;
            aniadirFavBtn.Command = ViewModel.AniadeFavorita;
            _tiempoEsperaBar.PrimaryCommands.Add(aniadirFavBtn);
            //Acerca de...
            var acercaDeButton = CreaAcercaDeButton();
            _tiempoEsperaBar.SecondaryCommands.Add(acercaDeButton);
        }

        #endregion

        /// <summary>
        /// Evento para añadir la parada actual como favorita.
        /// </summary>
        /// <param name="sender">Objeto que desencadena el evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        void aniadirFavBtn_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var appButton = sender as AppBarButton;
            if (appButton != null)
            {
                var command = appButton.Command as RelayCommand;
                if (command != null)
                {
                    if (command.CanExecute(ViewModel.CodigoParada))
                    {
                        //Creamos el popup.
                        _favoritaPopup = new Popup()
                        {
                            Height = Window.Current.Bounds.Height,
                            IsLightDismissEnabled = false,
                            Width = Window.Current.Bounds.Width
                        };

                        //Cargamos el control de usuario.
                        var controlUsuario = new ParadaFavoritaUc
                        {
                            Height = _favoritaPopup.Height,
                            Width = _favoritaPopup.Width
                        };

                        //Asignamos los eventos del popup.
                        controlUsuario.btnCancel.Click += new RoutedEventHandler(delegate (Object o, RoutedEventArgs a)
                        {
                            _favoritaPopup.IsOpen = false;
                            BottomAppBar = _snapshotBar;
                        });
                        controlUsuario.btnOK.Click += new RoutedEventHandler(delegate (Object o, RoutedEventArgs a)
                        {
                            //Validamos los datos de la parada introducidos y persistimos si todo es correcto.
                            var ok = ViewModel.ValidarFavorita(controlUsuario.txtNombre.Text);
                            if (ok)
                            {
                                ViewModel.AniadirFavorita(controlUsuario.txtNombre.Text);
                                _favoritaPopup.IsOpen = false;
                                BottomAppBar = _snapshotBar;
                            }
                        });

                        //Cargamos el control dentro del popup.
                        _favoritaPopup.Child = controlUsuario;

                        //Posicionamos el popup.
                        _favoritaPopup.SetValue(Canvas.LeftProperty, 0);
                        _favoritaPopup.SetValue(Canvas.TopProperty, 0);

                        //Y lo abrimos.
                        _favoritaPopup.IsOpen = true;

                        //Le damos el foco.
                        controlUsuario.Focus(FocusState.Programmatic);

                        //Deshabilitamos la barra hasta que se cierre el diálogo.
                        _snapshotBar = BottomAppBar;
                        BottomAppBar = null;
                    }
                }
            }
        }

        /// <summary>
        /// Evento que se desencadena al cambiar de vista en el control Pivot.
        /// </summary>
        /// <param name="sender">Objeto que desencadena el evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            GestionarTimersMapa(false);
            //Dependiendo de la pantalla mostramos una barra u otra.
            switch (pivot.SelectedIndex)
            {
                case 0:
                    BottomAppBar = _favoritasBar;
                    break;
                case 1:
                    BottomAppBar = _tiempoEsperaBar;
                    break;
                case 2:
                    BottomAppBar = _mapaBar;
                    CargarMapa();
                    break;
            }
        }

        /// <summary>
        /// Muestra un mensaje de error en la vista.
        /// </summary>
        /// <param name="sender">Objeto que desencadena el evento.</param>
        /// <param name="error">Mensaje de error.</param>
        private async void MuestraError(object sender, string error)
        {
            var tituloText = EMTNow.Resources.ResourceLoader.GetResourceString("TituloErrorText");
            var dialog = new MessageDialog(error, tituloText);
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.

            HardwareButtons.BackPressed += App.HardwareButtons_BackPressed;
            if (pivot.SelectedIndex == _indiceVistaMapa)
            {
                GestionarTimersMapa(true);
            }
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
