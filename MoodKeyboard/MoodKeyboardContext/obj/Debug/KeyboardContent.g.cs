﻿#pragma checksum "C:\Users\Walter\Documents\Adaptive Keyboard Development\Sample Code\MoodKeyboard\MoodKeyboardContext\KeyboardContent.xaml" "{406ea660-64cf-4c82-b6f0-42d48172a799}" "352B8ECCDC7ECB9DC96406C1287B4103"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.4952
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Adaptive.ControlsLibrary;
using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shapes;
using System.Windows.Threading;


namespace MoodKeyboardContext {
    
    
    public partial class KeyboardContent : System.Windows.Controls.UserControl {
        
        internal System.Windows.Controls.Grid LayoutRoot;
        
        internal System.Windows.VisualStateGroup KeyboardStates;
        
        internal System.Windows.VisualState Normal;
        
        internal System.Windows.VisualState Shift;
        
        internal System.Windows.VisualState Control;
        
        internal System.Windows.VisualState Alt;
        
        internal Adaptive.ControlsLibrary.Keyboard Keyboard;
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Windows.Application.LoadComponent(this, new System.Uri("/MoodKeyboardContext;component/KeyboardContent.xaml", System.UriKind.Relative));
            this.LayoutRoot = ((System.Windows.Controls.Grid)(this.FindName("LayoutRoot")));
            this.KeyboardStates = ((System.Windows.VisualStateGroup)(this.FindName("KeyboardStates")));
            this.Normal = ((System.Windows.VisualState)(this.FindName("Normal")));
            this.Shift = ((System.Windows.VisualState)(this.FindName("Shift")));
            this.Control = ((System.Windows.VisualState)(this.FindName("Control")));
            this.Alt = ((System.Windows.VisualState)(this.FindName("Alt")));
            this.Keyboard = ((Adaptive.ControlsLibrary.Keyboard)(this.FindName("Keyboard")));
        }
    }
}

