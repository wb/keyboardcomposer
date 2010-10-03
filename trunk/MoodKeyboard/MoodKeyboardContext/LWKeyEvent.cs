using System;
using System.Collections.Generic;
using System.Text;

namespace LWContextCommunication
{
    public class LWKeyMap : Dictionary<LWKey, LWKeyType>
    {
        public const String DELIMITER = "###";
        public LWKeyMap()
        {
        }

        public byte[] Serialize()
        {
            String s = "";

            foreach (KeyValuePair<LWKey, LWKeyType> kvp in this)
            {
                LWKeyEvent ed = new LWKeyEvent(kvp.Key, kvp.Value);
                s = s + ed.Serialize() + DELIMITER;
            }

            Encoding encoder = Encoding.UTF8;
            byte[] data = encoder.GetBytes(s);

            return data;
        }

        public static LWKeyMap Deserialize(byte[] data)
        {
            Encoding enc = Encoding.UTF8;
            String s = enc.GetString(data, 0, data.Length);

            LWKeyMap map = new LWKeyMap();

            if (s.Length == 0)
            {
                return map;
            }

            String[] delimiters = new String[1];
            delimiters[0] = DELIMITER;
            String[] items = s.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < items.Length; i++)
            {
                LWKeyEvent keyEvent = LWKeyEvent.Deserialize(items[i]);
                map[keyEvent.key] = keyEvent.keyType;
            }

            return map;
        }
    }

    public class LWKeyEvent
    {
        public LWKey key;
        public LWKeyType keyType;

        public LWKeyEvent()
        {
            key = new None();
            keyType = LWKeyType.NOT_IMPLEMENTED;
        }

        public LWKeyEvent(LWKey key, LWKeyType kt)
        {
            this.key = key;
            this.keyType = kt;
        }

        public String Serialize()
        {
            String s = "";
            s = s + String.Format("{0:G};", (int)keyType);
            s = s + key.Serialize();

            return s;
        }

        public static LWKeyEvent Deserialize(byte[] data)
        {
            Encoding enc = Encoding.UTF8;
            String s = enc.GetString(data, 0, data.Length);

            return LWKeyEvent.Deserialize(s);
        }

        public static LWKeyEvent Deserialize(String s)
        {
            String[] parts = s.Split(';');

            LWKeyEvent keyEvent = new LWKeyEvent();
            keyEvent.keyType = (LWKeyType)Int32.Parse(parts[0]);

            // If you update these, be sure to update the serialize counterparts
            switch (keyEvent.keyType)
            {
                case LWKeyType.NOT_IMPLEMENTED:
                    keyEvent.key = new None();
                    break;
                case LWKeyType.NOTE:
                    String[] noteParts = parts[1].Split(',');
                    keyEvent.key = new Note((NoteValue)Int32.Parse(noteParts[0]), Int32.Parse(noteParts[1]));
                    break;
                case LWKeyType.SPACE:
                    keyEvent.key = new Space();
                    break;
                case LWKeyType.CLEF:
                    keyEvent.key = new Clef();
                    break;
                case LWKeyType.CLEF_SPECIFIC:
                    keyEvent.key = new ClefSpecific((ClefType)Int32.Parse(parts[1]));
                    break;
                case LWKeyType.DOTS:
                    keyEvent.key = new Dots(Int32.Parse(parts[1]));
                    break;
                case LWKeyType.DYNAMIC:
                    keyEvent.key = new Dynamic((DynamicValue)Int32.Parse(parts[1]));
                    break;
                case LWKeyType.INVERSE_DURATION:
                    keyEvent.key = new InverseDuration(Int32.Parse(parts[1]));
                    break;
                case LWKeyType.REST:
                    keyEvent.key = new Rest();
                    break;
                case LWKeyType.TIME_SIGNATURE:
                    keyEvent.key = new TimeSignature();
                    break;
                case LWKeyType.TIME_SIGNATURE_SPECIFIC:
                    String[] timeParts = parts[1].Split(',');
                    TimePair timePair = new TimePair(Int32.Parse(timeParts[0]), Int32.Parse(timeParts[1]));
                    keyEvent.key = new TimeSignatureSpecific(timePair);
                    break;
                case LWKeyType.SLUR:
                    keyEvent.key = new Slur();
                    break;
                case LWKeyType.CRESCENDO:
                    keyEvent.key = new Crescendo();
                    break;
                case LWKeyType.DECRESCENDO:
                    keyEvent.key = new Decrescendo();
                    break;
                case LWKeyType.ARROW_LEFT:
                    keyEvent.key = new ArrowLeft();
                    break;
                case LWKeyType.ARROW_RIGHT:
                    keyEvent.key = new ArrowRight();
                    break;
                case LWKeyType.DELETE:
                    keyEvent.key = new Delete();
                    break;
            }

            return keyEvent;
        }
    };
}