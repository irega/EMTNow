using EMTNow.Comun;
using EMTNow.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Devices.Geolocation;

namespace EMTNow.Gestores
{
    public class GestorWS
    {
        /// <summary>
        /// Método que genera la consulta a un servicio web,
        /// </summary>
        /// <param name="urlApi">URL del servicio web.</param>
        /// <param name="metodo">Nombre del método del servicio web.</param>
        /// <param name="parametros">Parámetros de la llamada.</param>
        /// <returns>Un objeto <see cref="XDocument"/> con la respuesta en XML.</returns>
        private async Task<XDocument> CallWebService(string urlApi, string metodo, IDictionary<string, string> parametros)
        {
            var httpClient = new HttpClient();
            var url = string.Format("{0}/{1}?idClient={2}&passKey={3}", urlApi, metodo,
                Constantes.DatosWs.IdClient, Constantes.DatosWs.PassKey);

            foreach (var key in parametros.Keys)
            {
                var value = string.Empty;
                if (parametros.TryGetValue(key, out value))
                {
                    url = string.Format("{0}&{1}={2}", url, key, WebUtility.UrlEncode(value));
                }
            }
            var content = await httpClient.GetStringAsync(url);

            XDocument xmlDoc = null;
            try
            {
                xmlDoc = XDocument.Parse(content);
            }
            catch
            {
                return null;
            }
            return xmlDoc;
        }

        /// <summary>
        /// Obtiene los tiempos de espera de una parada.
        /// </summary>
        /// <param name="idParada">Código de la parada.</param>
        /// <returns>Una lista de tiempos de espera.</returns>
        public async Task<IList<TiempoEspera>> GetTiemposEspera(int idParada)
        {
            var parametros = new Dictionary<string, string>
            {
                {"idStop" ,idParada.ToString()},
                {"statistics" ,string.Empty},
                {"cultureInfo" ,"es"}
            };
            var xmlResponse = await CallWebService(Constantes.DatosWs.UrlApiGeo,
                Constantes.FuncionesGeoWs.GetArriveStopMethodName, parametros);

            if (xmlResponse == null)
            {
                return null;
            }

            var data = (from elem in xmlResponse.Descendants("Arrive")
                        select new TiempoEspera
                        {
                            IdStop = (int)elem.Element("IdStop"),
                            idLine = (int)elem.Element("idLine"),
                            IsHead = (bool)elem.Element("IsHead"),
                            Destination = (string)elem.Element("Destination"),
                            TimeLeftBus = (int)elem.Element("TimeLeftBus"),
                            DistanceBus = (int)elem.Element("DistanceBus"),
                            PositionTypeBus = (Enumerados.PositionTypeBus)((int)elem.Element("PositionTypeBus"))
                        }).ToList();

            return data;
        }

        /// <summary>
        /// Consulta las paradas cercanas a un punto geográfico dado.
        /// </summary>
        /// <param name="centro">Punto geográfico de referencia.</param>
        /// <returns>Lista de paradas cercanas.</returns>
        public async Task<IList<PosicionParada>> GetParadasDesdePosicion(Geopoint centro)
        {
            var parametros = new Dictionary<string, string>
            {
                {"coordinateX" ,centro.Position.Longitude.ToString()},
                {"coordinateY" ,centro.Position.Latitude.ToString()},
                {"Radius" , 500.ToString()},
                {"statistics" ,string.Empty},
                {"cultureInfo" ,"es"}
            };

            var xmlResponse = await CallWebService(Constantes.DatosWs.UrlApiGeo,
              Constantes.FuncionesGeoWs.GetStopsFromXYMethodName, parametros);

            if (xmlResponse == null)
            {
                return null;
            }

            var data = (from elem in xmlResponse.Descendants("Stop")
                        select new PosicionParada
                        {
                            IdStop = (int)elem.Element("IdStop"),
                            CoordinateX = (double)elem.Element("CoordinateX"),
                            CoordinateY = (double)elem.Element("CoordinateY")
                        }).ToList();

            return data;
        }

