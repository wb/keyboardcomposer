using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Media.Imaging;
using LWEvent;
using Adaptive.ControlsLibrary;
using System.Collections.Generic;
using Adaptive.Interfaces;
using Keyboard = Adaptive.ControlsLibrary.Keyboard;

namespace MoodKeyboardContext
{
    public class LWDrawer
    {
        private static Color normal = Color.FromArgb(255, 255, 255, 255);
        private static Color off = Color.FromArgb(255, 0, 0, 0);
        private static Color highlighted = Color.FromArgb(255, 255, 105, 180);
        private static Color menu = Color.FromArgb(255, 0, 0, 255);
        private static Color test = Color.FromArgb(255, 0, 255, 0);

        private Dictionary<AdaptiveKey, String> noteLengths = new Dictionary<AdaptiveKey, string>();
        private Dictionary<AdaptiveKey, String> times = new Dictionary<AdaptiveKey, string>();
        private Dictionary<AdaptiveKey, String> clefs = new Dictionary<AdaptiveKey, string>();
        private Dictionary<AdaptiveKey, String> restLengths = new Dictionary<AdaptiveKey, string>();
        private Dictionary<AdaptiveKey, String> dots = new Dictionary<AdaptiveKey, string>();
        private Dictionary<AdaptiveKey, Boolean> keysSelectedTable = new Dictionary<AdaptiveKey, bool>();
        private Dictionary<AdaptiveKey, String> unselectableKeys = new Dictionary<AdaptiveKey, string>();
        private Dictionary<AdaptiveKey, String> notes = new Dictionary<AdaptiveKey, string>();
        private Dictionary<AdaptiveKey, String> modeKeys = new Dictionary<AdaptiveKey, string>();
        private Dictionary<DynamicValue, String> dynamics = new Dictionary<DynamicValue, string>();
        private KeyTranslator keyTranslator;
        private LWMode mode = LWMode.NONE;
        private List<Note> currentlySelectedNotes = new List<Note>();
        private Dynamic currentDynamic = new Dynamic(DynamicValue.MF);
        private Dynamic previousDynamic = new Dynamic(DynamicValue.MF);

