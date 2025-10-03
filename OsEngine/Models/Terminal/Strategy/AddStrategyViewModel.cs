using System;
using System.Collections.ObjectModel;
using System.IO;

namespace OsEngine.ViewModels.Terminal.Strategy;

public class AddStrategyViewModel : BaseViewModel
{
   public static ObservableCollection<Strategy> Strategies { get; } = [];

   public static ObservableCollection<Strategy> GetStrategies()
   {
       var s = Path.DirectorySeparatorChar;
       var folders = Directory.GetDirectories(
               $"{AppDomain.CurrentDomain.BaseDirectory}{s}Custom{s}Strategies{s}"
               );
       foreach (var f in folders)
       {
           Console.WriteLine(f);
       }

       return null;
   }


   public class Strategy
   {
        public string Name;
        public Type Type;
   }
}

