using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Adaptive.ControlsLibrary;
using Adaptive.Interfaces;
using Keyboard = Adaptive.ControlsLibrary.Keyboard;
using LWEvent;

using System.IO;
using System.Runtime.Serialization;
using System.Text;

namespace MoodKeyboardContext
{
    [AdaptiveKeyRegion]
    public partial class KeyboardContent : UserControl, IAdaptiveContentProvider
    {
        private Storyboard bgStory = new Storyboard();
        private bool animationReady = false;
        static KeyboardContent sKeyboardContent;
        private KeyboardHandler keyboardHandler;
        private IAdaptiveContextMessaging context = null;

        private static Color highlighted = Color.FromArgb(255, 0, 0, 255);
        private static Color plain = Color.FromArgb(255, 0, 0, 0);

        private Boolean havePaintedKeyboard = false;

        private KeyTranslator keyTranslator;
        private LWDrawer drawer;

        public KeyboardContent()
        {
            Console.WriteLine("KeyboardContent::UserControl::KeyboardContent");
            InitializeComponent();
            sKeyboardContent = this;

            this.Loaded += new RoutedEventHandler(KeySetDefault_Loaded);
        }

        public void OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (context != null)
            {
                // Do something with the message:
                //  e.messageID   is the message ID
                //  e.messageData is the message data as a byte array
            }
        }
        
        // Interface method implementation
        // This method is called by the Runtime when this object is instantiated
        public void SetMessagingObject(IAdaptiveContextMessaging context)
        {
            if (context != null)
            {
                this.context = context;

                // Subscribe to message received events
                context.MessageReceived += new EventHandler<MessageReceivedEventArgs>(OnMessageReceived);
            }
        }


        void KeySetDefault_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded -= new RoutedEventHandler(KeySetDefault_Loaded);
            keyTranslator = new KeyTranslator(this.LayoutRoot.Children[0] as Keyboard);
            drawer = new LWDrawer(keyTranslator);
            drawer.RedrawKeyboard();

            // Get the Runtime object
            IAdaptiveRuntime runtime = Application.Current as IAdaptiveRuntime;

