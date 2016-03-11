using EMTNow.Gestores;
using EMTNow.Models;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.UI.Xaml.Controls;

namespace EMTNow.ViewModels
{
    /// <summary>
    /// Modelo de vista de la vista principal.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel()
        {
            _gestorWs = new GestorWS();
            _gestorLocalData = new GestorLocalData();
            _progresoVisible = false;

            //Commands.
            ConsultaTiempo = new RelayCommand(ConsultarTiempo,
                 (() => { return ValidaCodigoParada(CodigoParada); }));
            AniadeFavorita = new RelayCommand(() => { },
             (() => { return ValidaCodigoParada(CodigoParada); }));

            //Colecciones.
            TiemposEspera = new ObservableCollection<TiempoEspera>();

            //Recuperamos paradas favoritas del usuario.
            CargarParadasFavoritas();
        }

        /// <summary>
        /// Carga las paradas favoritas guardadas anteriormente.
        /// </summary>
        private async void CargarParadasFavoritas()
        {
            var paradasFavoritas = await _gestorLocalData.ObtenerParadasFavoritas();
            ParadasFavoritas = new ObservableCollection<ParadaFavorita>(paradasFavoritas);
        }

        /// <summary>
        /// Evento para notificar a la vista un error de negocio.
        /// </summary>
        public EventHandler<string> NotificarError;

        /// <summary>
        /// Propiedad para controlar el índice de la página mostrada.
        /// </summary>
        private int _indicePagina;
        public int IndicePagina
        {
            get
            {
                return _indicePagina;
            }
            set
            {
                _indicePagina = value;
                RaisePropertyChanged("IndicePagina");
            }
        }

        #region Favoritas

        /// <summary>
        /// Gestor de datos locales del usuario.
        /// </summary>
        private readonly GestorLocalData _gestorLocalData;

        /// <summary>
        /// Lista de paradas favoritas.
        /// </summary>
        private ObservableCollection<ParadaFavorita> _paradasFavoritas;
        public ObservableCollection<ParadaFavorita> ParadasFavoritas
        {
            get { return _paradasFavoritas; }
            set
            {
                _paradasFavoritas = value;
                RaisePropertyChanged("ParadasFavoritas");
            }
        }

        #region AniadirFavorita

        /// <summary>
        /// Añade una parada a la lista de favoritas del usuario.
        /// </summary>
        /// <param name="nombreParada">Nombre personalizado de la parada.</param>
        public void AniadirFavorita(string nombreParada)
        {
            var newParada = new ParadaFavorita
            {
                IdParada = int.Parse(CodigoParada),
                Nombre = nombreParada
            };
            ParadasFavoritas.Add(newParada);
            _gestorLocalData.GuardarParadasFavoritas(ParadasFavoritas);
        }

        /// <summary>
        /// Validación de los datos de una parada favorita.
        /// </summary>
        /// <param name="nombreFavorita">Nombre de la parada favorita.</param>
        /// <returns>Valor binario con el resultado de la validación.</returns>
        public bool ValidarFavorita(string nombreFavorita)
        {
            if (string.IsNullOrEmpty(nombreFavorita))
            {
                var nombreObligatorioText = Resources.ResourceLoader.GetResourceString("NombreParadaObligatorio");
                NotificarError(this, nombreObligatorioText);
                return false;
            }
            return true;
        }

        /// <summary>
        /// Command para añadir una parada a favoritas.
        /// </summary>
        public RelayCommand AniadeFavorita
        {
            get;
            private set;
        }

        #endregion

        /// <summary>
        /// Evento para controlar el click de una parada favorita.
        /// </summary>
        /// <param name="sender">Objeto que desencadena el evento.</param>
        /// <param name="e">Argumentos del evento.</param>
        public void Click_Favorita(object sender, ItemClickEventArgs e)
        {
            var paradaSelected = e.ClickedItem as ParadaFavorita;
            if (paradaSelected != null)
            {
                CodigoParada = paradaSelected.IdParada.ToString();
                IndicePagina = 1;
                ConsultarTiempo();
            }
        }

        /// <summary>
        /// Elimina una lista de paradas favoritas del usuario.
        /// </summary>
        /// <param name="favoritasToDelete">Paradas favoritas a eliminar.</param>
        public void DeleteFavoritas(IList<object> favoritasToDelete)
        {
            if (favoritasToDelete != null)
            {
                var favoritasList = favoritasToDelete.ToList().Cast<ParadaFavorita>().ToList();
                var paradasToSave = ParadasFavoritas.Where(p => !favoritasList.Any(p1 => p1.IdParada == p.IdParada)).ToList();
                ParadasFavoritas = new ObservableCollection<ParadaFavorita>(paradasToSave);
                _gestorLocalData.GuardarParadasFavoritas(ParadasFavoritas);
            }
        }

        #endregion

        #region TiempoEspera

        /// <summary>
        /// Gestor de conexión con servicios web.
        /// </summary>
        private readonly GestorWS _gestorWs;

        /// <summary>
        /// Código de parada.
        /// </summary>
        private string _codigoParada;
        public string CodigoParada
        {
            get
            {
                return _codigoParada;
            }
            set
            {
                _codigoParada = value;
                RaisePropertyChanged("CodigoParada");
                AniadeFavorita.RaiseCanExecuteChanged();
                ConsultaTiempo.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Valor binario que indica cuando debe ser visible el indicador de progreso.
        /// </summary>
        private bool _progresoVisible;
        public bool ProgresoVisible
        {
            get { return _progresoVisible; }
            set
            {
                _progresoVisible = value;
                RaisePropertyChanged("ProgresoVisible");
            }
        }

        /// <summary>
        /// Lista de tiempos de espera de la parada.
        /// </summary>
        private ObservableCollection<TiempoEspera> _tiemposEspera;
        public ObservableCollection<TiempoEspera> TiemposEspera
        {
            get { return _tiemposEspera; }
            set
            {
                _tiemposEspera = value;
                RaisePropertyChanged("TiemposEspera");
            }
        }

        /// <summary>
        /// Command para consultar el tiempo de espera.
        /// </summary>
        public RelayCommand ConsultaTiempo { get; private set; }

        /// <summary>
        /// Valida el código de parada introducido.
        /// </summary>
        /// <param name="strCodigoParada">Literal con el código de parada a validar.</param>
        /// <returns>Valor binario que indica el resultado de la validación.</returns>
        private bool ValidaCodigoParada(string strCodigoParada)
        {
            int codigoParada;
            return int.TryParse(CodigoParada, out codigoParada);
        }

        /// <summary>
        /// Consulta del tiempo de espera.
        /// </summary>
        public async void ConsultarTiempo()
        {
            //Vaciamos antigua colección.
            TiemposEspera = new ObservableCollection<TiempoEspera>();
            ProgresoVisible = true;

            //Pintamos datos.
            var tiempos = await _gestorWs.GetTiemposEspera(int.Parse(CodigoParada));
            if (tiempos == null)
            {
                tiempos = new List<TiempoEspera>();
            }
            TiemposEspera = new ObservableCollection<TiempoEspera>(tiempos);
            ProgresoVisible = false;
        }

        #endregion

        #region Mapa

        public async Task<IList<PosicionParada>> ObtenerPosicionParadas(Geopoint centro)
        {
            var posiciones = await _gestorWs.GetParadasDesdePosicion(centro);
            if (posiciones == null)
            {
                posiciones = new List<PosicionParada>();
            }
            return posiciones;
        }

        #endregion
    }
}
