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
using LWEvent;
using Adaptive.ControlsLibrary;
using System.Collections.Generic;
using Adaptive.Interfaces;
using Keyboard = Adaptive.ControlsLibrary.Keyboard;
using System.Windows.Media.Imaging;

namespace MoodKeyboardContext
{
    public class KeyTranslator
    {
        private static int STARTING_OCTAVE = 4;
        public const AdaptiveKey REST_KEY = AdaptiveKey.F7;
        public const AdaptiveKey TIME_SIGNATURE_KEY = AdaptiveKey.F6;
        public const AdaptiveKey CLEF_KEY = AdaptiveKey.F5;
        public const AdaptiveKey DYNAMIC_KEY = AdaptiveKey.L;
        public const AdaptiveKey DYNAMIC_UP = AdaptiveKey.Semicolon;
        public const AdaptiveKey DYNAMIC_DOWN = AdaptiveKey.K;
        public const AdaptiveKey SLUR_KEY = AdaptiveKey.F9;
        public const AdaptiveKey CRESCENDO = AdaptiveKey.F10;
        public const AdaptiveKey DECRESCENDO = AdaptiveKey.F11;

        private Dictionary<AdaptiveKey, Note> keysToNotes;
        private Keyboard keyboard;

        private static Color white = Color.FromArgb(255, 255, 255, 255);
        private static Color black = Color.FromArgb(255, 0, 0, 0);
        private static Color highlighted = Color.FromArgb(255, 255, 105, 180);


        public KeyTranslator(Keyboard keyboard)
        {
            this.keyboard = keyboard;

            keysToNotes = new Dictionary<AdaptiveKey, Note>();
            keysToNotes[AdaptiveKey.W] = new Note(NoteValue.A, STARTING_OCTAVE - 1);
            keysToNotes[AdaptiveKey.E] = new Note(NoteValue.B, STARTING_OCTAVE - 1);
            keysToNotes[AdaptiveKey.R] = new Note(NoteValue.C, STARTING_OCTAVE);
            keysToNotes[AdaptiveKey.T] = new Note(NoteValue.D, STARTING_OCTAVE);
            keysToNotes[AdaptiveKey.Y] = new Note(NoteValue.E, STARTING_OCTAVE);
            keysToNotes[AdaptiveKey.U] = new Note(NoteValue.F, STARTING_OCTAVE);
            keysToNotes[AdaptiveKey.I] = new Note(NoteValue.G, STARTING_OCTAVE);
            keysToNotes[AdaptiveKey.O] = new Note(NoteValue.A, STARTING_OCTAVE);
            keysToNotes[AdaptiveKey.P] = new Note(NoteValue.B, STARTING_OCTAVE);
            keysToNotes[AdaptiveKey.D2] = new Note(NoteValue.G_SHARP, STARTING_OCTAVE - 1);
            keysToNotes[AdaptiveKey.D3] = new Note(NoteValue.A_SHARP, STARTING_OCTAVE - 1);
            keysToNotes[AdaptiveKey.D5] = new Note(NoteValue.C_SHARP, STARTING_OCTAVE);
            keysToNotes[AdaptiveKey.D6] = new Note(NoteValue.D_SHARP, STARTING_OCTAVE);
            keysToNotes[AdaptiveKey.D8] = new Note(NoteValue.F_SHARP, STARTING_OCTAVE);
            keysToNotes[AdaptiveKey.D9] = new Note(NoteValue.G_SHARP, STARTING_OCTAVE);
            keysToNotes[AdaptiveKey.D0] = new Note(NoteValue.A_SHARP, STARTING_OCTAVE);
        }

        public TimePair TimePairFromKey(AdaptiveKey key)
        {
            switch (key)
            {
                case AdaptiveKey.S:
                    return new TimePair(2, 2);
                case AdaptiveKey.X:
                    return new TimePair(3, 2);
                case AdaptiveKey.D:
                    return new TimePair(4, 4);
                case AdaptiveKey.C:
                    return new TimePair(2, 4);
                case AdaptiveKey.F:
                    return new TimePair(3, 4);
                case AdaptiveKey.V:
                    return new TimePair(5, 4);
                case AdaptiveKey.G:
                    return new TimePair(3, 8);
                case AdaptiveKey.B:
                    return new TimePair(5, 8);
                case AdaptiveKey.H:
                    return new TimePair(6, 8);
                case AdaptiveKey.N:
                    return new TimePair(7, 8);
                case AdaptiveKey.J:
                    return new TimePair(9, 8);
                case AdaptiveKey.M:
                    return new TimePair(12, 8);
                default:
                    return new TimePair(4, 4);
            }
        }

