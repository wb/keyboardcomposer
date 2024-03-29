﻿using System;

namespace LWContextCommunication
{
    public abstract class LWKey
    {
        public virtual String Serialize() { return ""; }
    };

    public class Note : LWKey
    {
        public NoteValue value;
        public int octave;

        /*
         * Main constructor. Additional constructors should call this constructor.
         * */
        public Note(NoteValue value, int octave)
        {
            this.value = value;
            this.octave = octave;
        }

        public Note()
            : this(NoteValue.A, 5)
        {
        }

        public Note(Note note)
            : this(note.value, note.octave)
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
            return octave * 100 + (int)value;
        }

        public override String Serialize()
        {
            String s = String.Format("{0:G},{1:G}", (int)value, octave);
            return s;
        }
    };

    public class Rest : LWKey { };

    public class TimeSignature : LWKey { };

    public class Clef : LWKey { };

    public class Space : LWKey { };

    public class Delete : LWKey { };

    public class Dots : LWKey
    {
        public int dots;

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

        public override string Serialize()
        {
            String s = String.Format("{0:G}", duration);
            return s;
        }
    }

    public class None : LWKey { };

    public class ClefSpecific : LWKey
    {
        public ClefType clef;

        public ClefSpecific(ClefType clef)
        {
            this.clef = clef;
        }

        public override string Serialize()
        {
            String s = String.Format("{0:G}", (int)clef);
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

        public bool EqualTimePair(TimePair tp)
        {
            return (tp.over == over && tp.under == under);
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

    public class ArrowLeft : LWKey { } ;

    public class ArrowRight : LWKey { } ;

}