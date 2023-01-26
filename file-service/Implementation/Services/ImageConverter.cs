using System;
using System.IO;
using System.Text.RegularExpressions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace AIQXFileService.Implementation.Services
{
    public class ImageConverter
    {
        private readonly ConfigService _config;

        public ImageConverter(ConfigService configService)
        {
            _config = configService;
        }

        public MemoryStream GenerateThumbnailIfRequested(Stream imgStream, string type, string rgbColor, string desiredFit,
            int? desiredWidth, int? desiredHeight)
        {
            (byte r, byte g, byte b, int alpha) bgFill = (0, 0, 0, 0);
            //if the given height or width is null we will fetch the MaxImageSize from the config 
            var width = Math.Min(desiredWidth ?? Math.Max(0, _config.MaxImageSize()), _config.MaxImageSize());
            var height = Math.Min(desiredHeight ?? Math.Max(0, _config.MaxImageSize()), _config.MaxImageSize());

            var regex = new Regex(@"^#?([0-9a-f]{6})$");
            var matchRgbColor = regex.Match(rgbColor ?? string.Empty);

            if (matchRgbColor.Success)
            {
                // Prevent name collision
                var color = System.Drawing.ColorTranslator.FromHtml(matchRgbColor.Value);
                bgFill.r = color.R;
                bgFill.g = color.G;
                bgFill.b = color.B;
                bgFill.alpha = 1;
            }

            // not sure about it please check if this is ok
            //is the convertion for the frontend
            var resizeOption = desiredFit switch
            {
                "cover" => ResizeMode.Crop,
                "contain" => ResizeMode.Pad,
                "fill" => ResizeMode.BoxPad,
                "inside" => ResizeMode.Min,
                "outside" => ResizeMode.Max,
                _ => ResizeMode.Stretch
            };

            if (type == "image/png")
            {
                using var image = Image.Load(imgStream);
                image.Mutate(x => x
                    .Resize(new ResizeOptions()
                    {
                        Size = new Size(width, height),
                        Mode = resizeOption
                    })
                    .AutoOrient()
                    .BackgroundColor(new Rgba32(bgFill.r, bgFill.g, bgFill.b, bgFill.alpha)));

                var mem = new MemoryStream();
                image.SaveAsPng(mem);
                mem.Position = 0;
                //returns as png
                return mem;
            }
            else
            {
                using var image = Image.Load(imgStream);
                image.Mutate(x => x
                    .Resize(new ResizeOptions()
                    {
                        Size = new Size(width, height),
                        Mode = resizeOption
                    })
                    .AutoOrient()
                    .BackgroundColor(new Rgba32(bgFill.r, bgFill.g, bgFill.b, bgFill.alpha)));

                var mem = new MemoryStream();
                image.SaveAsPng(mem);
                mem.Position = 0;
                //returns as png
                return mem;
            }
        }
    }
}