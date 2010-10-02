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
using LWEvent;

using System.IO;
using System.Runtime.Serialization;
using System.Xml;


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

        public Window1()
        {
            InitializeComponent();

            keyToPng = new KeyToPng();

            bool result;

            result = InitializeAdaptive();
            if (!result)
                return;

            LoadAdaptiveContext();
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
                // Message from the keyboard
                System.Text.Encoding enc = System.Text.Encoding.UTF8;
                String dataStr = enc.GetString(data, 0, data.Length);
                //Console.WriteLine(dataStr);
                keyToPng.HandleKey(LWEventData.Deserialize(dataStr));
            }
        }

        private void goButton_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("painting");
            byte[] data = new byte[3];  //r,g,b
            data[0] = 0xFF;
            data[1] = 0x00;
            data[2] = 0x00;
            this.adaptiveContextManager.PostContextMessage(this.adaptiveContext, 5, data, (uint)data.Length);
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
}
