using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using Windows.Storage;
using Windows.UI.Popups;
using WinRT.Interop;

namespace SolidColorBackground
{
    public sealed partial class MainWindow : Window
    {
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 1);

        public MainWindow()
        {
            this.InitializeComponent();

            // Set window size and position
            var hwnd = WindowNative.GetWindowHandle(this);
            var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            var appWindow = AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(400, 600));
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDCHANGE = 0x02;

        private readonly Bitmap bitmap = new Bitmap(1, 1);

        private async void ColorPicker_ColorChanged(ColorPicker sender, ColorChangedEventArgs args)
        {
            await semaphoreSlim.WaitAsync();
            try
            {
                var color = colorPicker.Color;
                var solidColor = Color.FromArgb(color.A, color.R, color.G, color.B);

                bitmap.SetPixel(0, 0, solidColor);

                using (var memoryStream = new MemoryStream())
                {
                    bitmap.Save(memoryStream, ImageFormat.Png);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    var picturesFolder = await KnownFolders.PicturesLibrary.CreateFileAsync("solid_color_background.png", CreationCollisionOption.ReplaceExisting);
                    using (var fileStream = await picturesFolder.OpenStreamForWriteAsync())
                    {
                        await memoryStream.CopyToAsync(fileStream);
                    }

                    SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, picturesFolder.Path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

                    Debug.WriteLine($"File saved to: {picturesFolder.Path}");
                }
            }
            catch (Exception ex)
            {
                var messageDialog = new MessageDialog($"An error occurred while setting the background color: {ex.Message}");
                await messageDialog.ShowAsync();
            }
            finally
            {
                semaphoreSlim.Release();
            }
        }
    }
}
