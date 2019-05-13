using ContentAwareResize.Algorithms;
using ContentAwareResize.Core;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
using System.Windows.Threading;

namespace ContentAwareResize
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MyColor[,] ImageMatrix;
        //MyColor[,] CloneImage;
        private readonly BackgroundWorker BW = new BackgroundWorker();
        DispatcherTimer dispatcherTimer = new DispatcherTimer();
        Stopwatch stopWatch = new Stopwatch();
        string currentTime = string.Empty;
        public MainWindow()
        {
            InitializeComponent();
            TypesComboBox.Items.Add("Normal");
            TypesComboBox.Items.Add("Seam Carving");
            TypesComboBox.Items.Add("Forward Energy");

            dispatcherTimer.Tick += new EventHandler(dt_Tick);
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 1);
        }

        void dt_Tick(object sender, EventArgs e)
        {
            if (stopWatch.IsRunning)
            {
                TimeSpan ts = stopWatch.Elapsed;
                currentTime = String.Format("{0:00}:{1:00}:{2:00}",
                ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                TimeTextBox.Content = "Time: " + currentTime;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            if (openFileDialog1.ShowDialog() == true)
            {
                string OpenedFilePath = openFileDialog1.FileName;
                FilePath.Text = OpenedFilePath;
                ImageMatrix = ImageOperations.OpenImage(OpenedFilePath);
                ImageOperations.DisplayImage(ImageMatrix, ImageBox);
                WidthTextBox.Text = ImageOperations.GetWidth(ImageMatrix).ToString();
                HeightTextBox.Text = ImageOperations.GetHeight(ImageMatrix).ToString();
                int x = int.Parse(WidthTextBox.Text), y = int.Parse(HeightTextBox.Text);

                SeamCarving.leaveIt = new bool[y, x];
                for (int i = 0; i < y; i++)
                {
                    for (int j = 0; j < x; j++)
                    {
                        SeamCarving.leaveIt[i, j] = false;
                    }
                }

            }

        }
        int newHeight = 0, newWidth = 0;
        MyColor[,] resizedImage;
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            using (var fbd = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult result = fbd.ShowDialog();

                if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                {
                    string FilePath = FileNameTextBox.Text;
                    //MessageBox.Show(FilePath + " " + "test.jpg");
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)ImageBox.Source));
                    using (FileStream stream = new FileStream(FilePath, FileMode.Create))
                        encoder.Save(stream);
                }
            }
        }
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            newWidth = int.Parse(WidthTextBox.Text);
            newHeight = int.Parse(HeightTextBox.Text);

            if (TypesComboBox.SelectedIndex == 0)
            {
                resizedImage = ImageOperations.NormalResize(ImageMatrix, newHeight, newWidth);
                ImageOperations.DisplayImage(resizedImage, ImageBox);
            }
            else if (TypesComboBox.SelectedIndex == 1)
            {
                BW.DoWork += BW_DoWork;
                ProgressBar.Minimum = 1;
                ProgressBar.Maximum = 100;
                BW.ProgressChanged += BW_ProgressChanged;
                BW.WorkerReportsProgress = true;
                BW.RunWorkerCompleted += BW_RunWorkerCompleted;
                BW.RunWorkerAsync();
            }
        }



        private void BW_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ProgressBar.Value = e.ProgressPercentage;
            ImageOperations.DisplayImage(SeamCarving.TempMatrix, ImageBox);
        }
        private void BW_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ImageOperations.DisplayImage(resizedImage, ImageBox);
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SeamCarving.Visualisation = true;
        }
        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            SeamCarving.Visualisation = false;
        }

        BitmapSource LoadImage(Byte[] imageData)
        {
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                var decoder = BitmapDecoder.Create(ms,
                    BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                return decoder.Frames[0];
            }
        }
        private bool valid(int x, int y)
        {
            return (x < int.Parse(WidthTextBox.Text) && x >= 0 && y < int.Parse(HeightTextBox.Text) && y >= 0);
        }


        private Thread mouseThread = null;
        private void getPixels(object sender, MouseButtonEventArgs e)
        {
            List<Tuple<int, int>> ways = new List<Tuple<int, int>>();
            ways.Add(new Tuple<int, int>(1, 0));
            ways.Add(new Tuple<int, int>(0, 1));
            ways.Add(new Tuple<int, int>(1, 1));
            ways.Add(new Tuple<int, int>(-1, 0));
            ways.Add(new Tuple<int, int>(-1, -1));
            ways.Add(new Tuple<int, int>(-1, 1));
            ways.Add(new Tuple<int, int>(0, 1));
            ways.Add(new Tuple<int, int>(0, -1));



            while (true)
            {
                this.Dispatcher.Invoke(() =>
               {

                   System.Windows.Point myPoint = e.GetPosition((System.Windows.IInputElement)sender);
                   int x = (int)myPoint.X, y = (int)myPoint.Y;
                   System.Diagnostics.Debug.WriteLine(x + " " + y);


                   for (int i = y-5; i <= y + 10; i++)
                   {
                       for (int j = x-5; j <= x + 10; j++)
                       {
                           if (valid(j , i ))
                           {
                               SeamCarving.leaveIt[i,j] = true;
                               //ImageMatrix[i, j].red = 0;
                               //ImageMatrix[i, j].blue = 255;
                               //ImageMatrix[i, j].green = 0;
                           }
                       }
                   }
               });
            }
        }

        private void mouseDown(object sender, MouseButtonEventArgs e)
        {
            mouseThread = new Thread(new ThreadStart(delegate () { getPixels(sender, e); }));
            ImageOperations.DisplayImage(ImageMatrix, ImageBox);
            mouseThread.Start();
        }

        private void mouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mouseThread != null && mouseThread.IsAlive)
                mouseThread.Abort();
        }

        private void BW_DoWork(object sender, DoWorkEventArgs e)
        {
            SeamCarving.TempMatrix = ImageMatrix;
            BackgroundWorker worker = sender as BackgroundWorker;
            stopWatch.Start();
            dispatcherTimer.Start();
            resizedImage = SeamCarving.Resize(ImageMatrix, newHeight, newWidth, worker);
            ImageMatrix = resizedImage;
            stopWatch.Stop();
        }
    }
}