        public LWDrawer(KeyTranslator kt)
        {
            keyTranslator = kt;

            noteLengths[AdaptiveKey.Z] = "Images/Notes/thirtysecond-note.png";
            noteLengths[AdaptiveKey.X] = "Images/Notes/sixteenth-note.png";
            noteLengths[AdaptiveKey.C] = "Images/Notes/eighth-note.png";
            noteLengths[AdaptiveKey.V] = "Images/Notes/quarter-note.png";
            noteLengths[AdaptiveKey.B] = "Images/Notes/half-note.png";
            noteLengths[AdaptiveKey.N] = "Images/Notes/whole-note.png";

            restLengths[AdaptiveKey.Z] = "Images/Rests/thirtysecond-rest.png";
            restLengths[AdaptiveKey.X] = "Images/Rests/sixteenth-rest.png";
            restLengths[AdaptiveKey.C] = "Images/Rests/eighth-rest.png";
            restLengths[AdaptiveKey.V] = "Images/Rests/quarter-rest.png";
            restLengths[AdaptiveKey.B] = "Images/Rests/half-rest.png";
            restLengths[AdaptiveKey.N] = "Images/Rests/whole-rest.png";

            dots[AdaptiveKey.Comma] = ".";
            dots[AdaptiveKey.Period] = "..";

            unselectableKeys[AdaptiveKey.D1] = "";
            unselectableKeys[AdaptiveKey.Q] = "";
            unselectableKeys[AdaptiveKey.PlusSign] = "";
            unselectableKeys[AdaptiveKey.OpeningBracket] = "";

            notes[AdaptiveKey.W] = "";
            notes[AdaptiveKey.E] = "";
            notes[AdaptiveKey.R] = "";
            notes[AdaptiveKey.T] = "";
            notes[AdaptiveKey.Y] = "";
            notes[AdaptiveKey.U] = "";
            notes[AdaptiveKey.I] = "";
            notes[AdaptiveKey.O] = "";
            notes[AdaptiveKey.P] = "";
            notes[AdaptiveKey.MinusSign] = "";
            notes[AdaptiveKey.D0] = "";
            notes[AdaptiveKey.D9] = "";
            notes[AdaptiveKey.D8] = "";
            notes[AdaptiveKey.D7] = "";
            notes[AdaptiveKey.D6] = "";
            notes[AdaptiveKey.D5] = "";
            notes[AdaptiveKey.D4] = "";
            notes[AdaptiveKey.D3] = "";
            notes[AdaptiveKey.D2] = "";

            times[AdaptiveKey.S] = "Images/Times/cut-time.png";
            times[AdaptiveKey.X] = "Images/Times/3-2.png";
            times[AdaptiveKey.D] = "Images/Times/c.png";
            times[AdaptiveKey.C] = "Images/Times/2-4.png";
            times[AdaptiveKey.F] = "Images/Times/3-4.png";
            times[AdaptiveKey.V] = "Images/Times/5-4.png";
            times[AdaptiveKey.G] = "Images/Times/3-8.png";
            times[AdaptiveKey.B] = "Images/Times/5-8.png";
            times[AdaptiveKey.H] = "Images/Times/6-8.png";
            times[AdaptiveKey.N] = "Images/Times/7-8.png";
            times[AdaptiveKey.J] = "Images/Times/9-8.png";
            times[AdaptiveKey.M] = "Images/Times/12-8.png";

            clefs[AdaptiveKey.C] = "Images/Clefs/treble.png";
            clefs[AdaptiveKey.V] = "Images/Clefs/alto.png";
            clefs[AdaptiveKey.B] = "Images/Clefs/bass.png";
            clefs[AdaptiveKey.N] = "Images/Clefs/percussion.png";
            clefs[AdaptiveKey.M] = "Images/Clefs/tablature.png";

            modeKeys[KeyTranslator.REST_KEY] = "Images/rest.png";
            modeKeys[KeyTranslator.TIME_SIGNATURE_KEY] = "Images/time.png";
            modeKeys[KeyTranslator.CLEF_KEY] = "Images/clef.png";

            dynamics[DynamicValue.PPP] = "Images/Dynamics/ppp.png";
            dynamics[DynamicValue.PP] = "Images/Dynamics/pp.png";
            dynamics[DynamicValue.P] = "Images/Dynamics/p.png";
            dynamics[DynamicValue.MP] = "Images/Dynamics/mp.png";
            dynamics[DynamicValue.MF] = "Images/Dynamics/mf.png";
            dynamics[DynamicValue.F] = "Images/Dynamics/f.png";
            dynamics[DynamicValue.FF] = "Images/Dynamics/ff.png";
            dynamics[DynamicValue.FFF] = "Images/Dynamics/fff.png";
            
            //Keyboard kb = keyTranslator.GetKeyboard();

            //foreach (object item in kb.Items)
            //{
            //    KeyboardKey key = item as KeyboardKey;
            //    SetKeyContentAndColor(key, "", off, "", false);
            //}
        }

        public void Highlight(LWKeyMap map)
        {
            keysSelectedTable.Clear();
            currentlySelectedNotes.Clear();

            if (map != null && map.Count > 0)
            {
                //MessageBox.Show(String.Format("There are {0:G} items in the map!", map.Count));
                foreach (KeyValuePair<LWKey, LWKeyType> kvp in map)
                {
                    AdaptiveKey key = keyTranslator.KeyFromLWKey(kvp.Value, kvp.Key);
                    keysSelectedTable[key] = true;

                    if (kvp.Value == LWKeyType.DYNAMIC)
                    {
                        Dynamic d = kvp.Key as Dynamic;
                        previousDynamic.dynamicValue = currentDynamic.dynamicValue;
                        currentDynamic.dynamicValue = d.dynamicValue;
                    }
                    else if (kvp.Value == LWKeyType.NOTE)
                    {
                        Note n = new Note(kvp.Key as Note);
                        currentlySelectedNotes.Add(n);
                    }
                }

                if (currentlySelectedNotes.Count > 0)
                {
                    mode = LWMode.NOTE;
                }
                else if (map.ContainsValue(LWKeyType.REST))
                {
                    mode = LWMode.REST;
                }
                else if (map.ContainsValue(LWKeyType.TIME_SIGNATURE))
                {
                    mode = LWMode.TIME_SIGNATURE;
                }
                else if (map.ContainsValue(LWKeyType.CLEF))
                {
                    mode = LWMode.CLEF;
                }
                else
                {
                    mode = LWMode.NONE;
                }
            }
            else
            {
                MessageBox.Show("No items in map");
                mode = LWMode.NONE;
            }

            //MessageBox.Show("About to redraw keyboard because of message");
            RedrawKeyboard();
        }