            if (runtime != null)
            {
                // Subscribe to the Adaptive Runtime keypress events: 
                runtime.KeyPressed += new EventHandler<KeyPressEventArgs>(runtime_KeyPressed);
            }
            else
            {
                // If we failed to acquire the runtime, we must be running in a
                // browser. In this case we can wire up standard Silverlight key
                // events. Although not all key events are supported, this is a
                // usefull in debugging as it may be easier for designers to iterate
                // on keyboard design in a browser
                this.KeyDown += new KeyEventHandler(KeyboardContext_KeyDown);
                this.KeyUp += new KeyEventHandler(KeyboardContext_KeyUp);
            }
        }


        /// <summary>
        /// Runtime keypress event handler.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void runtime_KeyPressed(object sender, KeyPressEventArgs e)
        {
            if (e.fKeyUp)
            {
            }
            else
            {
                KeyboardKey key = FindKeyPressed(e.scanCode);

                drawer.KeyPressed(key.Key);

                LWEventData eventData = drawer.TranslateKeyboardKeyToEvent(key.Key);

                if (eventData.eventType != LWKeyType.NOT_IMPLEMENTED)
                {
                    String eventStr = eventData.Serialize();
                    Encoding encoder = Encoding.UTF8;
                    byte[] data = encoder.GetBytes(eventStr);
                    context.SendMessage((int) LWMessageID.FROM_KEYBOARD, data);
                }
            }

            // Here we simply forward this information to the Keyboard handler
            // Although one could preprocess/postprocess the events as well
            keyboardHandler.KbdEvent(e.fKeyUp, (byte)e.scanCode);
        }

        KeyboardKey FindKeyPressed(int scanCode)
        {
            Keyboard keyboard = this.LayoutRoot.Children[0] as Keyboard;

            foreach (object item in keyboard.Items)
            {
                KeyboardKey key = item as KeyboardKey;
                if (key != null)
                {
                    if (scanCode == key.ScanCode)
                    {
                        return key;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// This is the Silverlight standard keydown event handler.
        /// Note that this code is only for demonstration/debugging purposes
        /// it will never be invoked when inside the Adaptive Runtime.
        /// 
        /// This handler makes it possible to test this keyboard design as
        /// a standalone Silverlight application (i.e. inside the browser).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void KeyboardContext_KeyDown(object sender, KeyEventArgs e)
        {
            // NOTE: this is only needed to enable in-browser testing
            if (e.PlatformKeyCode == 0x5d)
            {
                // In the browser we can't get the Alt keys,
                // so re-purpose the Application key to "pretend" it's Alt
                keyboardHandler.KbdEvent(false, (byte)0xa4);
            }
            else
            {
                keyboardHandler.KbdEvent(false, (byte)e.PlatformKeyCode);
            }
        }

        /// <summary>
        /// This is the Silverlight standard keyup event handler.
        /// Note that this code is only for demonstration/debugging purposes
        /// it will never be invoked when inside the Adaptive Runtime.
        /// 
        /// This handler makes it possible to test this keyboard design as
        /// a standalone Silverlight application (i.e. inside the browser).
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void KeyboardContext_KeyUp(object sender, KeyEventArgs e)
        {
            // NOTE: this is only needed to enable in-browser testing
            if (e.PlatformKeyCode == 0x5d)
            {
                // In the browser we can't get the Alt keys,
                // so re-purpose the Application key to "pretend" it's Alt
                keyboardHandler.KbdEvent(true, (byte)0xa4);
            }
            else
            {
                keyboardHandler.KbdEvent(true, (byte)e.PlatformKeyCode);
            }
        }

        
        private void setupAnimations()
        {
            Color ToColor = Color.FromArgb(255, 13, 8, 116);
            int count = 0;

            Keyboard keyboard = this.LayoutRoot.Children[0] as Keyboard;
            foreach (object item in keyboard.Items)
            {
                KeyboardKey key = item as KeyboardKey;
                if (key != null)
                {
                    //get normal keystate
                    KeyState ks = key.Items[0] as KeyState;
                    if (ks != null)
                    {
                        Grid grid = ks.Content as Grid;
                        if (grid != null)
                        {
                            grid.Height = ks.ActualHeight;
                            grid.Width = ks.ActualWidth;
                            grid.Background = new SolidColorBrush(Color.FromArgb(0, 255, 0, 0));
                           
                            ColorAnimation animation = new ColorAnimation();
                            animation.To = ToColor;
                            animation.BeginTime = TimeSpan.FromMilliseconds(75 * count);
                            animation.Duration = TimeSpan.FromSeconds(1.0);

                            if (count == 10)
                            {
                                animation.BeginTime = TimeSpan.FromMilliseconds(0);
                                animation.Duration = TimeSpan.FromSeconds(0);
                            }

                            Storyboard.SetTarget(animation, grid);
                            Storyboard.SetTargetProperty(animation, new PropertyPath("(Grid.Background).(SolidColorBrush.Color)"));

                            this.bgStory.Children.Add(animation);
                            count += 1;
                        }
                    }
                }
            }
            LayoutRoot.Resources.Add("bgStory", this.bgStory);	
            this.animationReady = true;
        }

        internal static void doAnimation()
        {
            Console.WriteLine("Doing animation");
            KeyboardContent.sKeyboardContent.bgStory.Begin();
        }

        internal static void paintRed()
        {
            if (sKeyboardContent != null)
            {
                Adaptive.ControlsLibrary.Keyboard keyboard = KeyboardContent.sKeyboardContent.LayoutRoot.Children[0] as Adaptive.ControlsLibrary.Keyboard;

                if (keyboard != null)
                {
                    foreach (object item in keyboard.Items)
                    {
                        KeyboardKey key = item as KeyboardKey;
                        if (key != null)
                        {
                            //get normal keystate
                            KeyState ks = key.Items[0] as KeyState;
                            if (ks != null)
                            { 
                                Grid grid = ks.Content as Grid;
                                if (grid != null)
                                {
                                    grid.Height = ks.ActualHeight;
                                    grid.Width = ks.ActualWidth;


                                    Color c = Color.FromArgb(255, 255, 0, 0);
                                    grid.Background = new SolidColorBrush(c);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static void SetToColor(Color c)
        {
            Console.WriteLine("SetToColor");
            if (sKeyboardContent != null)
            {
                Adaptive.ControlsLibrary.Keyboard keyboard = KeyboardContent.sKeyboardContent.LayoutRoot.Children[0] as Adaptive.ControlsLibrary.Keyboard;

                if (keyboard != null)
                {
                    foreach (object item in keyboard.Items)
                    {
                        KeyboardKey key = item as KeyboardKey;
                        if (key != null)
                        {
                            //get normal keystate
                            KeyState ks = key.Items[0] as KeyState;
                            if (ks != null)
                            {
                                Grid grid = ks.Content as Grid;
                                if (grid != null)
                                {
                                    grid.Height = ks.ActualHeight;
                                    grid.Width = ks.ActualWidth;

                                    grid.Background = new SolidColorBrush(c);
                                }
                            }
                        }
                    }
                }
            }
        }

        internal static void AnimateToColor(Color color)
        {
            if (sKeyboardContent != null)
            {
                if (!sKeyboardContent.animationReady)
                {
                    sKeyboardContent.setupAnimations();
                }

                //sKeyboardContent.bgStory.Stop();
                foreach (Timeline animation in sKeyboardContent.bgStory.Children)
                {
                    ColorAnimation colorAnim = animation as ColorAnimation;
                    
                    if (colorAnim != null)
                    {
                        if (Color.Equals(colorAnim.To, plain))
                        {
                            colorAnim.To = highlighted;
                        }
                        else
                        {
                            colorAnim.To = plain;
                        }
                        //colorAnim.To = color;
                    }
                }

                sKeyboardContent.bgStory.Begin();    
            }

        }
        
        internal static void AnimateKeys()
        {
            Console.WriteLine("AnimateKeys");
            int count = 0;
            Color ToColor = Color.FromArgb(255,142,112,0);
            Storyboard storyboard = new Storyboard();

            if (sKeyboardContent != null)
            {
                Keyboard keyboard = KeyboardContent.sKeyboardContent.LayoutRoot.Children[0] as Keyboard;

                if (keyboard != null)
                {
                    
                    // Find the keys we are looking to animate
                    foreach (object item in keyboard.Items)
                    {
                        KeyboardKey key = item as KeyboardKey;
                        if (key != null)
                        {
                            
                            if (true)
                            {
                                KeyState ks = key.Items[0] as KeyState;
                                if (ks != null)
                                {
                                    
                                    Grid grid = ks.Content as Grid;

                                    // Set the background brush for the keys' grid to our
                                    // animated brush
                                    if (grid != null)
                                    {
                                        // Set the grid to be the full size of the key
                                        grid.Height = ks.ActualHeight;
                                        grid.Width = ks.ActualWidth;
                                        //grid.Background = new SolidColorBrush(Color.FromArgb(255,255,0,0));                                     
                                        
                                        ColorAnimation animation = new ColorAnimation();
                                        animation.To = ToColor;
                                        animation.BeginTime = TimeSpan.FromMilliseconds(45 * count);
                                        animation.Duration = TimeSpan.FromSeconds(0.5);
                                        Storyboard.SetTarget(animation, grid);
                                        Storyboard.SetTargetProperty(animation, new PropertyPath("(Grid.Background).(SolidColorBrush.Color)"));

                                        
                                        storyboard.Children.Add(animation);


                                        count += 1;
                                    }
                                     
                                }
                            }
                        }
                    }
                }
            }
            storyboard.Begin();
        }

    }
}
