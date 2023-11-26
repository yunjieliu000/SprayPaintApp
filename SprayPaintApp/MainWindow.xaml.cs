using Microsoft.Win32;
using System.Runtime.InteropServices.ObjectiveC;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace SprayPaintApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string originalImagePath;
        private string sprayPointsFilePath;
        private List<Point> sprayedPoints = new List<Point>();
        private bool isPainting = false;
        private bool isSpraying = false;
        private bool isErasing = false;
        private bool isMousePressed = false;
        private Point lastMousePosition;
        private int currDens = 10;
        private SolidColorBrush currBrush = Brushes.Red;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void OpenImage_Click(Object sender,  RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|* .jpg;*.jpeg;*.png;*|ALL Files|*.*";


            try
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    originalImagePath = openFileDialog.FileName;
                    sprayPointsFilePath = GetSprayPointsFilePath(originalImagePath);

                    BitmapImage bitmap = new BitmapImage(new Uri(originalImagePath));
                    DrawingCanvas.Background = new ImageBrush(bitmap);

                    LoadSprayPoints();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening image:{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Spray_Click(object sender, RoutedEventArgs e)
        {
            isSpraying = true;
            isErasing = false;

            MessageBox.Show("Spraying Activated. Move the mouse to start spraying.", "Spray Mode", MessageBoxButton.OK, MessageBoxImage.Information); ;

        }

        private void EraseButton_Click(object sender, RoutedEventArgs e)
        {
            isErasing = true;
            isSpraying = false;

            MessageBox.Show("Eraser Activated. Click on Spray to Erase.", "Eraser Mode", MessageBoxButton.OK, MessageBoxImage.Information); ;
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
//TODO: Add code to save file
                MessageBox.Show("Changes saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private string GetSprayPointsFilePath(string originalImagePath)
        {
            string directory = System.IO.Path.GetDirectoryName(originalImagePath);
            string filename = System.IO.Path.GetFileNameWithoutExtension(originalImagePath) + "_spray.txt";
            return System.IO.Path.Combine(directory, filename);
        }

        private void LoadSprayPoints()
        {
            if (File.Exists(sprayPointsFilePath))
            {
                try
                {
                    sprayedPoints.Clear();
                    using (StreamReader sr = new StreamReader(sprayPointsFilePath))
                    {
                        string line;
                        while((line = sr.ReadLine()) != null)
                        {
                            string[] coordinates = line.Split(",");
                            if (coordinates.Length == 2 && double.TryParse(coordinates[0], out double x) && double.TryParse(coordinates[1], out double y))
                            {
                                sprayedPoints.Add(new Point(x, y));
                            }
                        }
                    }

                }
                catch(Exception ex)
                {
                    MessageBox.Show($"Error loading spray points: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DrawingCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (isSpraying || isErasing)
            {
                isMousePressed = true;
                Point mousePos = e.GetPosition(DrawingCanvas);

                if (isSpraying)
                {
                    SprayPaint(mousePos);
                }
                else if (isErasing)
                {
                    EraseSpray(mousePos);
                }
            }

        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (isSpraying && isMousePressed)
                {
                    Point currPos = e.GetPosition(DrawingCanvas);
                    SprayPaint(currPos);
                    lastMousePosition = currPos;
                }
                else if (isErasing && isMousePressed)
                {
                    EraseSpray(e.GetPosition(DrawingCanvas));
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error wile spraying: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DrawingCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            isMousePressed = false;
        }

        private void SprayPaint(Point pos)
        {
            Random ran = new Random();
            int density = currDens;

            for (int i = 0; i < density; i++)
            {
                double offsetX = ran.Next(-10, 10);
                double offsetY = ran.Next(-10, 10);

                Point sprayPoint = new Point(pos.X + offsetX, pos.Y + offsetY);

                Ellipse ellipse = new Ellipse
                {
                    Width = 5,
                    Height = 5,
                    Fill = currBrush,
                    Margin = new Thickness(sprayPoint.X, sprayPoint.Y, 0, 0)
                };

                DrawingCanvas.Children.Add(ellipse);
                sprayedPoints.Add(sprayPoint);
            }
;
        }

        private void EraseSpray(Point pos)
        {
            double eraseRadius = 10.0;

            var pointsToErase = sprayedPoints.FindAll(point =>
            {
                double dist = Math.Sqrt(Math.Pow(point.X - pos.X, 2) + Math.Pow(point.Y - pos.Y, 2));
                return dist <= eraseRadius;
            });

            foreach (var point in pointsToErase)
            {
                Ellipse sprayPoint = FindEllipseAt(point);
                if (sprayPoint != null)
                {
                    DrawingCanvas.Children.Remove(sprayPoint) ;
                }
                sprayedPoints.Remove(point);
            }
        }

        private Ellipse FindEllipseAt(Point point)
        {
            foreach (UIElement element in DrawingCanvas.Children)
            {
                if (element is Ellipse ellipse)
                {
                    Point ellPosition = ellipse.TranslatePoint(new Point(0, 0), DrawingCanvas); 
                    double dist = Math.Sqrt(Math.Pow(ellPosition.X - point.X, 2) + Math.Pow(ellPosition.Y - point.Y, 2));

                    if (dist <= ellipse.Width)
                    {
                        return ellipse;
                    }

                }
            }
            return null;
        }


    }
}