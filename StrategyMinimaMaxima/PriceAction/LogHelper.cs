using StockSharp.Algo.Candles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StrategyMinimaMaxima.PriceAction
{
    public static class LogHelper
    {
        public static bool WriteSwingList(List<PriceActionSwing>? swings, string filePath)
        {
            if (swings != null && swings.Count > 0)
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine("--------- Last Swing Information ---------");
                str.AppendLine($"Total Swing Count: {swings.Count}" +
                    $" - Swing Begining Candle SeqNum: {swings[0].Leg1.BeginElement.Candle.SeqNum}" +
                    $" - Swing Ending Candle SeqNum: {swings[swings.Count - 1].Leg3.EndElement.Candle.SeqNum}");
                foreach (var item in swings)
                {
                    str.AppendLine();
                    str.AppendLine("######################################################################");
                    str.AppendLine(
                        $" -- PatternType: {item.PatternType}" +
                        $" -- Impulse: {PriceActionHelper.GetImpulseType(item)} " +
                        $" -- Correction: {PriceActionHelper.GetCorrectionType(item)} ");
                    str.AppendLine("######################################################################");
                    str.AppendLine();
                    str.AppendLine($"**Leg1 Begin Element: {item.Leg1.BeginElement}{System.Environment.NewLine}" +
                        $"**Leg1 End Element: {item.Leg1.EndElement}");
                    str.AppendLine();
                    str.AppendLine($"**Leg2 Begin Element: {item.Leg2.BeginElement}{System.Environment.NewLine}" +
                        $"**Leg2 End Element: {item.Leg2.EndElement}");
                    str.AppendLine();
                    str.AppendLine($"**Leg3 Begin Element: {item.Leg3.BeginElement}{System.Environment.NewLine}" +
                        $"**Leg3 End Element: {item.Leg3.EndElement}");
                    str.AppendLine();
                }
                File.WriteAllText(filePath, str.ToString());
                return true;
            }
            return false;
        }
        public static bool WriteSwing(PriceActionSwing swing, string filePath)
        {
            if (swing != null)
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine();
                str.AppendLine("######################################################################");
                str.AppendLine(
                    $" -- PatternType: {swing.PatternType}" +
                    $" -- Impulse: {PriceActionHelper.GetImpulseType(swing)} " +
                    $" -- Correction: {PriceActionHelper.GetCorrectionType(swing)} ");
                str.AppendLine("######################################################################");
                str.AppendLine();
                str.AppendLine($"**Leg1 Begin Element: {swing.Leg1.BeginElement}{System.Environment.NewLine}" +
                    $"**Leg1 End Element: {swing.Leg1.EndElement}");
                str.AppendLine();
                str.AppendLine($"**Leg2 Begin Element: {swing.Leg2.BeginElement}{System.Environment.NewLine}" +
                    $"**Leg2 End Element: {swing.Leg2.EndElement}");
                str.AppendLine();
                str.AppendLine($"**Leg3 Begin Element: {swing.Leg3.BeginElement}{System.Environment.NewLine}" +
                    $"**Leg3 End Element: {swing.Leg3.EndElement}");
                str.AppendLine();
                File.WriteAllText(filePath, str.ToString());
                return true;
            }
            return false;
        }
        public static bool WriteCandleList(List<Candle> candle, string filePath)
        {
            if (candle != null)
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine("--------- Candles info ---------");
                foreach (var item in candle)
                {
                    str.AppendLine($"SeqNo {item.SeqNum} -- Time:{new DateTime(item.OpenTime.Ticks).ToShortTimeString()}" +
                        $" -- Values: Open:{item.OpenPrice} ," +
                        $" Close:{item.ClosePrice} High:{item.HighPrice} Low:{item.LowPrice}");
                }
                File.WriteAllText(filePath, str.ToString());
                return true;
            }
            return false;
        }
        public static bool WriteCandle(TimeFrameCandle candle, string filePath)
        {
            if (candle != null)
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine("--------- Candle Info ---------");
                str.AppendLine($"SeqNo {candle.SeqNum} -- Type: -- Values: Open:{candle.OpenPrice} ," +
                    $" Close:{candle.ClosePrice} High:{candle.HighPrice} Low:{candle.LowPrice}");
                File.WriteAllText(filePath, str.ToString());
                return true;
            }
            return false;
        }
        public static bool WritePosition(PriceActionPosition position, string filePath)
        {
            if (position != null)
            {
                StringBuilder str = new StringBuilder();
                str.AppendLine("--------- position info ---------");
                str.AppendLine($"Direction: {position.Direction}");
                str.AppendLine($"Entry Price: {position.EntryPrice}");
                str.AppendLine($"Stop Loss: {position.StopLossPrice}");
                str.AppendLine($"Take Profit: {position.TakeProfitPrice}");

                File.WriteAllText(filePath, str.ToString());
                return true;
            }
            return false;
        }
    }
}
