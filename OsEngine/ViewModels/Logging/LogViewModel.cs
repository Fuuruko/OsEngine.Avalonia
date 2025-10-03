using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Controls;
using OsEngine.Language;
using OsEngine.Models.Entity;
using OsEngine.Models.Logging;
using OsEngine.Views.Logging;

namespace OsEngine.ViewModels.Logging;
/* private static readonly List<LogViewModel> _logs = [];
   private static readonly Dictionary<string, LogViewModel> _dlogs = [];
   private static readonly Task _watcher = Task.Run(SaveLogsToFiles);
   private static readonly Lock _lock = new();


   private ConcurrentQueue<LogMessage> _messageQueue = new();

// TODO: Make static or delete.
//       It seems that it's already use current culture for time.
// private CultureInfo _currentCulture;
private readonly string _uniqueName;
private readonly StartProgram _startProgram;
private string FilePath => $"Engine{Path.DirectorySeparatorChar}Log{Path.DirectorySeparatorChar}{_uniqueName}Log_{DateTime.Now:yyyy_MM_dd}.txt";

private ILog _logClass;
private bool _isDelete = false;

public static ObservableCollection<LogMessage> ErrorMessages { get; } = [];
public ObservableCollection<LogMessage> LogMessages { get; } = [];
public MessageSender MessageSender { get; }

public LogViewModel()
{
_uniqueName = "DEFAULT_";
// _currentCulture = OsLocalization.CurCulture;
}

public LogViewModel(string uniqueName, StartProgram startProgram)
{
_uniqueName = uniqueName;
_startProgram = startProgram;
// _currentCulture = OsLocalization.CurCulture;

if (_startProgram == StartProgram.IsOsOptimizer) { return; }

// CreateGrid();
_logs.Add(this);
Console.WriteLine($"logs {_logs.Count} {_logs.Capacity}");
// AddToLogsToCheck(this);

if (_startProgram == StartProgram.IsOsTrader)
{
MessageSender = new MessageSender(uniqueName, _startProgram);
// TryLoadLog();
}
}

public static LogViewModel GetLogByName(string name, StartProgram startProgram)
{
_dlogs.TryGetValue(name, out LogViewModel log);
return log ?? new(name, startProgram);
}

~LogViewModel()
{
Console.WriteLine("DISPOSE HAPPEN");
_logs.Remove(this);
}


public void Listen(ILog master)
{
_logClass = master;
_logClass.LogRecieved += ProcessMessage;
}

public static async void SaveLogsToFiles()
{
while (MainWindow.ProccesIsWorked)
{
try
{
    await Task.Delay(2000);

    Console.WriteLine(_logs.Count);
    lock(_lock)
    {
        // for (int i = 0; i < Math.Min(_logs.Count, _logs.Capacity); i++)// не потокобезопасная работа с LogsToCheck приводит к Capacity<Count
        foreach (LogViewModel log in _logs)
        {
            // if (LogsToCheck[i] == null)
            // {
            //     continue;
            // }
            Console.WriteLine(log);
            log.TrySaveLog();
            // LogsToCheck[i].TryPaintLog();
        }
    }
}
catch (Exception error)
{
    MessageBox.Show(error.ToString());
}
}
}

public void TrySaveLog()
{
    if (_startProgram == StartProgram.IsOsOptimizer
            || _messageQueue.IsEmpty)
    { return; }

    try
    {
        using StreamWriter writer = new(FilePath, true);
        Console.WriteLine($"File created or append. {_messageQueue.IsEmpty}");

        while (_messageQueue.IsEmpty == false)
        {
            if (_messageQueue.TryDequeue(out LogMessage message))
            {
                string mess = $"{message.Time.ToLocalTime()};{message.Type};{message.Message};";
                Console.WriteLine(mess);

                writer.WriteLine(mess);
            }
        }
    }
    catch (Exception error)
    {
        MessageBox.Show(error.ToString());
    }
}

// NOTE: Change how logs saved before
// public void TryLoadLog()
// {
//     try
//     {
//         if (!File.Exists(FilePath)) { return; }
//
//         using FileStream fs = new(FilePath, FileMode.Open, FileAccess.Read);
//         using StreamReader reader = new(fs);
//
//         long fileLength = fs.Length;
//         var position = fileLength - 1;
//
//         List<string> messages = [];
//
//         while (reader.EndOfStream == false)
//         {
//             messages.Add(reader.ReadLine());
//         }
//
//         if (messages.Count == 0)
//         {
//             return;
//         }
//
//         int startInd = messages.Count - 10;
//
//         if (startInd < 0)
//         {
//             startInd = 0;
//         }
//
//         for (int i = startInd; i < messages.Count; i++)
//         {
//             string msg = messages[i];
//
//             string[] msgArray = msg.Split(';');
//
//             if (msgArray.Length != 4) { continue; }
//
//             LogMessage message;
//
//             try
//             {
//                 var time = Convert.ToDateTime(msgArray[0]);
//                 var logType = Enum.Parse<LogMessageType>(msgArray[1]);
//                 message = new LogMessage(msgArray[0], logType) { Time = time };
//             }
//             catch
//             {
//                 continue;
//             }
//             LogMessages.Add(message);
//         }
//     }
//     catch (Exception error)
//     {
//         MessageBox.Show(error.ToString());
//     }
// }

/// <summary>
/// incoming message
/// входящее сообщение
/// </summary>
public void ProcessMessage(string message, LogMessageType type)
{
    if (_isDelete || !MainWindow.ProccesIsWorked) { return; }

    if (type == LogMessageType.Error && ErrorMessages.Count <= 500)
    {
        if (ErrorMessages.Count == 500)
        {
            message = "To much ERRORS. Error log shut down. Clear to turn on again";
        }
        LogMessage log = new(message, type);
        ErrorMessages.Add(log);
        lock(_lock)
        {
            _messageQueue.Enqueue(log);
        }
        LogErrorUi.ShowErrorLog();
    }
    else if (_startProgram != StartProgram.IsOsOptimizer)
    {
        LogMessage messageLog = new(message, type);
        LogMessages.Add(messageLog);

        lock(_lock)
        {
            _messageQueue.Enqueue(messageLog);
        }

        if (_messageQueue.Count > 500)
        {
            _messageQueue.TryDequeue(out LogMessage _);
        }

        MessageSender?.AddNewMessage(messageLog);
    }
}

/// <summary>
/// delete the object and clear all files associated with it
/// удалить объект и очистить все файлы связанные с ним
/// </summary>
// NOTE: Maybe move to deconstuctor
public void Delete()
{
    _isDelete = true;

    if (_startProgram != StartProgram.IsOsOptimizer)
    {
        lock (_lock) { _logs.Remove(this); }

        if (File.Exists(FilePath)) { File.Delete(FilePath); }
    }

    _messageQueue.Clear();
    LogMessages.Clear();
    _logClass.LogRecieved -= ProcessMessage;

} */

