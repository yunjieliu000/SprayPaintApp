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
using System.Windows.Markup;
using System.Globalization;

namespace SprayPaintApp
{

    public partial class MainWindow : Window
    {
        private string originalImagePath;
        private List<SprayPoint> sprayedPoints = new List<SprayPoint>();
        private bool isSpraying = false;
        private bool isErasing = false;
        private bool isMousePressed = false;
        private SprayPoint lastMousePosition;
        
        public MainWindow()
        {
            InitializeComponent();
        }

        // Event Handler for opening the image
        private void OpenImage_Click(Object sender,  RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image Files|* .jpg;*.jpeg;*.png;*|ALL Files|*.*";

            try
            {
                if (openFileDialog.ShowDialog() == true)
                {
                    originalImagePath = openFileDialog.FileName;

                    SprayHelper.ClearSprayedPoints(DrawingCanvas);
                    LoadImage(originalImagePath);
                    LoadSprayPoints(originalImagePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error opening image:{ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ErrorLogger.LogError($"Open Image Error: {ex.Message}");
            }
        }

        // Method to load the Image given file path

        // Event handler for the spray button
        private void Spray_Click(object sender, RoutedEventArgs e)
        {
            isSpraying = true;
            isErasing = false;

            MessageBox.Show("Spraying Activated. Move the mouse to start spraying.", "Spray Mode", MessageBoxButton.OK, MessageBoxImage.Information); ;

        }

        // Event handler for erase buton
        private void EraseButton_Click(object sender, RoutedEventArgs e)
        {
            isErasing = true;
            isSpraying = false;

            MessageBox.Show("Eraser Activated. Click to Erase.", "Eraser Mode", MessageBoxButton.OK, MessageBoxImage.Information); ;
        }

        // Event handler to save changes
        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                SaveImageWithSpray(originalImagePath);
                MessageBox.Show("Changes saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ErrorLogger.LogError($"Save Changes Error: {ex.Message}");
            }
        }


        // Event Handler for Canvas MouseDown Event
        private void DrawingCanvas_MouseDown(object sender, MouseEventArgs e)
        {
            if (isSpraying || isErasing)
            {
                isMousePressed = true;
                Point mousePos = e.GetPosition(DrawingCanvas);
                SprayPoint mouseSprayPos = new SprayPoint(mousePos.X, mousePos.Y, "");

                if (isSpraying)
                {
                    SprayPaint(mouseSprayPos);
                }
                else if (isErasing)
                {
                    EraseSpray(mouseSprayPos);
                }
            }

        }

        // Event handler for Canvas MouseMove event
        private void DrawingCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                Point currPos = e.GetPosition(DrawingCanvas);
                SprayPoint mouseCurrPos = new SprayPoint(currPos.X, currPos.Y, "");

                if (isSpraying && isMousePressed)
                {
                    SprayPaint(mouseCurrPos);
                    lastMousePosition = mouseCurrPos;
                }
                else if (isErasing && isMousePressed)
                {
                    EraseSpray(mouseCurrPos);
                    lastMousePosition = mouseCurrPos;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error wile spraying: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ErrorLogger.LogError($"Mouse Move Error: {ex.Message}");
            }
        }


        // Event handler for Canvas MouseUp event
        private void DrawingCanvas_MouseUp(object sender, MouseEventArgs e)
        {
            isMousePressed = false;
        }

        // Method to load the Image given file path
        private void LoadImage(string imagePath)
        {
            try
            {

                SprayHelper.ClearSprayedPoints(DrawingCanvas);
                BitmapImage bitmap = new BitmapImage(new Uri(imagePath));
                DrawingCanvas.Background = new ImageBrush(bitmap);
                sprayedPoints.Clear();
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError($"Load Image Error: {ex.Message}");
                throw new Exception($"Error loading image: {ex.Message}");
            }
        }

        // Method to spray points on canvas
        public void SprayPaint(SprayPoint pos, String state = "paint")
        {
            string selectedColor;
            if (state.Equals("repaint"))
            {
                selectedColor = pos.PointColor;
            }
            else
            {
                selectedColor = (Colors.SelectedItem as ComboBoxItem)?.Content.ToString();
            }

            Random ran = new Random();
            double density = Density.Value;

            for (int i = 0; i < density; i++)
            {
                double offsetX = ran.Next(-10, 10);
                double offsetY = ran.Next(-10, 10);

                SprayPoint sprayPoint = new SprayPoint(pos.X + offsetX, pos.Y + offsetY, selectedColor);
                Color currColor = (Color)ColorConverter.ConvertFromString(selectedColor);
                Brush currBrush = new SolidColorBrush(currColor);

                Ellipse ellipse = new Ellipse
                {
                    Width = density,
                    Height = density,
                    Fill = currBrush,
                    Margin = new Thickness(sprayPoint.X - 10, sprayPoint.Y - 10, 0, 0)
                };

                DrawingCanvas.Children.Add(ellipse);
                sprayedPoints.Add(sprayPoint);
            }
;
        }

        // Method to erase sprayed points on Canvas
        public void EraseSpray(SprayPoint pos)
        {
            double eraseRadius = Density.Value;

            var pointsToErase = sprayedPoints.FindAll(point =>
            {
                double dist = Math.Sqrt(Math.Pow(point.X - pos.X, 2) + Math.Pow(point.Y - pos.Y, 2));
                return dist <= eraseRadius;
            });

            foreach (var Spraypoint in pointsToErase)
            {
                Ellipse sprayPoint = SprayHelper.FindEllipseAt(Spraypoint, DrawingCanvas);
                if (sprayPoint != null)
                {
                    DrawingCanvas.Children.Remove(sprayPoint);
                }
                sprayedPoints.Remove(Spraypoint);
            }
        }


        // Method to Save sprayed points
        private void SaveImageWithSpray(string imagePath)
        {
            try
            {
                saveSprayPoints(SprayHelper.GetSprayPointsFilePath(imagePath));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving changes: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ErrorLogger.LogError($"Saving Changes Error: {ex.Message}");
            }
        }

        // Method to write all the sprayed point as a file
        private void saveSprayPoints(string filePath)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(filePath))
                {
                    foreach (SprayPoint point in sprayedPoints)
                    {
                        writer.WriteLine($"{point.X},{point.Y}, {point.PointColor}");
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError($"Saving Spray Points Error: {ex.Message}");
                throw new Exception($"Error saving spray points to file: {ex.Message}");
            }
        }

        // Method to load previously saved spray points from file
        private void LoadSprayPoints(string originalImagePath)
        {
                    try
                    {
                        string sprayPointsFilePath = SprayHelper.GetSprayPointsFilePath(originalImagePath);
                        if (File.Exists(sprayPointsFilePath))
                        {
                            List<SprayPoint> loadedSprayPoints = SprayHelper.LoadSprayPointsFromFile(sprayPointsFilePath);
                            SprayHelper.ClearSprayedPoints(DrawingCanvas);
                            foreach (SprayPoint Spraypoint in loadedSprayPoints)
                            {
                                SprayPaint(Spraypoint, "repaint");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error loading spray points: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        ErrorLogger.LogError($"Load Spray Points Error: {ex.Message}");
                    }
                
        }

    }
}