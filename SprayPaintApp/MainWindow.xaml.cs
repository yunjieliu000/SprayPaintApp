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
            isPainting = true;
            lastMousePosition = e.GetPosition(DrawingCanvas);
        }

        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (isPainting)
                {
                    Point currPos = e.GetPosition(DrawingCanvas);
                    SprayPaint(currPos);
                    lastMousePosition = currPos;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error wile painting: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DrawingCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            isPainting = false;
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

        private void DrawingCanvas_MouseDown_1(object sender, MouseButtonEventArgs e)
        {

        }

        private void DrawingCanvas_MouseUp_1(object sender, MouseButtonEventArgs e)
        {

        }
    }
}