using System;
using System.Collections.Generic;
using System.Linq;
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
using System.Windows.Markup;
using Microsoft.Adaptive;
using LWContextCommunication;
using System.ComponentModel;
using System.Windows.Forms;

using System.IO;
using System.Runtime.Serialization;
using System.Xml;
using System.Threading;


namespace MoodKeyboard
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window, IAdaptiveDelegateInterface
    {
        private AdaptiveContextManager adaptiveContextManager;
        private int adaptiveContext;
        private KeyToPng keyToPng;
        private ThreadedImageRefresher tir;
        private Thread thread;

        public Window1()
        {
            InitializeComponent();

            keyToPng = new KeyToPng();

            bool result;

            result = InitializeAdaptive();
            if (!result)
                return;

            LoadAdaptiveContext();

            tir = new ThreadedImageRefresher(this);
            thread = new Thread(new ThreadStart(tir.ImageReloadThread));
            thread.IsBackground = true;
            thread.Start();
        }


        private bool InitializeAdaptive()
        {
            Adaptive adaptive = null;
            try
            {
                adaptive = new Adaptive();
            }
            catch (XamlParseException)
            {
                // A XamlParseException gets thrown when the AdaptiveRuntime instantiation fails                
                return false;
            }

            try
            {
                adaptive.GetContextManager(out adaptiveContextManager);
                if (adaptiveContextManager == null)
                {                    
                    return false;
                }

            }
            catch (System.ApplicationException)
            {
                return false;
            }

            return true;
        }

        private void LoadAdaptiveContext()
        {
            string xapFileName = "MoodKeyboardContext.xap";
            string currentDirectory = System.IO.Directory.GetCurrentDirectory();
            string xapPath = System.IO.Path.Combine(currentDirectory, xapFileName);

            Console.WriteLine(xapPath);

            this.adaptiveContext = this.adaptiveContextManager.CreateContext(xapPath);
            this.adaptiveContextManager.RegisterMessageCallback(this.adaptiveContext, new AdaptiveMessageCallback(this));
            this.adaptiveContextManager.ActivateContext(this.adaptiveContext);
        }

        public void ReceiveMessageFromContext(int contextId, int messageId, byte[] data)
        {
            if (messageId == (int) LWMessageID.FROM_KEYBOARD)
            {
                tir.setMessage(messageId + "");

                // Message from the keyboard
                System.Text.Encoding enc = System.Text.Encoding.UTF8;
                String dataStr = enc.GetString(data, 0, data.Length);
                bool updateImage = keyToPng.HandleKey(LWKeyEvent.Deserialize(dataStr));
                byte[] cereal = keyToPng.score.currentSliceCereal();
                Console.WriteLine("Message: " + enc.GetString(cereal, 0, cereal.Length));
                this.adaptiveContextManager.PostContextMessage(this.adaptiveContext, (int)LWMessageID.HIGHLIGHT_KEYS, cereal, (uint) cereal.Length);

                if (updateImage)
                {
                    tir.RequestImageReload();
                }
            }
        }

        public void UpdateImage()
        {

            Action<String > DoUpdateText;
            DoUpdateText = (text) =>
            {
                Feedback.Text = text;
            };
            Dispatcher.BeginInvoke(DoUpdateText, "Rendering score...");

            keyToPng.UpdateImage();

            String s = keyToPng.getCurrentImageURI();
            Console.WriteLine("Loading image " + s);
            System.Text.Encoding enc = System.Text.Encoding.UTF8;
            byte[] data = enc.GetBytes(s);

            Action<String> DoUpdateImage;
            DoUpdateImage = (path) =>
            {
                Img_Score.Source = new BitmapImage(new Uri(path, UriKind.Absolute));
            };
            Dispatcher.BeginInvoke(DoUpdateImage, s);
            Dispatcher.BeginInvoke(DoUpdateText, "");

            this.adaptiveContextManager.PostContextMessage(this.adaptiveContext, (int)LWMessageID.CHANGE_PICTURE, data, (uint)data.Length);
        }

        private void Img_Score_ImageFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Console.WriteLine("Image failed to load into main window.");
        }

        private void LWLoad_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFile1 = new OpenFileDialog();

            // Initialize the OpenFileDialog to look for text files.
            openFile1.Filter = "Lilypond Files|*.ly";

            // Check if the user selected a file from the OpenFileDialog.
            if (openFile1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Console.WriteLine("Opening " + openFile1.FileName);
                // TODO
            }
        }

        private void LWSave_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFile1 = new SaveFileDialog();

            saveFile1.Filter = "Lilypond Files|*.ly";

            if (saveFile1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                Feedback.Text = "Saved";
                // TODO
                Console.WriteLine("Saved file to " + saveFile1.FileName);
            }
        }       
    }

    //This is an interface that classes that can respond to adaptive messages must implement.
    public interface IAdaptiveDelegateInterface
    {
        void ReceiveMessageFromContext(int contextID, int messageID, byte[] data);
    }

    //This is a wrapper class to handle adaptive message callbacks. The wrapper will forward
    //callbacks to a delegate that implements the IAdaptiveDelegateInterface.
    //Alternatively, the main view can implement IAdaptiveMessageCallback

    public class AdaptiveMessageCallback : IAdaptiveMessageCallback
    {
        IAdaptiveDelegateInterface handler = null;
        public AdaptiveMessageCallback(IAdaptiveDelegateInterface handler)
        {
            this.handler = handler;
        }

        public void ReceiveContextMessage(int ctxId, int msgId, byte[] pbBlob, uint uBlobSize)
        {
            // send the message back to the window
            handler.ReceiveMessageFromContext(ctxId, msgId, pbBlob);
        }
    }

    public class ThreadedImageRefresher
    {
        private String message = "hi";
        private Window1 window;
        private int count = 0;

        public ThreadedImageRefresher(Window1 window)
        {
            this.window = window;
        }

        public void setMessage(String message)
        {
            this.message = message;
        }

        public void ImageReloadThread()
        {
            while (true)
            {
                if (count > 0)
                {
                    count = 0;

                    Console.WriteLine("Updating image.");
                    window.UpdateImage();
                }
                else
                {
                    System.Threading.Thread.Sleep(1000);
                }
            }
        }

        public void RequestImageReload()
        {
            count++;
        }

       

    }
}
