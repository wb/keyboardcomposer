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
using LWContextCommunication;
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
        public const AdaptiveKey CRESCENDO_KEY = AdaptiveKey.F10;
        public const AdaptiveKey DECRESCENDO_KEY = AdaptiveKey.F11;

        private Dictionary<AdaptiveKey, Note> keysToNotes = new Dictionary<AdaptiveKey,Note>();
        private Dictionary<AdaptiveKey, ClefSpecific> clefs = new Dictionary<AdaptiveKey,ClefSpecific>();
        private Dictionary<AdaptiveKey, Dots> dots = new Dictionary<AdaptiveKey,Dots>();
        private Dictionary<AdaptiveKey, InverseDuration> durations = new Dictionary<AdaptiveKey,InverseDuration>();
        private Dictionary<AdaptiveKey, TimeSignatureSpecific> timeSignatures = new Dictionary<AdaptiveKey,TimeSignatureSpecific>();
        private Keyboard keyboard;

        public KeyTranslator(Keyboard keyboard)
        {
            if (keyboard == null)
                MessageBox.Show("Don't pass the key translator a null keyboard please!");

            this.keyboard = keyboard;

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

            clefs[AdaptiveKey.C] = new ClefSpecific(ClefType.TREBLE);
            clefs[AdaptiveKey.V] = new ClefSpecific(ClefType.ALTO);
            clefs[AdaptiveKey.B] = new ClefSpecific(ClefType.BASS);
            clefs[AdaptiveKey.N] = new ClefSpecific(ClefType.PERCUSSION);
            clefs[AdaptiveKey.M] = new ClefSpecific(ClefType.TABLATURE);

            dots[AdaptiveKey.Comma] = new Dots(1);
            dots[AdaptiveKey.Period] = new Dots(2);
        
            durations[AdaptiveKey.Z] = new InverseDuration(32);
            durations[AdaptiveKey.X] = new InverseDuration(16);
            durations[AdaptiveKey.C] = new InverseDuration(8);
            durations[AdaptiveKey.V] = new InverseDuration(4);
            durations[AdaptiveKey.B] = new InverseDuration(2);
            durations[AdaptiveKey.N] = new InverseDuration(1);

            timeSignatures[AdaptiveKey.S] = new TimeSignatureSpecific(new TimePair(2, 2));
            timeSignatures[AdaptiveKey.X] = new TimeSignatureSpecific(new TimePair(3, 2));
            timeSignatures[AdaptiveKey.D] = new TimeSignatureSpecific(new TimePair(4, 4));
            timeSignatures[AdaptiveKey.C] = new TimeSignatureSpecific(new TimePair(2, 4));
            timeSignatures[AdaptiveKey.F] = new TimeSignatureSpecific(new TimePair(3, 4));
            timeSignatures[AdaptiveKey.V] = new TimeSignatureSpecific(new TimePair(5, 4));
            timeSignatures[AdaptiveKey.G] = new TimeSignatureSpecific(new TimePair(3, 8));
            timeSignatures[AdaptiveKey.B] = new TimeSignatureSpecific(new TimePair(5, 8));
            timeSignatures[AdaptiveKey.H] = new TimeSignatureSpecific(new TimePair(6, 8));
            timeSignatures[AdaptiveKey.N] = new TimeSignatureSpecific(new TimePair(7, 8));
            timeSignatures[AdaptiveKey.J] = new TimeSignatureSpecific(new TimePair(9, 8));
            timeSignatures[AdaptiveKey.M] = new TimeSignatureSpecific(new TimePair(12, 8));
        }

        public AdaptiveKey KeyFromLWKey(LWKeyType type, LWKey key)
        {
            switch (type)
            {
                case LWKeyType.CLEF:
                    return CLEF_KEY;
                case LWKeyType.CLEF_SPECIFIC:
                    ClefSpecific cs = key as ClefSpecific;
                    foreach (KeyValuePair<AdaptiveKey, ClefSpecific> kvp in clefs)
                    {
                        if (kvp.Value.clef == cs.clef)
                            return kvp.Key;
                    }
                    return AdaptiveKey.None;
                case LWKeyType.CRESCENDO:
                    return CRESCENDO_KEY;
                case LWKeyType.DECRESCENDO:
                    return DECRESCENDO_KEY;
                case LWKeyType.DOTS:
                    Dots dot = key as Dots;
                    foreach (KeyValuePair<AdaptiveKey, Dots> kvp in dots)
                    {
                        if (kvp.Value.dots == dot.dots)
                            return kvp.Key;
                    }
                    return AdaptiveKey.None;
                case LWKeyType.DYNAMIC:
                    return DYNAMIC_KEY;
                case LWKeyType.INVERSE_DURATION:
                    InverseDuration iv = key as InverseDuration;
                    foreach (KeyValuePair<AdaptiveKey, InverseDuration> kvp in durations)
                    {
                        if (kvp.Value.duration == iv.duration)
                            return kvp.Key;
                    }
                    return AdaptiveKey.None;
                case LWKeyType.NOT_IMPLEMENTED:
                    return AdaptiveKey.None;
                case LWKeyType.NOTE:
                    Note note = key as Note;
                    foreach (KeyValuePair<AdaptiveKey, Note> kvp in keysToNotes)
                    {
                        if (note.Equals(kvp.Value))
                            return kvp.Key;
                    }
                    return AdaptiveKey.None;
                case LWKeyType.REST:
                    return REST_KEY;
                case LWKeyType.SLUR:
                    return SLUR_KEY;
                case LWKeyType.SPACE:
                    return AdaptiveKey.Space;
                case LWKeyType.TIME_SIGNATURE:
                    return TIME_SIGNATURE_KEY;
                case LWKeyType.TIME_SIGNATURE_SPECIFIC:
                    TimeSignatureSpecific tss = key as TimeSignatureSpecific;
                    foreach (KeyValuePair<AdaptiveKey, TimeSignatureSpecific> kvp in timeSignatures)
                    {
                        if (kvp.Value.timePair.EqualTimePair(tss.timePair))
                            return kvp.Key;
                    }
                    return AdaptiveKey.None;
                default:
                    return AdaptiveKey.None;
            }
        }

        public TimePair TimePairFromKey(AdaptiveKey key)
        {
            if (timeSignatures.ContainsKey(key))
            {
                return timeSignatures[key].timePair;
            }
            else
            {
                return new TimePair(-1, -1);
            }
        }

        public ClefType ClefFromKey(AdaptiveKey key)
        {
            if (clefs.ContainsKey(key))
            {
                return clefs[key].clef;
            }
            else
            {
                return ClefType.NONE;
            }
        }

        public AdaptiveKey KeyFromClefType(ClefType ct)
        {
            foreach (KeyValuePair<AdaptiveKey, ClefSpecific> kvp in clefs)
            {
                if (kvp.Value.clef == ct)
                {
                    return kvp.Key;
                }
            }
            return AdaptiveKey.None;
        }

        public int DotsFromKey(AdaptiveKey key)
        {
            if (dots.ContainsKey(key))
            {
                return dots[key].dots;
            }
            else
            {
                return 0;
            }
        }

        public int DurationFromKey(AdaptiveKey key)
        {
            if (durations.ContainsKey(key))
            {
                return durations[key].duration;
            }
            else
            {
                return -1;
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
