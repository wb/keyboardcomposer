using System;

namespace LWEvent
{
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
        TABLATURE,
        NONE
    };

    public enum DynamicValue
    {
        PPP,
        PP,
        P,
        MP,
        MF,
        F,
        FF,
        FFF,
        NONE
    };

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