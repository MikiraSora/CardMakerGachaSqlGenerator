using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace CardMakerGachaSqlGenerator.UI
{
    public partial class LoadTheaterImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int id)
                return new AsyncLoadImageTask(id);
            return default;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private partial class AsyncLoadImageTask : ObservableObject
        {
            public AsyncLoadImageTask(int theaterId)
            {
                var file = GameResourceManager.GetTheaterImageFile(theaterId);
                LoadValue(file);
            }

            private async void LoadValue(string imgFilePath)
            {
                if (!File.Exists(imgFilePath))
                    return;

                var imgData = await JacketGenerateWrapper.GetMainImageDataAsync(default, imgFilePath);

                AsyncValue = await Task.Run(() =>
                {
                    using var image = Image.LoadPixelData<Rgba32>(imgData.Data, imgData.Width, imgData.Height);
                    var memoryStream = new MemoryStream();
                    image.Mutate(i => i.Flip(FlipMode.Vertical));
                    image.SaveAsPng(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memoryStream;
                    bitmapImage.EndInit();

                    bitmapImage.Freeze();

                    return bitmapImage;
                });
            }


            [ObservableProperty]
            private BitmapImage asyncValue;
        }
    }
}
