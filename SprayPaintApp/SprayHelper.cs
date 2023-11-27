using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows;
using System.IO;

namespace SprayPaintApp
{
    public class SprayHelper
    {

        // Find the ellipse given a spray point and the canvas
        public static Ellipse FindEllipseAt(SprayPoint Spraypoint, Canvas DrawingCanvas)
        {
            foreach (UIElement element in DrawingCanvas.Children)
            {
                if (element is Ellipse ellipse)
                {
                    Point ellPosition = ellipse.TranslatePoint(new Point(0, 0), DrawingCanvas);
                    double dist = Math.Sqrt(Math.Pow(ellPosition.X - Spraypoint.X, 2) + Math.Pow(ellPosition.Y - Spraypoint.Y, 2));

                    if (dist <= ellipse.Width)
                    {
                        return ellipse;
                    }

                }
            }
            return null;
        }

        // Return the file path for the sprayed points file
        public static string GetSprayPointsFilePath(string originalImagePath)
        {
            string directory = System.IO.Path.GetDirectoryName(originalImagePath);
            string filename = System.IO.Path.GetFileNameWithoutExtension(originalImagePath) + "_spray.txt";
            return System.IO.Path.Combine(directory, filename);
        }


        // Return a list of Spray Points from file
        public static List<SprayPoint> LoadSprayPointsFromFile(string filePath)
        {
            List<SprayPoint> loadedSprayPoints = new List<SprayPoint>();

            try
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    string[] coordinates = line.Split(',');
                    if (coordinates.Length == 3 && double.TryParse(coordinates[0], out double x) && double.TryParse(coordinates[1], out double y))
                    {
                        String color = coordinates[2];
                        loadedSprayPoints.Add(new SprayPoint(x, y, color));
                    }
                }
            }

            catch (Exception ex)
            {
                ErrorLogger.LogError($"Load Spray Points Error: {ex.Message}");
                throw new Exception($"Error loading spray points from file: {ex.Message}");
            }

            return loadedSprayPoints;
        }

        // Clear all sprayed points from the canvas
        public static void ClearSprayedPoints(Canvas DrawingCanvas)
        {
            DrawingCanvas.Children.Clear();
        }
    }
}