        public void KeyPressed(AdaptiveKey key)
        {
            bool needToRedraw = false;
            ToggleHighlight(key);

            bool willShift = (key == AdaptiveKey.OpeningBracket || key == AdaptiveKey.Q || key == AdaptiveKey.PlusSign || key == AdaptiveKey.D1);

            if (willShift)
            {
                needToRedraw = true;

                foreach (KeyValuePair<AdaptiveKey, String> kvp in notes)
                {
                    keysSelectedTable[kvp.Key] = false;
                }

                switch (key)
                {
                    case AdaptiveKey.OpeningBracket:
                        keyTranslator.ShiftNotesDownOne();
                        break;
                    case AdaptiveKey.Q:
                        keyTranslator.ShiftNotesUpOne();
                        break;
                    case AdaptiveKey.PlusSign:
                        keyTranslator.ShiftNotesUpOctave();
                        break;
                    case AdaptiveKey.D1:
                        keyTranslator.ShiftNotesDownOctave();
                        break;
                }

                foreach (Note noteKey in currentlySelectedNotes)
                {
                    AdaptiveKey newKey = keyTranslator.GetKeyForNote(noteKey);
                    if (newKey != AdaptiveKey.None)
                    {
                        keysSelectedTable[newKey] = true;
                    }
                }
            }

            if (key == KeyTranslator.DYNAMIC_DOWN)
            {
                previousDynamic.dynamicValue = currentDynamic.dynamicValue;
                currentDynamic.dynamicValue = (DynamicValue)Math.Max((int)currentDynamic.dynamicValue - 1, (int) DynamicValue.PPP);
                if (!KeyIsSelected(KeyTranslator.DYNAMIC_KEY) || previousDynamic.dynamicValue == currentDynamic.dynamicValue)
                {
                    needToRedraw = true;
                }
            }
            else if (key == KeyTranslator.DYNAMIC_UP)
            {
                previousDynamic.dynamicValue = currentDynamic.dynamicValue;
                currentDynamic.dynamicValue = (DynamicValue)Math.Min((int)currentDynamic.dynamicValue + 1, (int)DynamicValue.FFF);
                if (!KeyIsSelected(KeyTranslator.DYNAMIC_KEY) || previousDynamic.dynamicValue == currentDynamic.dynamicValue)
                {
                    needToRedraw = true;
                }
            }

            LWMode newMode = keyTranslator.ModeFromKey(key, keysSelectedTable, currentlySelectedNotes.Count == 0);

            if (newMode != LWMode.STAY_THE_SAME)
            {
                if (mode != newMode)
                {
                    if (newMode != LWMode.NOTE)
                    {
                        currentlySelectedNotes.Clear();
                    }

                    mode = newMode;
                }
            }

            //MessageBox.Show("About to redraw keyboard because of keypress");
            if (needToRedraw)
            {
                RedrawKeyboard();
            }
        }

        public void ToggleHighlight(AdaptiveKey key)
        {
            if (unselectableKeys.ContainsKey(key) || (notes.ContainsKey(key) && !keyTranslator.KeyShowsANote(key)))
            {
                return;
            }

            Dictionary<AdaptiveKey, String> dictToClear = new Dictionary<AdaptiveKey, String>();

            if (noteLengths.ContainsKey(key) && mode == LWMode.NOTE)
                dictToClear = noteLengths;
            else if (restLengths.ContainsKey(key) && mode == LWMode.REST)
                dictToClear = restLengths;
            else if (times.ContainsKey(key) && mode == LWMode.TIME_SIGNATURE)
                dictToClear = times;
            else if (clefs.ContainsKey(key) && mode == LWMode.CLEF)
                dictToClear = clefs;
            else if (dots.ContainsKey(key) && (mode == LWMode.NOTE || mode == LWMode.REST))
            {
                dictToClear = new Dictionary<AdaptiveKey,string>(dots);
                dictToClear.Remove(key);
            }

            foreach (KeyValuePair<AdaptiveKey, String> kvp in dictToClear)
            {
                keysSelectedTable[kvp.Key] = false;
            }

            if (!keysSelectedTable.ContainsKey(key) || !keysSelectedTable[key])
            {
                keysSelectedTable[key] = true;

                if (keyTranslator.KeyShowsANote(key))
                {
                    currentlySelectedNotes.Add(new Note(keyTranslator.GetNoteForKey(key)));
                }
            }
            else
            {
                keysSelectedTable[key] = false;

                if (keyTranslator.KeyShowsANote(key))
                {
                    currentlySelectedNotes.Remove(keyTranslator.GetNoteForKey(key));
                }
            }
        }

