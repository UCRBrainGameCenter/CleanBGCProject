using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConditionTools 
{
    public static string TranslateTime(float time)
    {
        string hourComponent = "";
        string minuteComponent = "";
        string secondComponent = "";

        int seconds = Mathf.Max(Mathf.FloorToInt(time), 0);
        int minutes = seconds / 60;
        seconds -= 60 * minutes;

        int hours = minutes / 60;
        minutes -= 60 * hours;

        bool addSpace = false;

        if (hours > 0)
        {
            hourComponent = $"{hours} Hour{(hours == 1 ? "" : "s")}";
            addSpace = true;
        }

        if (minutes > 0)
        {
            minuteComponent = $"{(addSpace ? " " : "")}{minutes} Minute{(minutes == 1 ? "" : "s")}";
            addSpace = true;
        }

        if (seconds > 0 || (hours == 0 && minutes == 0))
        {
            secondComponent = $"{(addSpace ? " " : "")}{seconds} Second{(seconds == 1 ? "" : "s")}";
        }

        return $"{hourComponent}{minuteComponent}{secondComponent}";
    }
}
