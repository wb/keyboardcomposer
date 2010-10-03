using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using LWContextCommunication;
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

        public bool HandleKey(LWKeyEvent eventData)
        {
             
            /* add event data */
            return true | score.addEventData(eventData); /* make it return true everytime for now */

        }

        public void UpdateImage()
        {
            /* create the png */
            TextWriter tw = new StreamWriter("in.ly");
            tw.Write(score.LPSymbol());
            tw.Flush();
            tw.Close();
            ExecuteCommand("lilypond -dpreview -fpng -o C:/tmp/out" + (imageVersion + 1) + " in.ly", 5000);
            Console.WriteLine("lilypond -dpreview -fpng -o C:/tmp/out" + (imageVersion + 1) + " in.ly");
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

        public String getCurrentImageURI()
        {
            if (score.isEmpty())
            {
                return "C:/Users/Walter/Documents/KeyboardComposer/MoodKeyboard/MoodKeyboardContext/Images/blankScore.png";
            }
            else
            {
                return "C:/tmp/out" + this.imageVersion + ".png";
            }
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

        /* store slur toggle state */
        public static LPSlur slurState = LPSlur.slurNone;

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

            for (int i = 0; i < score.Count; i++ )
            {
                LPSlice slice = score.ElementAt(i);

                if (i == position)
                {
                    scoreAsString += "\\override NoteHead #'color = #magenta\n";
                }
                scoreAsString += slice.LPSymbol() + "\n";
                if (i == position)
                {
                    scoreAsString += "\\override NoteHead #'color = #black\n";
                }
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

        public bool addEventData(LWKeyEvent eventData)
        {
            bool updateImage = false;

            int beforePosition = position;


            /**
             * Process the control keys (arrows + space) 
             * */
            if (eventData.keyType == LWKeyType.ARROW_LEFT)
            {
                if (position > 0)
                {
                    position--;
                    updateImage = true;
                }
            }
            else if (eventData.keyType == LWKeyType.ARROW_RIGHT)
            {
                if (position < score.Count - 1)
                {
                    position++;
                    updateImage = true;
                }
            }
            else if (eventData.keyType == LWKeyType.SPACE)
            {
                /* if the current slice is empty, ignore the space */
                if (currentSlice().isEmpty())
                {
                    Console.WriteLine("Ignoring space because current slice is empty.");
                    return false; /* and of course, don't update image */
                }
               
                if (position + 1 >= score.Count)
                {
                    score.Add(new LPSlice());
                }

                position++;

                updateImage = true;
            }
            else if (eventData.keyType == LWKeyType.DELETE)
            {
                this.deleteCurrent();
                updateImage = true;

            }
            else if (eventData.keyType == LWKeyType.SLUR)
            {
                List<LPNote> notes = score.ElementAt(position).notes;

                if (notes.Count > 0)
                {

                    bool deleteEnding = false;

                    foreach (LPNote n in notes)
                    {
                        /* start a slur */
                        if (LPScore.slurState == LPSlur.slurNone && n.openSlur == LPSlur.slurNone)
                        {
                            n.openSlur = LPSlur.slurOpen;
                        }

                        /* delete a slur */
                        if (LPScore.slurState == LPSlur.slurEnabled && n.openSlur == LPSlur.slurOpen)
                        {
                            n.openSlur = LPSlur.slurNone;

                            /* also delete matching ending */
                            deleteEnding = true;
                        }

                        if (LPScore.slurState == LPSlur.slurEnabled && n.openSlur == LPSlur.slurNone)
                        {
                            n.closeSlur = LPSlur.slurClose;
                        }
                    }

                    if (deleteEnding)
                    {
                        /* first, find the ending (that is, the first with a closing slur) */
                        for (int i = position + 1; i < score.Count; i++)
                        {
                            List<LPNote> theNotes = score.ElementAt(i).notes;

                            if (theNotes.Count > 0 && theNotes.ElementAt(0).closeSlur == LPSlur.slurClose)
                            {
                                foreach (LPNote theNote in theNotes)
                                {
                                    theNote.closeSlur = LPSlur.slurNone;
                                }

                                break;
                            }
                        }
                    }

                    LPNote firstNote = notes.ElementAt(0);

                    if (firstNote.openSlur == LPSlur.slurOpen)
                    {
                        LPScore.slurState = LPSlur.slurEnabled;
                    }
                    else
                    {
                        LPScore.slurState = LPSlur.slurNone;
                    }

                }
            }

            if (beforePosition != position)
            {
                /* check to see if a slur is open */
                for (int i = position; i >= 0; i--)
                {
                    List<LPNote> theNotes = score.ElementAt(i).notes;

                    if (theNotes != null && theNotes.Count > 0)
                    {
                        if (theNotes.ElementAt(0).openSlur == LPSlur.slurOpen)
                        {
                            LPScore.slurState = LPSlur.slurEnabled;
                            break;
                        }
                        else if (theNotes.ElementAt(0).closeSlur == LPSlur.slurClose)
                        {
                            LPScore.slurState = LPSlur.slurNone;
                            break;
                        }
                    }
                }
            }

            /**
             * Add the actual data
             * */
            if (position >= 0 && position < score.Count)
            {
                score.ElementAt(position).addEventData(eventData);

                if ((eventData.keyType == LWKeyType.NOTE || eventData.keyType == LWKeyType.REST) && score.ElementAt(position).isEmpty())
                {
                    this.deleteCurrent();
                }
            }
            else
            {
                Console.WriteLine("Error: score position is invalid.");
            }

            return updateImage;
        }

        private void deleteCurrent()
        {
            /* remove the current position from the array */
            score.RemoveAt(position);
            Console.WriteLine("Removing note at position " + position);

            /* make sure we are pointing to something valid */
            if (position >= score.Count)
            {
                position = score.Count - 1;
                Console.WriteLine("Updated position to be at " + position);
            }

            if (position == -1)
            {
                position = 0;
            }

            /* if there is nothing left, add a blank one */
            if (score.Count == 0)
            {
                score.Add(new LPSlice());
                Console.WriteLine("Whoops! Can't have an empty score.  Adding a new blank measure.");
            }

        }

        private LPSlice currentSlice()
        {
            return score.ElementAt(position);
        }

        public byte[] currentSliceCereal()
        {
            return score.ElementAt(position).Serialize();
        }

        public Boolean isEmpty()
        {
            return (score.Count == 0 || (score.Count == 1 && score.ElementAt(0).isEmpty()));
        }
    }
    class LPSlice : LPObject
    {

        public List<LPNote> notes;
        public LPRest rest;
        public LPDynamic dynamic;
        public LPCrescendo crescendo = LPCrescendo.none;

        public LPSlice()
        {
            notes = new List<LPNote>();
        }

        public bool isEmpty()
        {
            return (notes.Count == 0 && rest == null);
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
                symbols += notes.ElementAt(0).dotsToString();

                if (notes.ElementAt(0).openSlur == LPSlur.slurOpen)
                {
                    symbols += "(";
                }

                if (notes.ElementAt(0).closeSlur == LPSlur.slurClose)
                {
                    symbols += ")";
                }
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

        public Boolean addEventData(LWKeyEvent eventData)
        {
            switch (eventData.keyType)
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

                case LWKeyType.DOTS:
                    Dots dots = eventData.key as Dots;
                    foreach(LPNote n in notes)
                    {
                        n.setDots(dots.dots);
                    }
                    if (rest != null)
                    {
                        rest.setDots(dots.dots);
                    }
                    return true;
                default:
                    return false;
            }
        }

        public byte[] Serialize()
        {
            LWKeyMap map = new LWKeyMap();

            /* add duration! */
            if ((this.notes != null && this.notes.Count > 0) || this.rest != null)
            {
                LWKey key = new InverseDuration(LPScore.defaultDuration);
                LWKeyType type = LWKeyType.INVERSE_DURATION;
                map.Add(key, type);
            }

            /* if there are notes, add them all! */
            if (this.notes != null && notes.Count > 0)
            {
                /* get number of dots */
                int dots = notes.ElementAt(0).dots;
                if (dots > 0) { map.Add(new Dots(dots), LWKeyType.DOTS); }
                foreach (LPNote n in this.notes)
                {
                    map.Add(new Note(n.value, n.octave), LWKeyType.NOTE);
                }
            }
            /* or add a rest yo */
            else if (this.rest != null)
            {
                int dots = rest.dots;
                if (dots > 0) { map.Add(new Dots(dots), LWKeyType.DOTS); }
                map.Add(new Rest(), LWKeyType.REST);
            }

            /* add a dynamic */
            if (this.dynamic != null)
            {
                map.Add(new Dynamic(dynamic.dynamic), LWKeyType.DYNAMIC);
            }

            /* and slur */

            /* but first we have to check if slur should be enabled */

            if (LPScore.slurState == LPSlur.slurEnabled)
            {
                map.Add(new Slur(), LWKeyType.SLUR);
            }

            if (map.Count == 0)
            {
                map.Add(new None(), LWKeyType.NOT_IMPLEMENTED);
            }

            return map.Serialize();
        }
    }
    class LPNote : LPObject
    {
        public NoteValue value;
        public int octave;
        public int duration;
        public int dots;
        public LPSlur openSlur = LPSlur.slurNone;
        public LPSlur closeSlur = LPSlur.slurNone;

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

        public string dotsToString()
        {
            string dots = "";

            for (int i = 0; i < this.dots; i++ )
            {
                dots += ".";
            }

            return dots;
        }

        public void setDots(int dotsCount)
        {
            if (dotsCount < 0 || dotsCount > 2)
            {
                return;
            }

            if (dotsCount == this.dots)
            {
                this.dots = 0;
            }
            else
            {
                this.dots = dotsCount;
            }
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
        public int dots;

        public LPRest(int duration)
        {
            this.duration = duration;
        }
        public string LPSymbol()
        {
            return "r" + duration;
        }

        public void setDots(int dotsCount)
        {
            if (dotsCount < 0 || dotsCount > 2)
            {
                return;
            }

            if (dotsCount == this.dots)
            {
                this.dots = 0;
            }
            else
            {
                this.dots = dotsCount;
            }
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
    enum LPSlur
    {
        slurOpen, slurClose, slurEnabled, slurNone
    }
}