        private bool KeyIsSelected(AdaptiveKey key)
        {
            if (keysSelectedTable.ContainsKey(key))
            {
                return keysSelectedTable[key];
            }

            return false;
        }

        private Color GetColorForKey(AdaptiveKey key)
        {
            if (KeyIsSelected(key))
            {
                if (modeKeys.ContainsKey(key))
                {
                    return menu;
                }
                else
                {
                    return highlighted;
                }
            }
            else
            {
                return normal;
            }
        }

        private void ColorOtherKeysAndNotes(KeyboardKey key)
        {
            if (keyTranslator.KeyShowsANote(key.Key))
            {
                Note note = keyTranslator.GetNoteForKey(key.Key);
                SetKeyContentAndColor(key, keyTranslator.GetStringForNote(note), GetColorForKey(key.Key), "", false);
            }
            else
            {
                switch (key.Key)
                {
                    case AdaptiveKey.PlusSign:
                        SetKeyContentAndColor(key, ">>", normal, "Images/ottava.png", false);
                        break;
                    case AdaptiveKey.OpeningBracket:
                        SetKeyContentAndColor(key, ">>", normal, "", false);
                        break;
                    case AdaptiveKey.D1:
                        SetKeyContentAndColor(key, "<<", normal, "Images/ottava.png", false);
                        break;
                    case AdaptiveKey.Q:
                        SetKeyContentAndColor(key, "<<", normal, "", false);
                        break;
                    case KeyTranslator.REST_KEY:
                    case KeyTranslator.TIME_SIGNATURE_KEY:
                    case KeyTranslator.CLEF_KEY:
                        SetKeyContentAndColor(key, "", GetColorForKey(key.Key), modeKeys[key.Key], false);
                        break;
                    case KeyTranslator.SLUR_KEY:
                        SetKeyContentAndColor(key, "", GetColorForKey(key.Key), "Images/slur.png", false);
                        break;
                    case KeyTranslator.CRESCENDO_KEY:
                        SetKeyContentAndColor(key, "", GetColorForKey(key.Key), "Images/crescendo.png", false);
                        break;
                    case KeyTranslator.DECRESCENDO_KEY:
                        SetKeyContentAndColor(key, "", GetColorForKey(key.Key), "Images/decrescendo.png", false);
                        break;
                    case AdaptiveKey.Space:
                        SetKeyContentAndColor(key, "Next", normal, "", false);
                        break;
                    case AdaptiveKey.Left:
                        SetKeyContentAndColor(key, "", normal, "Images/LeftArrow.png", false);
                        break;
                    case AdaptiveKey.Right:
                        SetKeyContentAndColor(key, "", normal, "Images/RightArrow.png", false);
                        break;
                    default:
                        SetKeyContentAndColor(key, "", test, "", false);
                        break;
                }
            }
        }