        /// <summary>
        /// Obtiene una lista de direcciones coincidentes con el literal pasado por parámetro.
        /// </summary>
        /// <param name="direccion">Literal de la dirección a buscar.</param>
        /// <returns>Una lista de direcciones.</returns>
        public async Task<IList<Direccion>> GetDirecciones(string direccion)
        {
            //Nos devuelve el primer número que contenga la dirección.
            var numero = Regex.Match(direccion, @"\d+").Value;

            var parametros = new Dictionary<string, string>
            {
                {"description", direccion},
                {"streetNumber", numero},
                {"Radius", 1000.ToString()},
                {"Stops", string.Empty},
                {"statistics", string.Empty},
                {"cultureInfo" ,"es"}
            };
            var xmlResponse = await CallWebService(Constantes.DatosWs.UrlApiGeo,
                Constantes.FuncionesGeoWs.GetStreetMethodName, parametros);

            if (xmlResponse == null)
            {
                return null;
            }
            var data = (from elem in xmlResponse.Descendants("Site")
                        select new Direccion
                        {
                            Descripcion = (string)elem.Element("Description"),
                            TipoCalle = (string)elem.Element("StreetType"),
                            TipoNumero = (string)elem.Element("NumberType"),
                            Numero = (string)elem.Element("StreetNumber"),
                            CodigoPostal = elem.Element("ZipCode") != null ? (string)elem.Element("ZipCode") : string.Empty,
                            CoordenadaX = (double)elem.Element("CoordinateX"),
                            CoordenadaY = (double)elem.Element("CoordinateY")
                        }).ToList();

            return data;
        }

        /// <summary>
        /// Calcula una ruta.
        /// </summary>
        /// <param name="datosCalculo">Datos para el cálculo.</param>
        /// <returns>Nodos de una ruta calculada.</returns>
        public async Task<IList<NodoRutaCalculada>> GetRutaCalculada(DatosCalculoRuta datosCalculo)
        {
            var parametros = new Dictionary<string, string>
            {
                {"coordinateXFrom", datosCalculo.OrigenCoordenadaX.ToString()},
                {"coordinateYFrom", datosCalculo.OrigenCoordenadaY.ToString()},
                {"originName", datosCalculo.Origen},
                {"coordinateXTo", datosCalculo.DestinoCoordenadaX.ToString()},
                {"coordinateYTo", datosCalculo.DestinoCoordenadaY.ToString()},
                {"destinationName", datosCalculo.Destino},
                {"criteriaSelection", datosCalculo.TipoCalculoId.ToString()},
                {"statistics", string.Empty},
                {"cultureInfo" ,"es"},
                {"day", string.Empty},
                {"month", string.Empty},
                {"year", string.Empty},
                {"hour", string.Empty},
                {"minute", string.Empty},
                {"GenerarAudio", string.Empty}
            };
            var xmlResponse = await CallWebService(Constantes.DatosWs.UrlApiMedia,
                            Constantes.FuncionesMediaWs.GetStreetRoute, parametros);

            //Si no recibimos respuesta o el código del resultado no es 300, devolvemos null.
            if (xmlResponse == null || !xmlResponse.Descendants("CodError").Any())
            {
                return null;
            }
            var codigoRespuesta = xmlResponse.Descendants("CodError").FirstOrDefault();
            if (codigoRespuesta == null || codigoRespuesta.Value != "300")
            {
                return null;
            }

            var sections = (from elem in xmlResponse.Descendants("Section")
                            select new
                            {
                                Orden = (int)elem.Element("Order"),
                                EsAndando = elem.Element("WalkingLeg") != null,
                                MinutosRuta = elem.Element("WalkingLeg") != null
                                    ? (int)(elem.Element("WalkingLeg").Descendants("SourceWalkingLeg").FirstOrDefault().Element("TimeToSpend"))
                                    : (int)(elem.Element("BusLeg").Descendants("SourceBusLeg").FirstOrDefault().Element("TimeToSpend"))
                            }).ToList();

            var cultureEs = new CultureInfo("es-ES");
            var sectionsDetails = (from elem in xmlResponse.Descendants("ListRouteSection")
                                   from det in elem.Descendants("Routes")
                                   select new
                                   {
                                       Orden = (int)elem.Element("Order"),
                                       PosX = double.Parse(((string)det.Element("PosxNode")), cultureEs.NumberFormat),
                                       PosY = double.Parse(((string)det.Element("PosyNode")), cultureEs.NumberFormat),
                                       IdLinea = (int)det.Element("IdLine"),
                                       IdParada = (int)det.Element("Node")
                                   }).ToList();

            var result = (from elem in sectionsDetails
                          join s in sections on elem.Orden equals s.Orden
                          select new NodoRutaCalculada
                          {
                              Orden = elem.Orden,
                              PosX = elem.PosX,
                              PosY = elem.PosY,
                              EsAndando = s.EsAndando,
                              MinutosRuta = s.MinutosRuta,
                              IdParada = elem.IdParada,
                              IdLinea = elem.IdLinea
                          }).ToList();
            return result;
        }
    }
}
