using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.Storage;
using Windows.Storage.Streams;

namespace EMTNow.Comun
{
    /// <summary>
    /// https://dzone.com/articles/working-isolated-storage
    /// </summary>
    public static class Win8StorageHelper
    {
        public static StorageFolder defaultFolder = Windows.Storage.ApplicationData.Current.RoamingFolder;

        public async static Task<bool> SaveData(string path, object data)
        {
            return await SaveData(path, defaultFolder, data);
        }

        public async static Task<bool> SaveData(string path, StorageFolder folder, object data)
        {
            // settings
            var option = Windows.Storage.CreationCollisionOption.ReplaceExisting;

            try
            {
                var file = await folder.CreateFileAsync(path, option);
                MemoryStream saveData = new MemoryStream();

                XmlSerializer x = new XmlSerializer(data.GetType());
                x.Serialize(saveData, data);

                if (saveData.Length > 0)
                {

                    // Get an output stream for the SessionState file and write the state asynchronously
                    using (var fileStream = await file.OpenStreamForWriteAsync())
                    {
                        saveData.Seek(0, SeekOrigin.Begin);
                        await saveData.CopyToAsync(fileStream);
                        await fileStream.FlushAsync();
                    }
                    return true;
                }
            }
            catch { }

            return false;
        }

        public async static Task<object> LoadData(string path, System.Type type)
        {
            return await LoadData(path, defaultFolder, type);
        }

        public async static Task<object> LoadData(string path, StorageFolder folder, System.Type type)
        {
            try
            {
                var file = await folder.GetFileAsync(path);

                using (IInputStream inStream = await file.OpenSequentialReadAsync())
                {
                    // Deserialize the Session State
                    XmlSerializer x = new XmlSerializer(type);
                    return x.Deserialize(inStream.AsStreamForRead());
                }
            }
            catch { return null; }
        }

        public async static void DeleteFile(StorageFolder folder, string filename)
        {
            var file = await folder.GetFileAsync(filename);
            await file.DeleteAsync();
        }

        public static void SafeDeleteFile(StorageFolder folder, string filename)
        {
            try
            {
                DeleteFile(folder, filename);
            }
            catch (Exception ex)
            {
            }
        }
    }
}
