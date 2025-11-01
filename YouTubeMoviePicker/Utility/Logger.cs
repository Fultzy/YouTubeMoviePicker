using Microsoft.Windows.Themes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace YouTubeMoviePicker.Utility;

public static class Logger
{
    private static readonly ConcurrentQueue<(string message, string logName)> logQueue = new();
    private static readonly Task logTask;
    private static readonly CancellationTokenSource cancellationTokenSource = new();

    static Logger()
    {
        logTask = Task.Run(ProcessLogQueue, cancellationTokenSource.Token);
    }

    private static async Task ProcessLogQueue()
    {
        while (!cancellationTokenSource.Token.IsCancellationRequested)
        {
            while (logQueue.TryDequeue(out var logEntry))
            {
                WriteToFile(logEntry.message, logEntry.logName);
            }
            await Task.Delay(100); // Adjust delay as needed
        }
    }

    private static void EnqueueLog(string message, string logName, bool timeStamp)
    {
        if (timeStamp)
        {
            message = DateTime.Now + " - " + message;
        }
        else
        {
            message = "\n" + message;
        }

        logQueue.Enqueue((message, logName));
    }

    private static string WriteToFile(string message, string logName)
    {
        var logfolder = Path.Combine(Environment.CurrentDirectory, "Logs");
        var logPath = Path.Combine(logfolder, $"{logName}.log");

        if (!Directory.Exists(logfolder))
        {
            Directory.CreateDirectory(logfolder);
        }

        CheckLogFile(logPath); // Check if the log file is older than 7 days

        File.AppendAllText(logPath, message + Environment.NewLine);

        return message;
    }

    private static void CheckLogFile(string filepath)
    {
        if (!File.Exists(filepath))
        {
            Directory.CreateDirectory(filepath);
        }

        // If the log file size is larger than 10 MB, archive it
        var fileInfo = new FileInfo(filepath);
        if (fileInfo.Length > 10 * 1024 * 1024)
        {
            ArchiveLogFile(filepath);
        }
    }

    private static void ArchiveLogFile(string filepath)
    {
        var archivePath = Path.Combine(Environment.CurrentDirectory, "Logs/LogArchive");
        if (!Directory.Exists(archivePath))
        {
            Directory.CreateDirectory(archivePath);
        }

        var fileName = Path.GetFileName(filepath);
        var archiveFilePath = Path.Combine(archivePath, fileName);

        // Move the log file to the archive folder and overwrite if exists
        File.Move(filepath, archiveFilePath, true);
    }

    // Log methods
    public static void MainLog(string message, bool timeStamp = true)
    {
        EnqueueLog(message, "MainLog", timeStamp);
    }

    public static void DiscordLog(string message, bool timeStamp = true)
    {
        EnqueueLog(message, "DiscordLog", timeStamp);
    }

    public static void SlackLog(string message, bool timeStamp = true)
    {
        EnqueueLog(message, "SlackLog", timeStamp);
    }

    public static void OMdbLog(string message, bool timeStamp = true)
    {
        EnqueueLog(message, "OMdbLog", timeStamp);
    }

    public static void YouTubeLog(string message, bool timeStamp = true)
    {
        EnqueueLog(message, "YouTubeLog", timeStamp);
    }

    public static void ExceptionLog(string message, bool timeStamp = true)
    {
        EnqueueLog(message, "ExceptionLog", timeStamp);
    }

    internal static void TeamsLog(string message, bool timeStamp = true)
    {
        EnqueueLog(message, "TeamsLog", timeStamp);
    }
}
