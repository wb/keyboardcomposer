using System;
using System.Runtime.Serialization;

namespace LWEvent
{
    public class LWEventData
    {
        public LWKey key;
        public LWKeyType eventType;

        public LWEventData()
        {
            key = new None();
            eventType = LWKeyType.NOT_IMPLEMENTED;
        }
        
        public LWEventData(LWKey key, LWKeyType et)
        {
            this.key = key;
            this.eventType = et;
        }

        public String Serialize()
        {
            String s = "";
            s = s + String.Format("{0:G};", (int)eventType);
            s = s + key.Serialize();

            return s;
        }

        public static LWEventData Deserialize(String s)
        {
            String[] parts = s.Split(';');

            LWEventData eventData = new LWEventData();
            eventData.eventType = (LWKeyType) Int32.Parse(parts[0]);

            // If you update these, be sure to update the serialize counterparts
            switch (eventData.eventType)
            {
                case LWKeyType.NOT_IMPLEMENTED:
                    eventData.key = new None();
                    break;
                case LWKeyType.NOTE:
                    String[] noteParts = parts[1].Split(',');
                    eventData.key = new Note((NoteValue) Int32.Parse(noteParts[0]), Int32.Parse(noteParts[1]));
                    break;
                case LWKeyType.SPACE:
                    eventData.key = new Space();
                    break;
                case LWKeyType.CLEF:
                    eventData.key = new Clef();
                    break;
                case LWKeyType.CLEF_SPECIFIC:
                    eventData.key = new ClefSpecific((ClefType) Int32.Parse(parts[1]));
                    break;
                case LWKeyType.DOTS:
                    eventData.key = new Dots(Int32.Parse(parts[1]));
                    break;
                case LWKeyType.DYNAMIC:
                    eventData.key = new Dynamic((DynamicValue)Int32.Parse(parts[1]));
                    break;
                case LWKeyType.INVERSE_DURATION:
                    eventData.key = new InverseDuration(Int32.Parse(parts[1]));
                    break;
                case LWKeyType.REST:
                    eventData.key = new Rest();
                    break;
                case LWKeyType.TIME_SIGNATURE:
                    eventData.key = new TimeSignature();
                    break;
                case LWKeyType.TIME_SIGNATURE_SPECIFIC:
                    String[] timeParts = parts[1].Split(',');
                    TimePair timePair = new TimePair(Int32.Parse(timeParts[0]), Int32.Parse(timeParts[1]));
                    eventData.key = new TimeSignatureSpecific(timePair);
                    break;
                case LWKeyType.SLUR:
                    eventData.key = new Slur();
                    break;
                case LWKeyType.CRESCENDO:
                    eventData.key = new Crescendo();
                    break;
                case LWKeyType.DECRESCENDO:
                    eventData.key = new Decrescendo();
                    break;
            }

            return eventData;
        }
    };

    public abstract class LWKey {
        public virtual String Serialize() { return ""; }
    };

    public class Note : LWKey
    {
        public NoteValue value;
        public int octave;
        public int duration;

        /*
         * Main constructor. Additional constructors should call this constructor.
         * */
        public Note(NoteValue value, int octave)
        {
            this.value = value;
            this.octave = octave;
            this.duration = 4;
        }

        public Note()
            :this(NoteValue.A, 5)
        { 
        }

        public Note(Note note)
            :this(note.value, note.octave)
        {
        }

        public override bool Equals(System.Object obj)
        {
            // If parameter is null return false.
            if (obj == null)
            {
                return false;
            }

            // If parameter cannot be cast to Point return false.
            Note p = obj as Note;
            if ((System.Object)p == null)
            {
                return false;
            }

            // Return true if the fields match:
            return (value == p.value) && (octave == p.octave);
        }

        public override int GetHashCode()
        {
            return octave * 100 + (int) value;
        }

        public override String Serialize()
        {
            String s = String.Format("{0:G},{1:G}", (int)value, octave);
            return s;
        }

        /*
         * This method is used to convert this note into the symbol used in Lilypond. 
         **/
        public String GetLilyPondSymbol()
        {
            // which note?
            String symbol = Note.NoteValueToString(this.value);

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

            // add duration
            symbol += this.duration;

            return symbol;
        }

        /*
         * This method is used to convert a NoteValue into a string.
         * */
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
    };

    public class Rest : LWKey { };

    public class TimeSignature : LWKey { };

    public class Clef : LWKey { };

    public class Space : LWKey { };

    public class Dots : LWKey
    {
        int dots;

        public Dots(int dots)
        {
            this.dots = dots;
        }

        public override string Serialize()
        {
            String s = String.Format("{0:G}", dots);
            return s;
        }
    }

    public class InverseDuration : LWKey
    {
        public int duration;

        public InverseDuration(int duration)
        {
            this.duration = duration;
        }

        public override string  Serialize()
        {
 	        String s = String.Format("{0:G}", duration);
            return s;
        }
    }

    public class None : LWKey { };

    public enum NoteValue
    {
        
        A = 0,
        A_SHARP,
        B,
        C,
        C_SHARP,
        D,
        D_SHARP,
        E,
        F,
        F_SHARP,
        G,
        G_SHARP
    };

    public enum ClefType
    {
        TREBLE,
        ALTO,
        BASS,
        PERCUSSION,
        TABLATURE
    }

    public class ClefSpecific : LWKey
    {
        public ClefType clef;

        public ClefSpecific(ClefType clef)
        {
            this.clef = clef;
        }

        public override string Serialize()
        {
            String s = String.Format("{0:G}", (int) clef);
            return s;
        }
    }

    public struct TimePair
    {
        public int over;
        public int under;

        public TimePair(int over, int under)
        {
            this.over = over;
            this.under = under;
        }
    }

    public class TimeSignatureSpecific : LWKey
    {
        public TimePair timePair;

        public TimeSignatureSpecific(TimePair timePair)
        {
            this.timePair.over = timePair.over;
            this.timePair.under = timePair.under;
        }

        public override string Serialize()
        {
            String s = String.Format("{0:G},{1:G}", timePair.over, timePair.under);
            return s;
        }
    }

    public class Dynamic : LWKey
    {
        public DynamicValue dynamicValue;

        public Dynamic(DynamicValue dv)
        {
            dynamicValue = dv;
        }

        public override string Serialize()
        {
            String s = String.Format("{0:G}", (int)dynamicValue);
            return s;
        }
    }

    public class Slur : LWKey { };

    public class Crescendo : LWKey { };

    public class Decrescendo : LWKey { } ;

    public enum DynamicValue
    {
        PPP,
        PP,
        P,
        MP,
        MF,
        F,
        FF,
        FFF
    }

    public enum LWKeyType
    { 
        NOTE,
        REST,
        TIME_SIGNATURE,
        CLEF,
        SPACE,
        INVERSE_DURATION,
        DOTS,
        CLEF_SPECIFIC,
        TIME_SIGNATURE_SPECIFIC,
        DYNAMIC,
        SLUR,
        CRESCENDO,
        DECRESCENDO,
        NOT_IMPLEMENTED
    };

    public enum LWMode
    {
        NOTE,
        REST,
        TIME_SIGNATURE,
        CLEF,
        NONE,
        STAY_THE_SAME
    };

    public enum LWMessageID
    {
        FROM_KEYBOARD,
        FROM_TOUCHPAD,
        FROM_APPLICATION
    };

};