        public ClefType ClefFromKey(AdaptiveKey key)
        {
            switch (key)
            {
                case AdaptiveKey.C:
                    return ClefType.TREBLE;
                case AdaptiveKey.V:
                    return ClefType.ALTO;
                case AdaptiveKey.B:
                    return ClefType.BASS;
                case AdaptiveKey.N:
                    return ClefType.PERCUSSION;
                case AdaptiveKey.M:
                    return ClefType.TABLATURE;
                default:
                    return ClefType.TREBLE;
            }
        }

        public int DotsFromKey(AdaptiveKey key)
        {
            switch (key)
            {
                case AdaptiveKey.Comma:
                    return 1;
                case AdaptiveKey.Period:
                    return 2;
                default:
                    return 0;
            }
        }

        public int DurationFromKey(AdaptiveKey key)
        {
            switch (key)
            {
                case AdaptiveKey.Z:
                    return 32;
                case AdaptiveKey.X:
                    return 16;
                case AdaptiveKey.C:
                    return 8;
                case AdaptiveKey.V:
                    return 4;
                case AdaptiveKey.B:
                    return 2;
                case AdaptiveKey.N:
                    return 1;
                default:
                    return 4;
            }
        }

        public LWMode ModeFromKey(AdaptiveKey key, Dictionary<AdaptiveKey, bool> keysSelectedTable, bool notesSelectedIsEmpty)
        {
            if (KeyShowsANote(key))
            {
                if (!notesSelectedIsEmpty)
                {
                    return LWMode.NOTE;
                }
                else
                {
                    return LWMode.NONE;
                }
            }
            else if (key == REST_KEY)
            {
                if (KeyIsSelected(key, keysSelectedTable))
                {
                    return LWMode.REST;
                }
                else
                {
                    return LWMode.NONE;
                }
            }
            else if (key == TIME_SIGNATURE_KEY)
            {
                if (KeyIsSelected(key, keysSelectedTable))
                {
                    return LWMode.TIME_SIGNATURE;
                }
                else
                {
                    return LWMode.NONE;
                }
            }
            else if (key == CLEF_KEY)
            {
                if (KeyIsSelected(key, keysSelectedTable))
                {
                    return LWMode.CLEF;
                }
                else
                {
                    return LWMode.NONE;
                }
            }
            else if (key == AdaptiveKey.Space)
            {
                return LWMode.NONE;
            }
            else
            {
                return LWMode.STAY_THE_SAME;
            }
        }

        public Keyboard GetKeyboard()
        {
            return keyboard;
        }

        public bool KeyShowsANote(AdaptiveKey key)
        {
            return keysToNotes.ContainsKey(key);
        }

        private bool KeyIsSelected(AdaptiveKey key, Dictionary<AdaptiveKey, bool> keysSelectedTable)
        {
            if (keysSelectedTable.ContainsKey(key))
            {
                return keysSelectedTable[key];
            }

            return false;
        }

        public Note GetNoteForKey(AdaptiveKey key)
        {
            if (keysToNotes.ContainsKey(key))
            {
                return keysToNotes[key];
            }

            return null;
        }

        public AdaptiveKey GetKeyForNote(Note note)
        {
            foreach (KeyValuePair<AdaptiveKey, Note> kvp in keysToNotes)
            {
                if (note.Equals(kvp.Value))
                    return kvp.Key;
            }

            return AdaptiveKey.None;
        }

        public void ShiftNotesUpOctave()
        {
            ShiftNotesOctaves(1);
        }

        public void ShiftNotesDownOctave()
        {
            ShiftNotesOctaves(-1);
        }