// NOTE: How manage _logs?
public class LogViewModel : BaseViewModel
{
    public LogViewModel(Logs logs)
    {
        Logs = logs;
    }

    public LogViewModel() {}

    public Logs Logs { get; }
    public static ObservableCollection<Log> ErrorMessages { get; } = Logs.ErrorMessages;

    public void ShowErrorLog() => LogErrorUi.ShowErrorLog();

    public void ShowInFileManager()
    {
        try
        {
            string FilePath = Logs.FilePath;
            Console.WriteLine($"{FilePath} {File.Exists(FilePath)}");
            if (!File.Exists(Logs.FilePath)) { return; }

            // string argument = "/select, \"" + path + "\"";
            // System.Diagnostics.Process.Start("explorer.exe", argument);

            // Normalize the path for cross-platform compatibility
            // path = path.Replace('/', Path.DirectorySeparatorChar)
            //     .Replace('\\', Path.DirectorySeparatorChar);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Process.Start(new ProcessStartInfo
                        {
                        FileName = "explorer.exe",
                        Arguments = FilePath,
                        UseShellExecute = true
                        });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                // For Linux, you can use xdg-open or a specific file manager like nautilus
                Process.Start(new ProcessStartInfo
                        {
                        FileName = "xdg-open",
                        Arguments = FilePath,
                        UseShellExecute = true
                        });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // For macOS, use open command
                Process.Start(new ProcessStartInfo
                        {
                        FileName = "open",
                        Arguments = FilePath,
                        UseShellExecute = true
                        });
            }
            else
            {
                throw new PlatformNotSupportedException("This platform is not supported.");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString());
        }
    }

    public async void ClearErrorLog(Window win)
    {
        bool confirm = await MessageBox.ConfirmDialog(OsLocalization.Logging.Label30, win);

        if (confirm) { Logs.ErrorMessages.Clear(); }
    }

    public void OpenMessageSender() => Logs.MessageSender?.ShowDialog();
}
