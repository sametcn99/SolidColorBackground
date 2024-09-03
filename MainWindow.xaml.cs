using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using Windows.Storage;
using Windows.UI.Popups;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SolidColorBackground
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            // Set window size and position
            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(370, 600));

        }

        /// <summary>
        /// Sets the desktop wallpaper using the specified parameters.
        /// </summary>
        /// <param name="uAction">The action to perform.</param>
        /// <param name="uParam">Additional parameters for the action.</param>
        /// <param name="lpvParam">The parameter value.</param>
        /// <param name="fuWinIni">The flags that specify how the change should be applied.</param>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        /// <summary>
        /// The constant value for setting the desktop wallpaper.
        /// </summary>
        const int SPI_SETDESKWALLPAPER = 20;
        /// <summary>
        /// The constant value for updating the INI file.
        /// </summary>
        const int SPIF_UPDATEINIFILE = 0x01;
        /// <summary>
        /// The constant value for sending a change notification.
        /// </summary>
        const int SPIF_SENDCHANGE = 0x02;

        private readonly Bitmap bitmap = new Bitmap(1, 1);

        /// <summary>
        /// Event handler for the ColorChanged event of the ColorPicker control.
        /// </summary>
        /// <param name="sender">The ColorPicker control that raised the event.</param>
        /// <param name="args">The event arguments.</param>
        private async void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            try
            {
                // get the color from the color picker
                var color = colorPicker.Color;
                var solidColor = Color.FromArgb(color.A, color.R, color.G, color.B);

                // set the pixel color of the bitmap
                bitmap.SetPixel(0, 0, solidColor);

                // save the bitmap to a memory stream
                using (var memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    // save the memory stream to a temporary file
                    var tempFile = Path.GetTempFileName();
                    using (var fileStream = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                    {
                        await memoryStream.CopyToAsync(fileStream);
                    }

                    // set the bitmap as the windows background
                    var file = await StorageFile.GetFileFromPathAsync(tempFile);
                    Debug.WriteLine(file.Path);

                    SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, file.Path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

                    // cleanup the temporary file
                    File.Delete(tempFile);
                }
            }
            catch (Exception ex)
            {
                // log the exception details
                var messageDialog = new MessageDialog($"An error occurred while setting the background color: {ex.Message}");
                await messageDialog.ShowAsync();
            }
        }
    }
}