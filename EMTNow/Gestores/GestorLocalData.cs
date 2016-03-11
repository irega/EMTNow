using EMTNow.Comun;
using EMTNow.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace EMTNow.Gestores
{
    public class GestorLocalData
    {
        /// <summary>
        /// Obtiene las paradas favoritas del usuario.
        /// </summary>
        /// <returns>Una lista de paradas favoritas.</returns>
        public async Task<IList<ParadaFavorita>> ObtenerParadasFavoritas()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            var result = new List<ParadaFavorita>();
            try
            {
                //Leemos el archivo JSON.
                StorageFile textFile = await localFolder.GetFileAsync(Constantes.LocalData.ParadasFavoritasFileName);
                using (IRandomAccessStream textStream = await textFile.OpenReadAsync())
                {
                    using (DataReader textReader = new DataReader(textStream))
                    {
                        uint textLength = (uint)textStream.Size;
                        await textReader.LoadAsync(textLength);
                        string jsonContents = textReader.ReadString(textLength);
                        //Deserializamos.
                        result = JsonConvert.DeserializeObject<IList<ParadaFavorita>>(jsonContents) as List<ParadaFavorita>;
                    }
                }
            }
            //Controlamos la excepción específica si el archivo no existe.
            catch (FileNotFoundException)
            {
                return new List<ParadaFavorita>();
            }
            return result;
        }

        /// <summary>
        /// Guarda las paradas favoritas del usuario.
        /// </summary>
        /// <param name="paradasFavoritas">Paradas favoritas a guardar.</param>
        public async void GuardarParadasFavoritas(IList<ParadaFavorita> paradasFavoritas)
        {
            //Serializamos a JSON. 
            string jsonContents = JsonConvert.SerializeObject(paradasFavoritas);
            
            //Creamos el archivo.
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile textFile = await localFolder.CreateFileAsync(Constantes.LocalData.ParadasFavoritasFileName,
                                         CreationCollisionOption.ReplaceExisting);
            //Escribimos el JSON.
            using (IRandomAccessStream textStream = await textFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (DataWriter textWriter = new DataWriter(textStream))
                {
                    textWriter.WriteString(jsonContents);
                    await textWriter.StoreAsync();
                }
            }
        }
    }
}
