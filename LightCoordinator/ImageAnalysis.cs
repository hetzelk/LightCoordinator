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
using LightCoordinator.Extensions;

namespace LightCoordinator
{
    public static class ImageAnalysis
    {
        public static Tuple<int, int> DivideScreen(int total)
        {
            int rows = 1;
            int columns = 1;
            bool satisfied = false;

            bool toggle = true;
            while (!satisfied)
            {
                if (rows * columns >= total)
                {
                    satisfied = true;
                    break;
                }
                else
                {
                    if (toggle)
                    {
                        rows += 1;
                        toggle = false;
                    }
                    else
                    {
                        columns += 1;
                        toggle = true;
                    }
                }
            }

            return new Tuple<int, int>(rows, columns);
        }

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
            //Screen screen = screens.Where(x => x.Bounds.X == 0).First();
            Screen screen = screens[screenNumber];

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

        public static Tuple<Dictionary<int, int>, int> GetAllColors(Bitmap image, int pixelJump=5)
        {
            Dictionary<int, int> allPixels = new Dictionary<int, int>();
            
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

        //TODO: if the screen has not changed and the colors don't change, don't send an update
        //TODO: When creating a palette from the screen, add a color.2 that can be used, when colors are being mirrored the second color can be used to change it up
        public static List<LCColor> GetChunkColors(Bitmap image, int rowSize, int columnSize, int x, int y, int width, int height, int pixelJump=10)
        {
            List<LCColor> pixels = new List<LCColor>();
            int currentRow = x;
            int currentColumn = y;
            int rowCount = 0;
            int columnCount = 0;

            while (columnCount < columnSize)
            {
                if (currentColumn < height)
                {
                    while (rowCount < rowSize)
                    {
                        if (currentRow < width)
                        {
                            try
                            {
                                int pixelColor = image.GetPixel(currentRow, currentColumn).ToArgb();
                                /*new LCColor(Color.FromArgb(pixelColor))*/;
                                pixels.Add(new LCColor(Color.FromArgb(pixelColor)));
                            }
                            catch (Exception e)
                            {
                                Log.Write(e.ToString());
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

        public static List<LCColor> GetAverageColors(Bitmap image, int rows, int columns, int borderReduction = 0)
        {
            List<LCColor> averageColors = new List<LCColor>();
            List<List<LCColor>> chunks = new List<List<LCColor>>();

            //this will reduce the border on every side by X pixels
            int width = image.Size.Width - borderReduction;
            int height = image.Size.Height - borderReduction;

            int rowSize = width / rows;
            int columnSize = height / columns;

            int currentRow = borderReduction;
            int currentColumn = borderReduction;

            List<int> currentRowPixels = new List<int>();
            while (currentColumn < height)
            {
                currentRow = borderReduction;
                while (currentRow < width)
                {
                    chunks.Add(GetChunkColors(image, rowSize, columnSize, currentRow, currentColumn, width, height, pixelJump: 5));
                    currentRow += rowSize;
                }
                currentColumn += columnSize;
            }

            foreach (List<LCColor> chunk in chunks)
            {
                double r = 0;
                double g = 0;
                double b = 0;

                foreach (LCColor color in chunk)
                {
                    r += color.r;
                    g += color.g;
                    b += color.b;
                }

                r = Math.Round(r / chunk.Count);
                g = Math.Round(g / chunk.Count);
                b = Math.Round(b / chunk.Count);

                LCColor averageColor = new LCColor(Color.FromArgb(Convert.ToInt32(r), Convert.ToInt32(g), Convert.ToInt32(b)));
                averageColors.Add(averageColor);
            }

            return averageColors;
        }

        public static List<LCColor> ReduceColors(List<LCColor> colors, bool allowDuplicates, int total)
        {
            List<LCColor> finalColors = new List<LCColor>();
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

        public static List<LCColor> GetColorsFromImage(Bitmap image, int count)
        {
            var colorTuple = GetAllColors(image);

            Dictionary<int, int> pixelsHighToLow = colorTuple.Item1;
            int totalPixels = colorTuple.Item2;

            List<LCColor> mostUsedColors = new List<LCColor>();

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

                        LCColor color = new LCColor(Color.FromArgb(kvp.Key));
                        if ((color.r == 0 && color.g == 0 && color.b == 0 && addBlack) || percent > 50)
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
                mostUsedColors.Add(new LCColor(Color.Black));
            }

            return mostUsedColors;
        }

        public static void AddToList(this List<LCColor> colors, LCColor color, int timesToAdd)
        {
            while (timesToAdd > 0)
            {
                colors.Add(color);
                timesToAdd -= 1;
            }
        }
    }
}