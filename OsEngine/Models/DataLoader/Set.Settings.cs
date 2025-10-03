using System;
using System.IO;
using OsEngine.ViewModels.Data;

namespace OsEngine.Models.Data;

public partial class SetViewModel
{
    public SecuritySettings BaseSettings { get; private set; }
    public object[] SecuritiesLoad { get; private set; }

    // public void Save()
    // {
    //     try
    //     {
    //         if (Name == "Set_")
    //         {
    //             return;
    //         }
    //
    //         if (!Directory.Exists("Data\\" + Name))
    //         {
    //             Directory.CreateDirectory("Data\\" + Name);
    //         }
    //         using StreamWriter writer = new("Data\\" + Name + @"\\Settings.txt", false);
    //         writer.WriteLine(BaseSettings.GetSaveStr());
    //
    //         for (int i = 0; SecuritiesLoad != null && i < SecuritiesLoad.Count; i++)
    //         {
    //             string security = SecuritiesLoad[i].GetSaveStr();
    //             writer.WriteLine(security);
    //         }
    //
    //         writer.Close();
    //     }
    //     catch (Exception)
    //     {
    //         // ignored
    //     }
    // }
    //
    // private void Load()
    // {
    //     if (!File.Exists("Data\\" + Name + @"\\Settings.txt"))
    //     {
    //         return;
    //     }
    //
    //     try
    //     {
    //
    //         if (SecuritiesLoad == null)
    //         {
    //             SecuritiesLoad = new List<SecurityToLoad>();
    //         }
    //
    //         using StreamReader reader = new("Data\\" + Name + @"\\Settings.txt");
    //         BaseSettings.Load(reader.ReadLine());
    //
    //         while (reader.EndOfStream == false)
    //         {
    //             SecuritiesLoad.Add(new SecurityToLoad());
    //             SecuritiesLoad[SecuritiesLoad.Count - 1].Load(reader.ReadLine());
    //             SecuritiesLoad[SecuritiesLoad.Count - 1].NewLogMessageEvent += SendNewLogMessage;
    //         }
    //
    //         reader.Close();
    //     }
    //     catch (Exception)
    //     {
    //         // ignored
    //     }
    // }
    //
    // public void Delete()
    // {
    //     _isDeleted = true;
    //
    //     if (File.Exists("Data\\" + Name + @"\\Settings.txt"))
    //     {
    //         File.Delete("Data\\" + Name + @"\\Settings.txt");
    //     }
    //
    //     if (Directory.Exists("Data\\" + Name))
    //     {
    //         try
    //         {
    //             DirectoryInfo info = new("Data\\" + Name);
    //             info.Delete(true);
    //         }
    //         catch (Exception)
    //         {
    //             // ignore
    //         }
    //     }
    //
    //     for (int i = 0; SecuritiesLoad != null && i < SecuritiesLoad.Count; i++)
    //     {
    //         SecuritiesLoad[i].Delete();
    //         SecuritiesLoad[i].NewLogMessageEvent -= NewLogMessageEvent;
    //     }
    //
    //     SecuritiesLoad?.Clear();
    // }
}
