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
            eventData.eventType = (LWKeyType)Int32.Parse(parts[0]);

            // If you update these, be sure to update the serialize counterparts
            switch (eventData.eventType)
            {
                case LWKeyType.NOT_IMPLEMENTED:
                    eventData.key = new None();
                    break;
                case LWKeyType.NOTE:
                    String[] noteParts = parts[1].Split(',');
                    eventData.key = new Note((NoteValue)Int32.Parse(noteParts[0]), Int32.Parse(noteParts[1]));
                    break;
                case LWKeyType.SPACE:
                    eventData.key = new Space();
                    break;
                case LWKeyType.CLEF:
                    eventData.key = new Clef();
                    break;
                case LWKeyType.CLEF_SPECIFIC:
                    eventData.key = new ClefSpecific((ClefType)Int32.Parse(parts[1]));
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
}