using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using System.IO;

namespace MarkerReferenceImager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DirectoryInfo referenceDir;
        string markerID = "";
        string imagePath = "";

        string referenceImageDirPath = @":\Cemetery\ReferenceImages";

        public MainWindow()
        {
            InitializeComponent();

            //add correct drive letter to ref path
            referenceImageDirPath = System.AppDomain.CurrentDomain.BaseDirectory.Substring(0,1) + referenceImageDirPath;

            if (Directory.Exists(referenceImageDirPath))
            {
                referenceDir = new DirectoryInfo(referenceImageDirPath);
            }
        }

        private void MainImage_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                SetImageFile(files[0]);
            }
        }

        private void SetImageFile(string filePath)
        {
            try
            {
                if (!SetMarkerID(filePath.Substring(filePath.LastIndexOf('\\') + 1, 6))) throw new Exception("The dropped file does not have a valid marker ID code.");

                BitmapImage im = new BitmapImage();
                im.BeginInit();
                im.UriSource = new Uri(filePath);
                im.EndInit();
                MainImage.Source = im;
                imagePath = filePath;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                ResetInterface();
            }
        }

        private bool SetMarkerID(string id)
        {
            if (!Regex.IsMatch(id, @"[A-Z]{2}\d{4}")) return false;

            markerID = id;

            foreach (FileInfo file in referenceDir.EnumerateFiles())
            {
                if (file.Name.StartsWith(id))
                {
                    ShowConflictInterface(file);
                    return true;
                }
            }

            ShowNormalInterface();

            return true;
        }

        private void ShowNormalInterface()
        {
            Inquiry.Text = "Use this as the reference image for " + markerID + "?";
            Control.Visibility = Visibility.Visible;
            this.Height = 480;
            InquiryBack.Background = Brushes.SteelBlue;
            Inquiry.Foreground = Brushes.White;
        }

        private void ShowConflictInterface(FileInfo f)
        {
            Inquiry.Foreground = Brushes.Red;
            Inquiry.Text = "Reference image for " + markerID + " exists (shown below). Replace it?";
            InquiryBack.Background = Brushes.White;
            Control.Visibility = Visibility.Visible;

            BitmapImage bi = new BitmapImage();
            using (FileStream fs = new FileStream(f.FullName, FileMode.Open))
            {
                bi.BeginInit();
                bi.CacheOption = BitmapCacheOption.OnLoad;
                bi.StreamSource = fs;
                bi.EndInit();
            }

            ExistingImage.Source = bi;
            this.Height = 865;
        }

        private void ResetInterface()
        {
            markerID = "";
            imagePath = "";
            MainImage.Source = null;
            Control.Visibility = Visibility.Hidden;
            InquiryBack.Background = Brushes.SteelBlue;
            Inquiry.Foreground = Brushes.White;
            this.Height = 480;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            ResetInterface();
        }

        private void Execute_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                BitmapImage bi = new BitmapImage();
                bi.BeginInit();
                bi.UriSource = new Uri(imagePath);
                bi.DecodePixelWidth = 1000;
                bi.DecodePixelHeight = 750;
                bi.EndInit();

                PngBitmapEncoder enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bi));
                using (FileStream fs = new FileStream(referenceImageDirPath + @"\" + markerID + ".png", FileMode.Create))
                {
                    enc.Save(fs);
                }
                ResetInterface();
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.ToString());
            }

        }
    }
}
