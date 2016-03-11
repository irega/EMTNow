namespace EMTNow.Comun
{
    /// <summary>
    /// Constantes de la aplicación.
    /// </summary>
    public static class Constantes
    {
        /// <summary>
        /// Funciones del servicio web Geo.
        /// </summary>
        public static class FuncionesGeoWs
        {
            public const string GetArriveStopMethodName = "getArriveStop";
            public const string GetStopsFromXYMethodName = "getStopsFromXY";
            public const string GetStreetMethodName = "GetStreet";
        }

        /// <summary>
        /// Funciones del servicio web Media.
        /// </summary>
        public static class FuncionesMediaWs
        {
            public const string GetStreetRoute = "GetStreetRoute";
        }

        /// <summary>
        /// Almacenamiento de datos locales.
        /// </summary>
        public static class LocalData
        {
            public const string ParadasFavoritasFileName = "paradasFavoritas.json";
        }

        /// <summary>
        /// Datos de conexión de los servicios web.
        /// </summary>
        public static class DatosWs
        {
            public const string UrlApiGeo = "https://servicios.emtmadrid.es:8443/geo/servicegeo.asmx";
            public const string UrlApiMedia = "https://servicios.emtmadrid.es:8443/servicemedia/servicemedia.asmx"; 
            public const string IdClient = "TODO";
            public const string PassKey = "TODO";
        }
    }
}
