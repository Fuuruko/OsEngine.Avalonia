using System;
using System.Collections.Generic;

using OsEngine.Language;
using OsEngine.Models.Utils;

namespace OsEngine.ViewModels.Utils;

public partial class SettingsViewModel : BaseViewModel
{
    public string[] TimeFormats { get; } = ["H:mm:ss", "h:mm:ss tt"];
    public string[] DateFormats { get; } = ["dd.MM.yyyy", "M/d/yyyy"];
    public List<OsLocalization.OsLocalType> Localizations { get; } = OsLocalization.GetExistLocalizationTypes();
}
