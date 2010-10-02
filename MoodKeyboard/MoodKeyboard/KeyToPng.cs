using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LWEvent;
using System.Diagnostics;

namespace MoodKeyboard
{
    class KeyToPng
    {
        public LPScore score;
        public int imageVersion = 0;

        public KeyToPng()
        {
            score = new LPScore();
        }

        public void HandleKey(LWEventData eventData)
        {
             
            /* add event data */
            score.addEventData(eventData);

            /* print the lilypond file */
            Console.WriteLine("===============================================");
            Console.WriteLine();
            Console.WriteLine(score.LPSymbol());
            Console.WriteLine();
            Console.WriteLine("===============================================");

            /* create the png */
            TextWriter tw = new StreamWriter("in.ly");
            tw.Write(score.LPSymbol());
            tw.Flush();
            tw.Close();
            //ExecuteCommand("lilypond -fpng -o ../MoodKeyboardContext/Images/out in.ly", 5000);
            
            ExecuteCommand("lilypond -fpng -o C:/Users/Walter/Desktop/tmp/out" + (imageVersion + 1) + " in.ly", 5000);
            Console.WriteLine("lilypond -fpng -o C:/Users/Walter/Desktop/tmp/out" + (imageVersion + 1) + " in.ly");
            /* send updates back to keyboard */

            /* and don't forget the image :) */

            imageVersion++;


        }

        public static int ExecuteCommand(string Command, int Timeout)
        {
            int ExitCode;
            ProcessStartInfo ProcessInfo;
            Process Process;

            ProcessInfo = new ProcessStartInfo("cmd.exe", "/C " + Command);
            ProcessInfo.CreateNoWindow = true;
            ProcessInfo.UseShellExecute = false;
            Process = Process.Start(ProcessInfo);
            Process.WaitForExit(Timeout);
            ExitCode = Process.ExitCode;
            Process.Close();

            return ExitCode;
        }
    }

    interface LPObject
    {
        String LPSymbol();
    }
    class LPScore : LPObject
    {
        private List<LPSlice> score;
        private int position = 0;

        /* store defaults */
        public static int defaultDuration = 1;

        /* store (de)crescendo toggle state */
        public static LPCrescendo crescendoState = LPCrescendo.none;

        public LPScore()
        {
            score = new List<LPSlice>();
            score.Add(new LPSlice());
        }

        public string LPSymbol()
        {
            String scoreAsString = "% Specify the paper size\n"
                + "\\paper {\n"
                + "#(define dump-extents #t)\n"
                + "indent = 0\\mm\n"
                + "line-width = 160\\mm\n"
                + "force-assignment = #\"\"\n"
                + "line-width = #(- line-width (* mm  3.000000))\n"
                + "}\n"
                + "% Version info\n"
                + "\\version \"2.12.3\"\n"
                + "% Header information\n"
                + "\\header {\n"
                + "title=\"Title\"\n"
                + "composer=\"Composer\"\n"
                + "}\n"
                + "% Layout information (not sure what this does)\n"
                + "\\layout {\n"
                + "\\context { \\Score\n"
                + "}\n"
                + "}\n"
                + "% Not entirely sure how this works, but it seems to tie notes over bar lines correctly\n"
                + "\\layout {\n"
                + "\\context {\n"
                + "\\Voice\n"
                + "\\remove \"Note_heads_engraver\"\n"
                + "\\consists \"Completion_heads_engraver\"\n"
                + "}\n"
                + "}\n"
                + "% Specify the first instrument (in theory we can have multiple?)\n"
                + "InstrumentOne = {\n"
                + "\\clef \"treble\"\n"
                + "\\key c \\major\n"
                + "\\time 4/4\n";

            foreach (LPSlice slice in score)
            {
                scoreAsString += slice.LPSymbol() + "\n";
            }

            scoreAsString += "}\n"
                + "% Create a staff in which to put the instruments (must come after defintion of instruments)\n"
                + "\\new Staff <<\n"
                + "\\context Staff <<\n"
                + "\\context Voice = \"InstrumentOne\" { \\InstrumentOne }\n"
                + ">>\n"
                + ">>\n";

            return scoreAsString;

        }

