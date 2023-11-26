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
    }
}