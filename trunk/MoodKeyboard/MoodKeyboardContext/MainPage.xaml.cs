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

using LWEvent;
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
        private int xOffset = 0;
        private Point oldTouchPoint;

        private static int touchpadWidth = 1366;
        private static int touchpadHeight = 216;

        public MainPage()
        {
            InitializeComponent();
            
            this.touchIndicator.Visibility = Visibility.Collapsed;
            this.MouseLeftButtonDown += new MouseButtonEventHandler(MainPage_MouseLeftButtonDown);
            this.MouseMove += new MouseEventHandler(MainPage_MouseMove);
            this.MouseLeftButtonUp += new MouseButtonEventHandler(MainPage_MouseLeftButtonUp);

            // spectrum.Height, (int)spectrum.Width
            bmi = new BitmapImage(new Uri("Images/samplemusic.png", UriKind.Relative));
            bmi.ImageOpened += new EventHandler<RoutedEventArgs>(bmi_ImageOpened);

            this.spectrum.Source = bmi;
        }

        void bmi_ImageOpened(object sender, RoutedEventArgs e)
        {
            bmSrc = new WriteableBitmap(bmi);

            bmDst = new WriteableBitmap(touchpadWidth, touchpadHeight);

            ResetDst(0);

            this.spectrum.Source = bmDst;
        }

        void ResetDst(int xDiff)
        {
            xOffset = xOffset + xDiff;
            if (xOffset < 0) xOffset = 0;
            if (xOffset > bmSrc.PixelWidth - bmDst.PixelWidth - 1) xOffset = bmSrc.PixelWidth - bmDst.PixelWidth - 1;

            int[] pixelsDst = bmDst.Pixels;
            int[] pixelsSrc = bmSrc.Pixels;

            //MessageBox.Show(String.Format("dst len {0:N} src len {1:N}", pixelsDst.Length, pixelsSrc.Length));

            for (int i = 0; i < touchpadHeight; i++)
            {
                for (int j = xOffset, jj = 0; jj < touchpadWidth; j++, jj ++)
                {
                    if (i * bmDst.PixelWidth + jj > pixelsDst.Length)
                        MessageBox.Show(String.Format("{0:N} and {1:N} too big for dst", i, jj));
                    if (i * bmSrc.PixelWidth + j > pixelsSrc.Length)
                        MessageBox.Show(String.Format("{0:N} and {1:N} too big for src", i, j));
                    pixelsDst[i * bmDst.PixelWidth + jj] = pixelsSrc[i * bmSrc.PixelWidth + j];
                }
            }

            bmDst.Invalidate();
        }
        
        void MainPage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            touching = false;
            Point p = e.GetPosition(null);
            ResetDst((int)(oldTouchPoint.X - p.X));

            LWTouchPoint tp = new LWTouchPoint(p.X, p.Y);
            Encoding encoder = Encoding.UTF8;
            byte[] data = encoder.GetBytes(tp.Serialize());

            context.SendMessage((int) LWMessageID.FROM_TOUCHPAD, data);
        }

        void MainPage_MouseMove(object sender, MouseEventArgs e)
        {
            //if (touching)
            //{
            //    Point p = e.GetPosition(null);

            //    ResetDst((int)(oldTouchPoint.X - p.X));

            //    double newX = p.X - 20;
            //    double newY = p.Y - 20;

            //    this.touchIndicator.SetValue(Canvas.LeftProperty, newX);
            //    this.touchIndicator.SetValue(Canvas.TopProperty, newY);

                
            //}
        }

        void MainPage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            touching = true;
            if (this.touchIndicator.Visibility != Visibility.Visible)
            {
                this.touchIndicator.Visibility = Visibility.Visible;
            }
            oldTouchPoint = e.GetPosition(null);
        }


        public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (e.messageID == (int)LWMessageID.CHANGE_PICTURE)
            {
                Encoding enc = Encoding.UTF8;
                String s = enc.GetString(e.messageData, 0, e.messageData.Length);
                BitmapImage im = new BitmapImage(new Uri(s, UriKind.RelativeOrAbsolute));
                this.spectrum.Source = im;
                this.InvalidateArrange();
            }
            if (this.context != null)
            {
                return;
            }
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
