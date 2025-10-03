using Avalonia.Data.Converters;
using OsEngine.Models.Market.Servers.Tester;

namespace OsEngine.Converters;

public static class RegimeToBoolConverter
{
    public static FuncValueConverter<TesterRegime, bool> IsActive { get; } =
        new(regime => regime != TesterRegime.NotActive);

    public static FuncValueConverter<TesterRegime, bool> IsPause { get; } =
        new(regime => regime == TesterRegime.Pause);

    // public static FuncValueConverter<TesterRegime, bool> IsPause { get; } =
    //     new(regime => regime == TesterRegime.Pause);
    //
    // public static FuncValueConverter<TesterRegime, bool> IsPause { get; } =
    //     new(regime => regime == TesterRegime.Pause);
}

