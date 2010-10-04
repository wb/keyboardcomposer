using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Adaptive.Interfaces;

using LWContextCommunication;
using System.Text;
using System.IO;

namespace MoodKeyboardContext
{
    [AdaptiveTouchRegion()]
    public partial class MainPage : UserControl, IAdaptiveContentProvider
    {
        private IAdaptiveContextMessaging context = null;
        private bool touching = false;
        private System.Windows.Media.Imaging.WriteableBitmap bmSrc = null;
        private System.Windows.Media.Imaging.WriteableBitmap bmDst = null;
        private BitmapImage bmi = null;
        private MultiScaleImage msi = null;
        private Point oldTouchPoint;

        private static int touchpadWidth = 1366;
        private static int touchpadHeight = 216;
        private double baseHeight = 0;
        private double yOffset = 0;
        private TranslateTransform tt;
        private int framesSinceUpdate = 0;

        public MainPage()
        {
            InitializeComponent();
            
            this.MouseLeftButtonDown += new MouseButtonEventHandler(MainPage_MouseLeftButtonDown);
            this.MouseMove += new MouseEventHandler(MainPage_MouseMove);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(MainPage_MouseLeftButtonUp);

            bmi = new BitmapImage(new Uri("Images/blankScore.png", UriKind.Relative));
            bmi.ImageOpened += new EventHandler<RoutedEventArgs>(bmi_ImageOpened);

            this.spectrum.Source = bmi;
        }

        public void bmi_ImageOpened(Object sender, RoutedEventArgs e)
        {
            tt = new TranslateTransform();
            bmi.ImageOpened -= new EventHandler<RoutedEventArgs>(bi_ImageOpened);
            baseHeight = -bmi.PixelHeight * touchpadWidth / bmi.PixelWidth + touchpadHeight;
            tt.Y = baseHeight;
            this.spectrum.RenderTransform = tt;
            this.InvalidateArrange();
        }

        void MainPage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            touching = false;
            Point p = e.GetPosition(null);
            int yDiff = (int)(oldTouchPoint.Y - p.Y);

            tt.Y = tt.Y - yDiff;
            tt.Y = Math.Min(tt.Y, 0);
            tt.Y = Math.Max(tt.Y, baseHeight);

            yOffset = tt.Y - baseHeight;

            this.spectrum.RenderTransform = tt;
        }

        void MainPage_MouseMove(object sender, MouseEventArgs e)
        {
            if (touching)
            {
                Point p = e.GetPosition(null);
                double yDiff = oldTouchPoint.Y - p.Y;

                if (framesSinceUpdate > 3)
                {
                    framesSinceUpdate = 0;
                    oldTouchPoint = p;

                    tt.Y = tt.Y - yDiff;
                    tt.Y = Math.Min(tt.Y, 0);
                    tt.Y = Math.Max(tt.Y, baseHeight);

                    yOffset = tt.Y - baseHeight;

                    this.spectrum.RenderTransform = tt;
                }
                else
                {
                    framesSinceUpdate++;
                }
            }
        }

        void MainPage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            touching = true;
            framesSinceUpdate = 0;
            oldTouchPoint = e.GetPosition(null);
        }


        public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (this.context != null)
            {
                if (e.messageID == (int)LWMessageID.CHANGE_PICTURE)
                {
                    Encoding enc = Encoding.UTF8;
                    String s = enc.GetString(e.messageData, 0, e.messageData.Length);
                    bmi = new BitmapImage(new Uri(s, UriKind.RelativeOrAbsolute));
                    bmi.ImageOpened += new EventHandler<RoutedEventArgs>(bi_ImageOpened);
                    this.spectrum.Source = bmi;
                }
            }
        }

        void bi_ImageOpened(object sender, RoutedEventArgs e)
        {
            bmi.ImageOpened -= new EventHandler<RoutedEventArgs>(bi_ImageOpened);
            double yDiff = tt.Y - baseHeight;
            baseHeight = -bmi.PixelHeight * touchpadWidth / bmi.PixelWidth + touchpadHeight;
            tt.Y = baseHeight + yDiff;
            tt.Y = Math.Min(tt.Y, 0);
            tt.Y = Math.Max(tt.Y, baseHeight);
            this.spectrum.RenderTransform = tt;
            this.InvalidateArrange();
        }


        public void SetMessagingObject(IAdaptiveContextMessaging context)
        {
            if (context != null)
            {
                this.context = context;
                this.context.MessageReceived += new EventHandler<MessageReceivedEventArgs>(OnMessageReceived);
            }
        }
    }
}
