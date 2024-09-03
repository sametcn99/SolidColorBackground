using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

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
        }

        // P/Invoke kullanarak masaüstü arka planını değiştirme
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);

        const int SPI_SETDESKWALLPAPER = 20;
        const int SPIF_UPDATEINIFILE = 0x01;
        const int SPIF_SENDCHANGE = 0x02;

        private async void ColorPicker_ColorChanged(Microsoft.UI.Xaml.Controls.ColorPicker sender, Microsoft.UI.Xaml.Controls.ColorChangedEventArgs args)
        {
            try
            {
                Debug.WriteLine("SetBtn_Click");
                // get the color from the color picker
                var color = colorPicker.Color;
                var solidColor = Color.FromArgb(color.A, color.R, color.G, color.B);

                // create a bitmap with the color
                var bitmap = new Bitmap(1, 1);
                bitmap.SetPixel(0, 0, solidColor);

                // save the bitmap to a temporary png file
                var tempFile = System.IO.Path.GetTempFileName() + ".png";
                bitmap.Save(tempFile, ImageFormat.Png);

                // set the bitmap as the windows background
                var file = await Windows.Storage.StorageFile.GetFileFromPathAsync(tempFile);
                Debug.WriteLine(file.Path);

                SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, file.Path, SPIF_UPDATEINIFILE | SPIF_SENDCHANGE);

            }
            catch (Exception ex)
            {
                // log the exception details
                var messageDialog = new Windows.UI.Popups.MessageDialog($"An error occurred while setting the background color: {ex.Message}");
                await messageDialog.ShowAsync();
            }
        }
    }
}
