using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Json;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ImageMagick;
using Serilog;

namespace AulasSiencb2.Helpers
{
    class PrintUtils
    {
        public static JsonObject CaculatePercentageColor(string pdf_path)
        {
            try
            {
                JsonArray pages = new JsonArray();
                using (MagickImageCollection collection = new MagickImageCollection())
                {
                    var settings = new MagickReadSettings
                    {
                        Density = new Density(300, 300)
                    };
                    collection.Read(pdf_path, settings);
                    int index = 0;
                    foreach (MagickImage image in collection)
                    {
                        //image.Format = MagickFormat.Jpg;
                        //string output = Path.Combine(output_path, $"page_{index}.jpg");
                        //Debug.WriteLine(output);
                        //image.Write(output);
                        //index++;

                        using (MemoryStream ms = new MemoryStream())
                        {
                            image.Format = MagickFormat.Jpg;
                            image.Write(ms);
                            ms.Seek(0, SeekOrigin.Begin);
                            BitmapImage bitmap = new BitmapImage();
                            bitmap.BeginInit();
                            bitmap.StreamSource = ms;
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.EndInit();
                            bitmap.Freeze();
                            WriteableBitmap writeableBitmap = new WriteableBitmap(bitmap);
                            int sizebuffer = writeableBitmap.PixelWidth * 4;
                            byte[] pixelData = new byte[sizebuffer * writeableBitmap.PixelHeight];
                            writeableBitmap.CopyPixels(pixelData, sizebuffer, 0);
                            int concolor = 0;
                            for (int i = 0; i < (sizebuffer * writeableBitmap.PixelHeight); i = i + 4)
                            {
                                byte blue = pixelData[i];
                                byte green = pixelData[i + 1];
                                byte red = pixelData[i + 2];
                                byte alpha = pixelData[i + 3];
                                if (blue != green || green != red || red != blue)
                                {
                                    concolor++;
                                    //Debug.WriteLine($"Pixel {i / 4} : R={red}, G={green}, B={blue}");
                                }
                            }
                            Debug.WriteLine($"Total de pixeles con color: {concolor}");
                            Debug.WriteLine($"Total de pixeles: {(sizebuffer / 4) * writeableBitmap.PixelHeight}");
                            Debug.WriteLine($"Porcentaje de pixeles con color: {((double)concolor / ((sizebuffer / 4) * writeableBitmap.PixelHeight)) * 100}%");
                            JsonObject page = new JsonObject();
                            page.Add("colorPixel", concolor);
                            page.Add("totalPixel", (sizebuffer / 4) * writeableBitmap.PixelHeight);
                            page.Add("percentage", ((double)concolor / ((sizebuffer / 4) * writeableBitmap.PixelHeight)) * 100);
                            page.Add("index", index);
                            pages.Add(page);
                            index++;
                        }
                    }

                }
                JsonObject result = new JsonObject();
                result.Add("success", true);
                result.Add("pages", pages);
                Log.Information($"Calculo de porcentaje de color exitoso para el archvio {pdf_path}");
                return result;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                Log.Error($"Error al calcular el porcentaje de color para el archivo {pdf_path}: {e.Message}");
                JsonObject obj = new JsonObject();
                obj.Add("success", false);
                obj.Add("message", e.Message);
                return obj;
            }
        }
    }
}
