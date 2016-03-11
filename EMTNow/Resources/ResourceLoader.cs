using System;
using Windows.UI.Xaml;

namespace EMTNow.Resources
{
    /// <summary>
    /// Clase para cargar literales traducidos de la aplicación.
    /// </summary>
    public static class ResourceLoader
    {
        public static string GetResourceString(string key)
        {
            var result = string.Empty;
            var resourceLoader = Application.Current.Resources["ResourceLoader"] as Windows.ApplicationModel.Resources.ResourceLoader;
            if (resourceLoader == null)
            {
                throw new Exception("No se puede obtener ResourceLoader");
            }

            result = resourceLoader.GetString(key);
            return result;
        }
    }
}
