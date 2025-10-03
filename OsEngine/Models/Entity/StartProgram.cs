/*
 * Your rights to use code governed by this license http://o-s-a.net/doc/license_simple_engine.pdf
 * Ваши права на использование кода регулируются данной лицензией http://o-s-a.net/doc/license_simple_engine.pdf
*/

using System;

namespace OsEngine.Models.Entity
{
    /// <summary>
    /// what program start the object
    /// </summary>
    [Flags]
    public enum StartProgram
    {
        /// <summary>
        /// tester
        /// </summary>
        IsTester = 1,

        /// <summary>
        /// optimizer
        /// </summary>
        IsOsOptimizer = 2,

        Testing = IsTester | IsOsOptimizer,

        /// <summary>
        /// trade terminal
        /// </summary>
        IsOsTrader = 4,

        /// <summary>
        /// data downloader
        /// </summary>
        IsOsData = 8,

        /// <summary>
        /// ticks to candles converter
        /// </summary>
        IsOsConverter = 16,
    }
}