        void ShiftNotesOctaves(int numOctaves)
        {
            // Shift all the notes right by one
            if (keysToNotes.ContainsKey(AdaptiveKey.MinusSign))
                keysToNotes[AdaptiveKey.MinusSign].octave += numOctaves;
            if (keysToNotes.ContainsKey(AdaptiveKey.D0))
                keysToNotes[AdaptiveKey.D0].octave += numOctaves;
            if (keysToNotes.ContainsKey(AdaptiveKey.D9))
                keysToNotes[AdaptiveKey.D9].octave += numOctaves;
            if (keysToNotes.ContainsKey(AdaptiveKey.D8))
                keysToNotes[AdaptiveKey.D8].octave += numOctaves;
            if (keysToNotes.ContainsKey(AdaptiveKey.D7))
                keysToNotes[AdaptiveKey.D7].octave += numOctaves;
            if (keysToNotes.ContainsKey(AdaptiveKey.D6))
                keysToNotes[AdaptiveKey.D6].octave += numOctaves;
            if (keysToNotes.ContainsKey(AdaptiveKey.D5))
                keysToNotes[AdaptiveKey.D5].octave += numOctaves;
            if (keysToNotes.ContainsKey(AdaptiveKey.D4))
                keysToNotes[AdaptiveKey.D4].octave += numOctaves;
            if (keysToNotes.ContainsKey(AdaptiveKey.D3))
                keysToNotes[AdaptiveKey.D3].octave += numOctaves;
            if (keysToNotes.ContainsKey(AdaptiveKey.D2))
                keysToNotes[AdaptiveKey.D2].octave += numOctaves;

            keysToNotes[AdaptiveKey.P].octave += numOctaves;
            keysToNotes[AdaptiveKey.O].octave += numOctaves;
            keysToNotes[AdaptiveKey.I].octave += numOctaves;
            keysToNotes[AdaptiveKey.U].octave += numOctaves;
            keysToNotes[AdaptiveKey.Y].octave += numOctaves;
            keysToNotes[AdaptiveKey.T].octave += numOctaves;
            keysToNotes[AdaptiveKey.R].octave += numOctaves;
            keysToNotes[AdaptiveKey.E].octave += numOctaves;
            keysToNotes[AdaptiveKey.W].octave += numOctaves;
        }

        public void ShiftNotesUpOne()
        {
            Dictionary<AdaptiveKey, Note> newDict = new Dictionary<AdaptiveKey, Note>();

            // Shift all the notes right by one
            if (keysToNotes.ContainsKey(AdaptiveKey.D0))
                newDict[AdaptiveKey.MinusSign] = keysToNotes[AdaptiveKey.D0];
            if (keysToNotes.ContainsKey(AdaptiveKey.D9))
                newDict[AdaptiveKey.D0] = keysToNotes[AdaptiveKey.D9];
            if (keysToNotes.ContainsKey(AdaptiveKey.D8))
                newDict[AdaptiveKey.D9] = keysToNotes[AdaptiveKey.D8];
            if (keysToNotes.ContainsKey(AdaptiveKey.D7))
                newDict[AdaptiveKey.D8] = keysToNotes[AdaptiveKey.D7];
            if (keysToNotes.ContainsKey(AdaptiveKey.D6))
                newDict[AdaptiveKey.D7] = keysToNotes[AdaptiveKey.D6];
            if (keysToNotes.ContainsKey(AdaptiveKey.D5))
                newDict[AdaptiveKey.D6] = keysToNotes[AdaptiveKey.D5];
            if (keysToNotes.ContainsKey(AdaptiveKey.D4))
                newDict[AdaptiveKey.D5] = keysToNotes[AdaptiveKey.D4];
            if (keysToNotes.ContainsKey(AdaptiveKey.D3))
                newDict[AdaptiveKey.D4] = keysToNotes[AdaptiveKey.D3];
            if (keysToNotes.ContainsKey(AdaptiveKey.D2))
                newDict[AdaptiveKey.D3] = keysToNotes[AdaptiveKey.D2];

            newDict[AdaptiveKey.P] = keysToNotes[AdaptiveKey.O];
            newDict[AdaptiveKey.O] = keysToNotes[AdaptiveKey.I];
            newDict[AdaptiveKey.I] = keysToNotes[AdaptiveKey.U];
            newDict[AdaptiveKey.U] = keysToNotes[AdaptiveKey.Y];
            newDict[AdaptiveKey.Y] = keysToNotes[AdaptiveKey.T];
            newDict[AdaptiveKey.T] = keysToNotes[AdaptiveKey.R];
            newDict[AdaptiveKey.R] = keysToNotes[AdaptiveKey.E];
            newDict[AdaptiveKey.E] = keysToNotes[AdaptiveKey.W];
            
            // Put in the leftmost notes
            if (newDict.ContainsKey(AdaptiveKey.D9))
            {
                newDict[AdaptiveKey.D2] = new Note(newDict[AdaptiveKey.D9]);
                newDict[AdaptiveKey.D2].octave -= 1;
            }

            newDict[AdaptiveKey.W] = new Note(newDict[AdaptiveKey.O]);
            newDict[AdaptiveKey.W].octave -= 1;

            keysToNotes = newDict;
        }

