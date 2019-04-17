using UnityEngine;
using System.IO;
using System;
using BGC.IO;
using BGC.Users;

public class LocalLogger
{
    private readonly string currentLogFile = "";
    private string FileTimeStamp => DateTime.Now.ToString("yy_MM_dd_HH_mm_ss");
    private string Header =>
        $"%{ApplicationData.Name} Version {ApplicationData.Version}, " +
        $"Device: asdf, " +
        $"User: {PlayerData.UserName}, " +
        $"Session: {PlayerData.GetInt("SessionNumber")}";


    public LocalLogger(string directory, string fileName)
    {
        currentLogFile = DataManagement.PathForDataFile(directory, $"{fileName}_{FileTimeStamp}.txt");
        PushLines(Header, "");
    }


    /// <summary>
    /// Append line to the log file
    /// </summary>
    public void PushLine(string line)
    {
        //WriteLine(newLogLine) to the LogFile
        using (StreamWriter logWriter = File.AppendText(currentLogFile))
        {
            logWriter.WriteLine(line);
        }
    }

    /// <summary>
    /// Append lines to the log file
    /// </summary>
    public void PushLines(params string[] lines)
    {
        using (StreamWriter logWriter = File.AppendText(currentLogFile))
        {
            foreach (string line in lines)
            {
                logWriter.WriteLine(line);
            }
        }
    }
}
