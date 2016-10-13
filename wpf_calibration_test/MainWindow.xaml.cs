using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpf_calibration_test {
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DataFromLaser = new Dictionary<int, double[]>();
            DataFromRobot = new Dictionary<int, double>();
            AngleOFTriangle = 90;
            LengthOfTriangle = 250;
            GenerateDataForLaser();
            GenerateDataFromRobot();

            text = new TextBlock();
            lb_AngleOFTriangle.Content = $"AngleOFTriangle = {slider_AngleOFTriangle.Value}";


            y1 = 400;
            y2 = 400;
        }
        public Dictionary<int, double[]> DataFromLaser;
        public Dictionary<int, double> DataFromRobot;
        public double AngleOFTriangle; // угол треугольника плиты
        public double LengthOfTriangle; // длинна треугольника

        TextBlock text;
        double centre;
        Thread thrd;

        double y1, y2;

        //тестовые значения для лазера
        public void GenerateDataForLaser() {
            DataFromLaser.Add(1, new double[] { 0 }); // 1 - номер измерения, 0 - положение канавки относительно центра 
            DataFromLaser.Add(2, new double[] { 200, 100 }); // 2 - номер измерения, (20, 10) растояние по модулю от центра лазера до левой и правой границы
            DataFromLaser.Add(3, new double[] { 150, 75 });
            DataFromLaser.Add(4, new double[] { 100, 50 });
        }
        //тестовые значения для робота
        public void GenerateDataFromRobot() {
            DataFromRobot.Add(1, 0); // 1 - номер измерения, 0 - координата робота по X, начальная 0
            DataFromRobot.Add(2, 10); // 2 - номер измерения, 10 растояние по модулю от начальной точки до текущей
            DataFromRobot.Add(3, 20);
            DataFromRobot.Add(4, 30);
        }

        public void DrawStartElements() {
            cnv.Children.Clear();
            centre = cnv.ActualWidth / 2;
            Line CentreLine = new Line() {
                X1 = centre,
                X2 = centre,
                Y1 = 0,
                Y2 = cnv.ActualHeight,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection() { 10, 5, 5, 5 }  //new double[] { 2, 1, 1, 2 };
            };
            cnv.Children.Add(CentreLine);

            //double len = LengthOfTriangle / Math.Sin(GradToRad(AngleOFTriangle) / 2);
            double len = LengthOfTriangle * Math.Tan(GradToRad(AngleOFTriangle) / 2);
            Line TriangleLeftLine = new Line() {
                X1 = centre,
                X2 = centre - len,
                Y1 = cnv.ActualHeight,
                Y2 = cnv.ActualHeight - LengthOfTriangle,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 4,
            };
            cnv.Children.Add(TriangleLeftLine);

            Line TriangleRightLine = new Line() {
                X1 = centre,
                X2 = centre + len,
                Y1 = cnv.ActualHeight,
                Y2 = cnv.ActualHeight - LengthOfTriangle,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 4,
            };
            cnv.Children.Add(TriangleRightLine);

            Line DataLine1 = new Line() {
                X1 = centre - DataFromLaser[2][0],
                X2 = centre + DataFromLaser[2][1],
                //Y1 = cnv.ActualHeight - (DataFromRobot[2] + 500),
                //Y2 = cnv.ActualHeight - (DataFromRobot[2] + 500),
                Y1 = cnv.ActualHeight - y1,
                Y2 = cnv.ActualHeight - y2,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 4,
            };
            cnv.Children.Add(DataLine1);
            var g1 = RenderedIntersect(cnv, DataLine1, TriangleLeftLine);
            if (g1.Bounds.IsEmpty) {  //TODO: почему то всегда пустой
                        y1 -= 10;
                        y2 -= 10;
                    Console.WriteLine($"y1 = {y1}");
            }
            else {
                Console.WriteLine($"Bounds  = {g1.Bounds.Y}");
            }                     
            //var g1 = RenderedIntersect(cnv, sa[0], TriangleRightLine);
            //var g2 = RenderedIntersect(cnv, sa[0], TriangleLeftLine);

        }
        public void DrawData() {

        }
        public void Draw() {
            DrawStartElements();
        }

        private void bt_draw_Click(object sender, RoutedEventArgs e) {
            Draw();
        }
       

        public double GradToRad(double grad) {
            return grad * Math.PI / 180;
        }
        private void cnv_SizeChanged(object sender, SizeChangedEventArgs e) {
            Draw();
        }
        private void cnv_MouseMove(object sender, MouseEventArgs e) {
            cnv.Children.Remove(text);
            Point position = e.GetPosition(sender as Canvas);            
            text.Text = $"X = {(cnv.ActualWidth / 2 - position.X) * 2}; Y = {cnv.ActualHeight - position.Y}"; //TODO: pos X
            text.Name = "TextBox1";
            Canvas.SetLeft(text, position.X + 20);
            Canvas.SetTop(text, position.Y);
            cnv.Children.Add(text);
        }
        private void slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (slider_AngleOFTriangle.IsLoaded) {
                lb_AngleOFTriangle.Content = $"AngleOFTriangle = {slider_AngleOFTriangle.Value}";
                AngleOFTriangle = slider_AngleOFTriangle.Value;
                Draw();
            }
        }
        private void cnv_MouseLeave(object sender, MouseEventArgs e) {
            cnv.Children.Remove(text);
        }
        private void slider_LengthOfTriangle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e) {
            if (slider_LengthOfTriangle.IsLoaded) {
                lb_AngleOFTriangle.Content = $"AngleOFTriangle = {slider_LengthOfTriangle.Value}";
                LengthOfTriangle = slider_LengthOfTriangle.Value;
                Draw();
            }
        }

        static CombinedGeometry RenderedIntersect(Visual ctx, Shape s1, Shape s2) {
            var p = new Pen(Brushes.Transparent, 0.01);
            var t1 = s1.TransformToAncestor(ctx) as Transform;
            var t2 = s2.TransformToAncestor(ctx) as Transform;
            var g1 = s1.RenderedGeometry;
            var g2 = s2.RenderedGeometry;
            if (s1 is Line)
                g1 = g1.GetWidenedPathGeometry(p);
            if (s2 is Line)
                g2 = g2.GetWidenedPathGeometry(p);
            g1.Transform = t1;
            g2.Transform = t2;
            return new CombinedGeometry(GeometryCombineMode.Intersect, g1, g2);
        }
    }
}