        public void RedrawKeyboard()
        {
            Keyboard kb = keyTranslator.GetKeyboard();
            if (kb == null)
            {
                MessageBox.Show("Keyboard is null!");
            }

            //MessageBox.Show(String.Format("Keyboard has {0:G} items, mode is {1:G}", kb.Items.Count, mode));

            foreach (object item in kb.Items)
            {
                KeyboardKey key = item as KeyboardKey;

                if (key != null)
                {
                    switch (mode)
                    {
                        case LWMode.NOTE:
                            if (noteLengths.ContainsKey(key.Key))
                            {
                                SetKeyContentAndColor(key, "", GetColorForKey(key.Key), noteLengths[key.Key], false);
                            }
                            else if (dots.ContainsKey(key.Key))
                            {
                                SetKeyContentAndColor(key, dots[key.Key], GetColorForKey(key.Key), "", false);
                            }
                            else if (key.Key == KeyTranslator.DYNAMIC_UP && currentDynamic.dynamicValue != DynamicValue.FFF)
                            {
                                SetKeyContentAndColor(key, "", normal, "Images/volumeUp.png", false);
                            }
                            else if (key.Key == KeyTranslator.DYNAMIC_KEY)
                            {
                                SetKeyContentAndColor(key, "", GetColorForKey(key.Key), dynamics[currentDynamic.dynamicValue], false);
                            }
                            else if (key.Key == KeyTranslator.DYNAMIC_DOWN && currentDynamic.dynamicValue != DynamicValue.PPP)
                            {
                                SetKeyContentAndColor(key, "", normal, "Images/volumeDown.png", false);
                            }
                            else
                            {
                                ColorOtherKeysAndNotes(key);
                            }
                            break;
                        case LWMode.REST:
                            if (restLengths.ContainsKey(key.Key))
                            {
                                SetKeyContentAndColor(key, "", GetColorForKey(key.Key), restLengths[key.Key], false);
                            }
                            else if (dots.ContainsKey(key.Key))
                            {
                                SetKeyContentAndColor(key, dots[key.Key], GetColorForKey(key.Key), "", false);
                            }
                            else
                            {
                                ColorOtherKeysAndNotes(key);
                            }
                            break;

                        case LWMode.TIME_SIGNATURE:
                            if (times.ContainsKey(key.Key))
                            {
                                SetKeyContentAndColor(key, "", GetColorForKey(key.Key), times[key.Key], false);
                            }
                            else
                            {
                                ColorOtherKeysAndNotes(key);
                            }
                            break;

                        case LWMode.CLEF:
                            if (clefs.ContainsKey(key.Key))
                            {
                                SetKeyContentAndColor(key, "", GetColorForKey(key.Key), clefs[key.Key], false);
                            }
                            else
                            {
                                ColorOtherKeysAndNotes(key);
                            }
                            break;
                        default:
                            ColorOtherKeysAndNotes(key);
                            break;
                    }
                }
                else { MessageBox.Show("Key was null!"); }
            }
            
        }

        void AddImage(String path, UIElementCollection coll, VerticalAlignment align)
        {
            Image img = new Image();
            Uri uri = new Uri(path, UriKind.Relative);
            img.Source = new BitmapImage(uri);
            img.Stretch = Stretch.Uniform;
            img.VerticalAlignment = align;
            coll.Add(img);
        }

        void AddText(String text, UIElementCollection coll, VerticalAlignment align)
        {
            TextBlock tb = new TextBlock();
            tb.Text = text;
            tb.TextAlignment = TextAlignment.Center;
            tb.VerticalAlignment = align;
            coll.Add(tb);
        }

        bool SetKeyContentAndColor(KeyboardKey key, String text, Color color, String imgPath, bool textOnTop)
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
                    UIElementCollection children = grid.Children;
                    children.Clear();

                    bool hasimage = !(imgPath.Length == 0);
                    bool hastext = !(text.Length == 0);

                    if (hasimage && hastext)
                    {
                        if (textOnTop)
                        {
                            AddText(text, children, VerticalAlignment.Top);
                            AddImage(imgPath, children, VerticalAlignment.Bottom);
                        }
                        else
                        {
                            AddImage(imgPath, children, VerticalAlignment.Top);
                            AddText(text, children, VerticalAlignment.Bottom);
                        }
                    }

                    else if (hasimage)
                    {
                        AddImage(imgPath, children, VerticalAlignment.Center);
                    }

                    else if (hastext)
                    {
                        AddText(text, children, VerticalAlignment.Center);
                    }

                    if (color != null)
                    {
                        grid.Background = new SolidColorBrush(color);
                    }

                    key.Items.Clear();
                    key.Items.Add(ks);

