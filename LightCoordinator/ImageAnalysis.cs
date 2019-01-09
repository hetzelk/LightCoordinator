using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using Newtonsoft.Json.Linq;
using System.Windows.Forms;
using LightCoordinator.Model;

namespace LightCoordinator
{
    public static class ImageAnalysis
    {
        public static Bitmap createBitMap(string filepath)
        {
            Bitmap bMap = Bitmap.FromFile(filepath) as Bitmap;

            if (bMap == null) throw new FileNotFoundException("Cannot open picture file for analysis");

            return bMap;
        }

        public static Bitmap CaptureBitmap(int screenNumber)
        {
            //int the future figure out how to easily change screens
            List<Screen> screens = Screen.AllScreens.ToList();
            Screen screen = screens.Where(x => x.Bounds.X == 0).First();

            Point point = new Point(screen.WorkingArea.X, screen.WorkingArea.Y);
            Rectangle bounds = Screen.GetBounds(point);
            Bitmap bitmap = new Bitmap(bounds.Width, bounds.Height);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                try
                {
                    g.CopyFromScreen(new Point(bounds.Left, bounds.Top), Point.Empty, bounds.Size);
                }
                catch (Exception e)
                {
                    // if there is an error, just send an empty bitmap to send a blank scene
                    bitmap = null;
                }
            }
            //bitmap.Save("current.jpg", ImageFormat.Jpeg);

            return bitmap;
        }

        public static Tuple<Dictionary<int, int>, int> GetAllColors(Bitmap image)
        {
            Dictionary<int, int> allPixels = new Dictionary<int, int>();

            int pixelJump = 10;
            decimal totalPixels = 0;

            for (int row = 0; row < image.Size.Width;)
            {
                for (int col = 0; col < image.Size.Height;)
                {
                    int pixelColor = image.GetPixel(row, col).ToArgb();

                    if (allPixels.Keys.Contains(pixelColor))
                    {
                        allPixels[pixelColor]++;
                    }
                    else
                    {
                        allPixels.Add(pixelColor, 1);
                    }
                    totalPixels += 1;
                    col += pixelJump;
                }
                row += pixelJump;
            }

            return Tuple.Create<Dictionary<int, int>, int>(allPixels.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value), Convert.ToInt32(totalPixels));
        }

        public static List<Color> GetChunkColors(Bitmap image, int chunkSize, int pixelJump, int x, int y, int width, int height)
        {
            List<Color> pixels = new List<Color>();
            int currentRow = x;
            int currentColumn = y;
            int rowCount = 0;
            int columnCount = 0;

            while (columnCount < chunkSize)
            {
                if (currentColumn < height)
                {
                    while (rowCount < chunkSize)
                    {
                        if (currentRow < width)
                        {
                            try
                            {
                                int pixelColor = image.GetPixel(currentRow, currentColumn).ToArgb();
                                /*new LCColor(Color.FromArgb(pixelColor))*/;
                                pixels.Add(Color.FromArgb(pixelColor));
                            }
                            catch (Exception e)
                            {
                                Console.Write("");
                            }

                            rowCount += pixelJump;
                            currentRow += pixelJump;
                        }
                        else
                        {
                            break;
                        }
                    }
                    rowCount = 0;
                    columnCount += pixelJump;
                    currentColumn += pixelJump;
                    currentRow = x;
                }
                else
                {
                    break;
                }
            }

            return pixels;
        }

        public static List<Color> GetAverageColors(Bitmap image)
        {
            List<Color> averageColors = new List<Color>();
            List<List<Color>> chunks = new List<List<Color>>();

            int width = image.Size.Width;
            int height = image.Size.Height;

            int chunkSize = 100;

            int currentRow = 0;
            int currentColumn = 0;

            List<int> currentRowPixels = new List<int>();
            while (currentColumn < height)
            {
                currentRow = 0;
                while (currentRow < width)
                {
                    chunks.Add(GetChunkColors(image, chunkSize, 5, currentRow, currentColumn, width, height));
                    currentRow += chunkSize;
                }
                currentColumn += chunkSize;
            }

            foreach (List<Color> chunk in chunks)
            {
                double r = 0;
                double g = 0;
                double b = 0;

                foreach (Color color in chunk)
                {
                    r += color.R;
                    g += color.G;
                    b += color.B;
                }

                r = Math.Round(r / chunk.Count);
                g = Math.Round(g / chunk.Count);
                b = Math.Round(b / chunk.Count);

                Color averageColor = Color.FromArgb(Convert.ToInt32(r), Convert.ToInt32(g), Convert.ToInt32(b));
                averageColors.Add(averageColor);
            }

            return averageColors;
        }

        public static List<Color> ReduceColors(List<Color> colors, bool allowDuplicates, int total)
        {
            List<Color> finalColors = new List<Color>();
            Random random = new Random();
            int attempts = 0;

            while (finalColors.Count < total && attempts < 5)
            {
                int i = random.Next(0, colors.Count);

                if (allowDuplicates)
                {
                    finalColors.Add(colors[i]);
                }
                else if (!finalColors.Contains(colors[i]))
                {
                    finalColors.Add(colors[i]);
                }
                else
                {
                    attempts += 1;
                }
            }

            return finalColors;
        }

        public static List<Color> GetColorsFromImage(Bitmap image, int count)
        {
            var colorTuple = GetAllColors(image);

            Dictionary<int, int> pixelsHighToLow = colorTuple.Item1;
            int totalPixels = colorTuple.Item2;

            List<Color> mostUsedColors = new List<Color>();

            decimal numberOfGroups = 4;
            int counter = 0;
            int groupSize = Convert.ToInt32(Math.Ceiling(pixelsHighToLow.Count / numberOfGroups));

            var result = pixelsHighToLow.GroupBy(x => counter++ / groupSize);

            int timesToAdd = 0;
            List<int> take = new List<int>(new int[] { 25, 15, 5, 5, 0, 0 });
            bool addBlack = true;

            foreach (var dict in result)
            {
                var tempdict = dict.Take(take[0]);

                foreach (KeyValuePair<int, int> kvp in tempdict)
                {
                    decimal pixelCount = kvp.Value;

                    if (mostUsedColors.Count < count)
                    {
                        int percent = Convert.ToInt32((pixelCount / totalPixels) * 100);

                        timesToAdd = Math.Max((percent * 20) / 100, 1);

                        Color color = Color.FromArgb(kvp.Key);
                        if ((color.R == 0 && color.G == 0 && color.B == 0 && addBlack) || percent > 50)
                        {
                            mostUsedColors.AddToList(color, timesToAdd);
                            addBlack = false;
                        }
                        else
                        {
                            mostUsedColors.AddToList(color, timesToAdd);
                        }
                    }
                }

                take.Remove(take[0]);
                if (mostUsedColors.Count > 50)
                {
                    break;
                }
            }

            if (mostUsedColors.Count == 0)
            {
                mostUsedColors.Add(Color.Black);
            }

            return mostUsedColors;
        }

        public static void AddToList(this List<Color> colors, Color color, int timesToAdd)
        {
            while (timesToAdd > 0)
            {
                colors.Add(color);
                timesToAdd -= 1;
            }
        }
    }
}