        public void ShiftNotesDownOne()
        {
            Dictionary<AdaptiveKey, Note> newDict = new Dictionary<AdaptiveKey, Note>();
            
            // Shift all the notes left by one
            newDict[AdaptiveKey.O] = keysToNotes[AdaptiveKey.P];
            newDict[AdaptiveKey.I] = keysToNotes[AdaptiveKey.O];
            newDict[AdaptiveKey.U] = keysToNotes[AdaptiveKey.I];
            newDict[AdaptiveKey.Y] = keysToNotes[AdaptiveKey.U];
            newDict[AdaptiveKey.T] = keysToNotes[AdaptiveKey.Y];
            newDict[AdaptiveKey.R] = keysToNotes[AdaptiveKey.T];
            newDict[AdaptiveKey.E] = keysToNotes[AdaptiveKey.R];
            newDict[AdaptiveKey.W] = keysToNotes[AdaptiveKey.E];

            if (keysToNotes.ContainsKey(AdaptiveKey.MinusSign))
                newDict[AdaptiveKey.D0] = keysToNotes[AdaptiveKey.MinusSign];
            if (keysToNotes.ContainsKey(AdaptiveKey.D0))
                newDict[AdaptiveKey.D9] = keysToNotes[AdaptiveKey.D0];
            if (keysToNotes.ContainsKey(AdaptiveKey.D9))
                newDict[AdaptiveKey.D8] = keysToNotes[AdaptiveKey.D9];
            if (keysToNotes.ContainsKey(AdaptiveKey.D8))
                newDict[AdaptiveKey.D7] = keysToNotes[AdaptiveKey.D8];
            if (keysToNotes.ContainsKey(AdaptiveKey.D7))
                newDict[AdaptiveKey.D6] = keysToNotes[AdaptiveKey.D7];
            if (keysToNotes.ContainsKey(AdaptiveKey.D6))
                newDict[AdaptiveKey.D5] = keysToNotes[AdaptiveKey.D6];
            if (keysToNotes.ContainsKey(AdaptiveKey.D5))
                newDict[AdaptiveKey.D4] = keysToNotes[AdaptiveKey.D5];
            if (keysToNotes.ContainsKey(AdaptiveKey.D4))
                newDict[AdaptiveKey.D3] = keysToNotes[AdaptiveKey.D4];
            if (keysToNotes.ContainsKey(AdaptiveKey.D3))
                newDict[AdaptiveKey.D2] = keysToNotes[AdaptiveKey.D3];

            // Put in the leftmost notes
            if (newDict.ContainsKey(AdaptiveKey.D4))
            {
                newDict[AdaptiveKey.MinusSign] = new Note(newDict[AdaptiveKey.D4]);
                newDict[AdaptiveKey.MinusSign].octave += 1;
            }

            newDict[AdaptiveKey.P] = new Note(newDict[AdaptiveKey.E]);
            newDict[AdaptiveKey.P].octave += 1;

            keysToNotes = newDict;
        }

        public String GetStringForNote(Note note)
        {
            String s = "";
            switch (note.value)
            {
                case NoteValue.A_SHARP:
                    s = "A#";
                    break;
                case NoteValue.A:
                    s = "A";
                    break;
                case NoteValue.B:
                    s = "B";
                    break;
                case NoteValue.C_SHARP:
                    s = "C#";
                    break;
                case NoteValue.C:
                    s = "C";
                    break;
                case NoteValue.D_SHARP:
                    s = "D#";
                    break;
                case NoteValue.D:
                    s = "D";
                    break;
                case NoteValue.E:
                    s = "E";
                    break;
                case NoteValue.F_SHARP:
                    s = "F#";
                    break;
                case NoteValue.F:
                    s = "F";
                    break;
                case NoteValue.G_SHARP:
                    s = "G#";
                    break;
                case NoteValue.G:
                    s = "G";
                    break;
            }

            s = s + String.Format("{0:}", note.octave);

            return s;
        }
    }
}