        public void addEventData(LWEventData eventData)
        {
            if (eventData.eventType == LWKeyType.SPACE)
            {
               
                if (position + 1 >= score.Count)
                {
                    score.Add(new LPSlice());
                }

                position++;
            }

            if (position >= 0 && position < score.Count)
            {
                score.ElementAt(position).addEventData(eventData);
            }
            else
            {
                Console.WriteLine("Error: score position is invalid.");
            }
        }

        public byte[] currentSliceCereal()
        {
            return score.ElementAt(position).Serialize();
        }
    }
    class LPSlice : LPObject
    {

        private List<LPNote> notes;
        private LPRest rest;
        private LPDynamic dynamic;
        private LPCrescendo crescendo = LPCrescendo.none;

        public LPSlice()
        {
            notes = new List<LPNote>();
        }

        public String LPSymbol()
        {
            /* Notes */
            String symbols = "";

            if (notes.Count > 0)
            {
                symbols += "<";
                foreach (LPNote n in notes)
                {
                    symbols += n.LPSymbol() + " ";
                }
                symbols += ">";
                symbols += notes.ElementAt(0).duration;
            }
            else if (rest != null)
            {
                symbols += rest.LPSymbol();
            }

            if (dynamic != null)
            {
                symbols += " " + dynamic.LPSymbol() + " ";
            }

            return symbols;
        }

        public Boolean addEventData(LWEventData eventData)
        {
            switch (eventData.eventType)
            {
                /* Rules:
                 * 1) If this note already exists, remove it from the list.
                 * 2) If it doesn't exist, add it.
                 * 3) If a rest exists, remove the rest.
                 * */
                case LWKeyType.NOTE:
                    Note note = eventData.key as Note;
                    LPNote lpNote = new LPNote(note.value, note.octave, LPScore.defaultDuration);
                    if (notes.Contains(lpNote))
                    {
                        notes.Remove(lpNote);
                    }
                    else
                    {
                        notes.Add(lpNote);
                    }

                    if (rest != null)
                    {
                        rest = null;
                    }

                    return true;

                /* Rules:
                 * 1) If a rest already exists, remove it.
                 * 2) If not, add it.
                 * 3) If note(s) already exist, remove them.
                 * */
                case LWKeyType.REST:
                    LPRest lpRest = new LPRest(LPScore.defaultDuration);
                    if (rest != null)
                    {
                        rest = null;
                    }
                    else
                    {
                        rest = lpRest;
                    }

                    if (notes.Count > 0)
                    {
                        notes.Clear();
                    }

                    return true;
                /* Rules:
                 * 1) change note and rest durations accordingly.
                 * */
                case LWKeyType.INVERSE_DURATION:
                    InverseDuration inverseDuration = eventData.key as InverseDuration;

                    /* update the actual notes/rests for lilypond */
                    if (rest != null)
                    {
                        rest.duration = inverseDuration.duration;
                    }

                    foreach (LPNote n in notes)
                    {
                        n.duration = inverseDuration.duration;
                    }


                    /* and set this as the default duration */
                    LPScore.defaultDuration = inverseDuration.duration;

                    return true;
                /* Rules:
                 * 1) if no dynamic, add it
                 * 2) else, if its equal, remove it
                 * 3) else, change it
                 * */
                case LWKeyType.DYNAMIC:
                    Dynamic d = eventData.key as Dynamic;
                    LPDynamic lpD = new LPDynamic(d.dynamicValue);
                    if (dynamic == null)
                    {
                        dynamic = lpD;
                    }
                    else if (dynamic.Equals(lpD))
                    {
                        dynamic = null;
                    }
                    else
                    {
                        dynamic = lpD;
                    }
                    return true;
                case LWKeyType.CRESCENDO:
                    /* if there is currently no (de)crescendo going on, life is easy */
                    if (LPScore.crescendoState == LPCrescendo.none)
                    {
                        this.crescendo = LPCrescendo.crescendoOpen;
                        LPScore.crescendoState = LPCrescendo.crescendoEnabled;
                    }
                    else if (LPScore.crescendoState == LPCrescendo.crescendoEnabled)
                    {
                        this.crescendo = LPCrescendo.crescendoClose;
                        LPScore.crescendoState = LPCrescendo.none;
                    }
                    else if (LPScore.crescendoState == LPCrescendo.decrescendoEnabled)
                    {

                    }
                    return true;
                case LWKeyType.DECRESCENDO:

                    return true;
                default:
                    return false;
            }
        }

