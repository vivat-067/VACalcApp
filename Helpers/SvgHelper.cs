using Avalonia.Media;
using Avalonia.Svg.Skia;
using System;
using System.Collections.Generic;

namespace VACalcApp.Helpers
{
    public static class SvgHelper
    {
        private static readonly Dictionary<string, IImage> ImagesCache = new();

        public static IImage? LoadFromAssets(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return null;

            if (ImagesCache.TryGetValue(path, out var image)) return image;

            try
            {
                var uri = path.StartsWith("avares://") ? path : $"avares://VACalcApp/{path.TrimStart('/')}";

                var svgImage = new SvgImage { Source = SvgSource.Load(uri) };
                return ImagesCache[path] = svgImage;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SvgHelper Error]: {path} - {ex.Message}");
                return null;
            }
        }
    }
}