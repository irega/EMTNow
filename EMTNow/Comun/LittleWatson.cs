using System;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace EMTNow.Comun
{
    /// <summary>
    /// http://darkgenesis.zenithmoon.com/further-adventures-in-unhandled-exception-handling-for-win8/
    /// </summary>
    public static class LittleWatson
    {

        public static StorageFolder folder = Windows.Storage.ApplicationData.Current.TemporaryFolder;
        const string Filename = "LittleWatson.txt";
        const string EmailTarget = "emtnow@outlook.es";

        private static TypedEventHandler<DataTransferManager, DataRequestedEventArgs> handler;
        private static ErrorMessageInfo errormessage;

        public static void ReportException(UnhandledExceptionEventArgs ex, string extra)
        {
            try
            {

                errormessage = new ErrorMessageInfo()
                {
                    Usermessage = extra,
                    Info = ex.Message,
                    Exception = ex.Exception.Message,
                    ExceptionDetail = ex.Exception.StackTrace
                };
                Win8StorageHelper.SaveData(Filename, folder, errormessage);
            }
            catch (Exception)
            {
            }
        }

        public async static void CheckForPreviousException()
        {
            try
            {
                errormessage = null;
                errormessage = (ErrorMessageInfo)await Win8StorageHelper.LoadData(Filename, folder, typeof(ErrorMessageInfo));
                if (errormessage != null)
                {
                    ShowErrorMessageDialog();
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                if (errormessage != null)
                {
                    Win8StorageHelper.SafeDeleteFile(folder, Filename);
                }
            }
        }



        private static async void ShowErrorMessageDialog()
        {
            // Register handler for DataRequested events for sharing
            if (handler != null)
                DataTransferManager.GetForCurrentView().DataRequested -= handler;

            handler = new TypedEventHandler<DataTransferManager, DataRequestedEventArgs>(OnDataRequested);
            DataTransferManager.GetForCurrentView().DataRequested += handler;

            // Create the message dialog and set its content
            var tituloText = EMTNow.Resources.ResourceLoader.GetResourceString("TituloExcepcionNoControlada");
            var ocurrioExcepcionText = EMTNow.Resources.ResourceLoader.GetResourceString("OcurrioExcepcionText");
            var messageDialog = new MessageDialog(ocurrioExcepcionText, tituloText);

            // Add commands and set their callbacks; both buttons use the same callback function instead of inline event handlers
            var compartirText = EMTNow.Resources.ResourceLoader.GetResourceString("CompartirErrorText");
            messageDialog.Commands.Add(new UICommand(
                compartirText,
                new UICommandInvokedHandler(LittleWatson.CommandInvokedHandler)));
            var cancelarText = EMTNow.Resources.ResourceLoader.GetResourceString("CancelarText");
            messageDialog.Commands.Add(new UICommand(
                cancelarText,
                new UICommandInvokedHandler(LittleWatson.CommandInvokedHandler)));

            // Set the command that will be invoked by default
            messageDialog.DefaultCommandIndex = 0;

            // Set the command to be invoked when escape is pressed
            messageDialog.CancelCommandIndex = 1;

            // Show the message dialog
            await messageDialog.ShowAsync();
        }

        private static void CommandInvokedHandler(IUICommand command)
        {
            if (command.Label == "Cancel")
            {
                DataTransferManager.GetForCurrentView().DataRequested -= handler;
            }
            else
            {
                DataTransferManager.ShowShareUI();
            }
        }

        private static void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var request = args.Request;
            request.Data.Properties.Title = errormessage.Usermessage;
            request.Data.Properties.Description = errormessage.Info;

            // Share recipe text
            StringBuilder builder = new StringBuilder();
            var enviarAText = EMTNow.Resources.ResourceLoader.GetResourceString("EnviarExcepcionAText");
            builder.Append(string.Format("{0}:\r\n", enviarAText));
            builder.Append(EmailTarget);
            builder.AppendLine();

            var detalleErrorText = EMTNow.Resources.ResourceLoader.GetResourceString("DetalleErrorText");
            builder.Append(string.Format("{0}:\r\n", detalleErrorText));
            builder.Append(errormessage.Exception);

            builder.AppendLine();
            var infoAdicionalText = EMTNow.Resources.ResourceLoader.GetResourceString("InformacionAdicionalText");
            builder.Append(string.Format("\r\n{0}:\r\n", infoAdicionalText));
            builder.Append(errormessage.ExceptionDetail);
            builder.AppendLine();

            request.Data.SetText(builder.ToString());

            DataTransferManager.GetForCurrentView().DataRequested -= handler;
        }

    }


    public class ErrorMessageInfo
    {
        public string Usermessage;
        public string Info;
        public string Exception;
        public string ExceptionDetail;
    }
}