        public byte[] Serialize()
        {
            LWKeyMap map = new LWKeyMap();

            /* add duration! */
            if (this.notes != null && this.rest != null)
                map.Add(LWKeyType.INVERSE_DURATION, new InverseDuration(LPScore.defaultDuration));

            /* if there are notes, add them all! */
            if (this.notes != null)
            {
                foreach (LPNote n in this.notes)
                {
                    map.Add(LWKeyType.NOTE, new Note(n.value, n.octave));
                }
            }
            /* or add a rest yo */
            else if (this.rest != null)
            {
                map.Add(LWKeyType.REST, new Rest());
            }

            return map.Serialize();
        }
    }
    class LPNote : LPObject
    {
        public NoteValue value;
        public int octave;
        public int duration;

        public LPNote(NoteValue value, int octave, int duration)
        {
            this.value = value;
            this.octave = octave;
            this.duration = duration;
        }

        public String LPSymbol()
        {
            // which note?
            String symbol = LPNote.NoteValueToString(this.value);

            // now, get the octave symbol (3 has no modifiers, 4+ has a ', and 2- has a ,)
            if (this.octave >= 4)
            {
                for (int i = 4; i <= this.octave; i++)
                {
                    symbol += "'";
                }
            }
            else if (this.octave <= 2)
            {
                for (int i = 2; i >= this.octave; i--)
                {
                    symbol += ",";
                }
            }

            return symbol;
        }

        private static String NoteValueToString(NoteValue value)
        {
            switch (value)
            {
                case NoteValue.A:
                    return "a";
                case NoteValue.A_SHARP:
                    return "ais";
                case NoteValue.B:
                    return "b";
                case NoteValue.C:
                    return "c";
                case NoteValue.C_SHARP:
                    return "cis";
                case NoteValue.D:
                    return "d";
                case NoteValue.D_SHARP:
                    return "dis";
                case NoteValue.E:
                    return "e";
                case NoteValue.F:
                    return "f";
                case NoteValue.F_SHARP:
                    return "fis";
                case NoteValue.G:
                    return "g";
                case NoteValue.G_SHARP:
                    return "gis";
                default:
                    return "error";
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else if (obj.GetType() != this.GetType())
            {
                return base.Equals(obj);
            }
            else
            {
                LPNote n = (LPNote)obj;
                return (this.value == n.value && this.octave == n.octave && this.duration == n.duration);
            }
        }
    }
    class LPRest : LPObject
    {
        public int duration;

        public LPRest(int duration)
        {
            this.duration = duration;
        }
        public string LPSymbol()
        {
            return "r" + duration;
        }
    }
    class LPDynamic : LPObject
    {
        public DynamicValue dynamic;

        public LPDynamic(DynamicValue dynamic)
        {
            this.dynamic = dynamic;
        }

        public String LPSymbol()
        {
            switch (dynamic)
            {
                case DynamicValue.PPP:
                    return "\\ppp";
                case DynamicValue.PP:
                    return "\\pp";
                case DynamicValue.P:
                    return "\\p";
                case DynamicValue.MP:
                    return "\\mp";
                case DynamicValue.MF:
                    return "\\mf";
                case DynamicValue.F:
                    return "\\f";
                case DynamicValue.FF:
                    return "\\ff";
                case DynamicValue.FFF:
                    return "\\fff";
                default:
                    return "\\ffff";
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            else if (obj.GetType() != this.GetType())
            {
                return base.Equals(obj);
            }
            else
            {
                LPDynamic d = (LPDynamic)obj;
                return (this.dynamic == d.dynamic);
            }
        }
    }
    enum LPCrescendo
    {
        crescendoOpen, crescendoClose, decrescendoOpen, decrescendoClose, none, crescendoEnabled, decrescendoEnabled
    }
}