                    return true;
                }
            }
            return false;
        }

        public LWEventData TranslateKeyboardKeyToEvent(AdaptiveKey key)
        {
            LWKey eventKey = new None();
            LWKeyType type = LWKeyType.NOT_IMPLEMENTED;
            
            if (keyTranslator.KeyShowsANote(key))
            {
                eventKey = keyTranslator.GetNoteForKey(key);
                type = LWKeyType.NOTE;
            }
            else
            {
                switch (key)
                {
                    case KeyTranslator.REST_KEY:
                        eventKey = new Rest();
                        type = LWKeyType.REST;
                        break;
                    case KeyTranslator.TIME_SIGNATURE_KEY:
                        eventKey = new TimeSignature();
                        type = LWKeyType.TIME_SIGNATURE;
                        break;
                    case KeyTranslator.CLEF_KEY:
                        eventKey = new Clef();
                        type = LWKeyType.CLEF;
                        break;
                    case AdaptiveKey.Space:
                        eventKey = new Space();
                        type = LWKeyType.SPACE;
                        break;
                    case KeyTranslator.SLUR_KEY:
                        eventKey = new Slur();
                        type = LWKeyType.SLUR;
                        break;
                    case KeyTranslator.CRESCENDO_KEY:
                        eventKey = new Crescendo();
                        type = LWKeyType.CRESCENDO;
                        break;
                    case KeyTranslator.DECRESCENDO_KEY:
                        eventKey = new Decrescendo();
                        type = LWKeyType.DECRESCENDO;
                        break;
                    case AdaptiveKey.Left:
                        eventKey = new ArrowLeft();
                        type = LWKeyType.ARROW_LEFT;
                        break;
                    case AdaptiveKey.Right:
                        eventKey = new ArrowRight();
                        type = LWKeyType.ARROW_RIGHT;
                        break;
                    default:
                        switch (mode)
                        {
                            case LWMode.NOTE:
                                if (noteLengths.ContainsKey(key))
                                {
                                    eventKey = new InverseDuration(keyTranslator.DurationFromKey(key));
                                    type = LWKeyType.INVERSE_DURATION;
                                }
                                else if (dots.ContainsKey(key))
                                {
                                    int numDots = 0;
                                    if (KeyIsSelected(key))
                                    {
                                        numDots = keyTranslator.DotsFromKey(key);
                                    }
                                    eventKey = new Dots(numDots);
                                    type = LWKeyType.DOTS;
                                }
                                    // Confusing, so I'll explain:
                                    // If the down key was pressed and we care because the dynamic is highlighted,
                                    // but we're not at pianissimo for at least the second time (where we can't go any lower, send a notification
                                    // Likewise for up key and not fortissimo
                                    // Finally, also send one whenever the dynamic key is pressed itself
                                else if ((key == KeyTranslator.DYNAMIC_DOWN
                                                    && KeyIsSelected(KeyTranslator.DYNAMIC_KEY)
                                                    && !(currentDynamic.dynamicValue == DynamicValue.PPP && previousDynamic.dynamicValue == DynamicValue.PPP))
                                    
                                      || (key == KeyTranslator.DYNAMIC_UP
                                                    && KeyIsSelected(KeyTranslator.DYNAMIC_KEY)
                                                    && !(currentDynamic.dynamicValue == DynamicValue.FFF && previousDynamic.dynamicValue == DynamicValue.FFF))
                                    
                                      ||  key == KeyTranslator.DYNAMIC_KEY)
                                {
                                    eventKey = new Dynamic(currentDynamic.dynamicValue);
                                    type = LWKeyType.DYNAMIC;
                                }
                                break;
                            case LWMode.REST:
                                if (restLengths.ContainsKey(key))
                                {
                                    eventKey = new InverseDuration(keyTranslator.DurationFromKey(key));
                                    type = LWKeyType.INVERSE_DURATION;
                                }
                                else if (dots.ContainsKey(key))
                                {
                                    int numDots = 0;
                                    if (KeyIsSelected(key))
                                    {
                                        numDots = keyTranslator.DotsFromKey(key);
                                    }
                                    eventKey = new Dots(numDots);
                                    type = LWKeyType.DOTS;
                                }
                                break;
                            case LWMode.CLEF:
                                if (clefs.ContainsKey(key))
                                {
                                    eventKey = new ClefSpecific(keyTranslator.ClefFromKey(key));
                                    type = LWKeyType.CLEF_SPECIFIC;
                                }
                                break;
                            case LWMode.TIME_SIGNATURE:
                                if (times.ContainsKey(key))
                                {
                                    eventKey = new TimeSignatureSpecific(keyTranslator.TimePairFromKey(key));
                                    type = LWKeyType.TIME_SIGNATURE_SPECIFIC;
                                }
                                break;
                        }
                        break;
                }
            }

            LWEventData eventData = new LWEventData(eventKey, type);
            return eventData;
        }

    }
}
