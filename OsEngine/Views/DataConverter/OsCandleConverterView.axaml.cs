/*
 * Your rights to use code governed by this license https://github.com/AlexWan/OsEngine/blob/master/LICENSE
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using OsEngine.Language;
using OsEngine.ViewModels.OsConverter;

namespace OsEngine.Views.OsConverter;

public partial class OsCandleConverterView : Window
{

    public OsCandleConverterView()
    {
        InitializeComponent();
        DataContext = new OsCandleConverterViewModel();
        // OsEngine.Layout.StickyBorders.Listen(this);
        // OsEngine.Layout.StartupLocation.Start_MouseInCentre(this);
        LabelOsa.Content = "V " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

        Label1.Content = OsLocalization.Converter.Label1;
        Label2.Content = OsLocalization.Converter.Label2;
        ButtonSetSource.Content = OsLocalization.Converter.Label3;
        ButtonSetExitFile.Content = OsLocalization.Converter.Label3;
        Label4.Header = OsLocalization.Converter.Label4;
        ButtonStart.Content = OsLocalization.Converter.Label5;
        // ComboBoxTimeFrameInitial.SelectionChanged += ChangeSourceTimeFrame;

        Activate();
        Focus();
    }

    private async void ButtonSetSource_Click(object sender, RoutedEventArgs e)
    {
        var file = await StorageProvider.OpenFilePickerAsync(new()
        {
            AllowMultiple = false,
        });

        if (file.Count >= 1)
        {
            Console.WriteLine(file[0]);
            TextBoxSource.Text = file[0].Path.AbsolutePath;
        }
        // try
        // {
        //     _candleConverter.SelectSourceFile();
        // }
        // catch (Exception ex)
        // {
        //     _candleConverter.SendNewLogMessage(ex.ToString(), Logging.LogMessageType.Error);
        // }
    }

    private async void ButtonSetExitFile_Click(object sender, RoutedEventArgs e)
    {
        var file = await StorageProvider.SaveFilePickerAsync(new()
        {

        });

        if (file != null)
        {
            TextBoxExit.Text = file.Path.AbsolutePath;
            // ((OsCandleConverterViewModel)DataContext).Save(
            //     TimeFrame.Sec1,
            //     TextBoxSource.Text,
            //     TextBoxExit.Text
            //     );
        }

        // try
        // {
        //     _candleConverter.CreateExitFile();
        // }
        // catch (Exception ex)
        // {
        //     _candleConverter.SendNewLogMessage(ex.ToString(),Logging.LogMessageType.Error);
        // }
    }

    // public void ChangeSourceTimeFrame(object sender, SelectionChangedEventArgs e)
    // {
    //     (DataContext as OsCandleConverterViewModel)
    //         .ChangeSourceTimeFrame((TimeFrame)ComboBoxTimeFrameInitial.SelectedItem);
    // }
}
