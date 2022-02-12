﻿using System;
using System.Windows;
using System.Windows.Media;
using System.Collections;
using Ecng.Collections;
using Ecng.Common;
using StockSharp.Algo.Candles;
using StockSharp.Algo.Commissions;
using StockSharp.Algo.Storages;
using StockSharp.Algo.Strategies;
using StockSharp.Algo.Testing;
using StockSharp.BusinessEntities;
using StockSharp.Configuration;
using StockSharp.Logging;
using StockSharp.Messages;
using StockSharp.Xaml;
using StockSharp.Xaml.Charting;
using System.Collections.Generic;
using StockSharp.Algo;
using StrategyMinimaMaxima.PriceAction;
using StrategyMinimaMaxima.PriceAction.Signal;
using System.Linq;

namespace StrategyMinimaMaxima
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int _report_temp_counter;
        private HistoryEmulationConnector _connector;
        private ChartCandleElement _candleElement;
        private ChartTradeElement _tradesElem;

        private CandleSeries _candleSeries_1h;
        private CandleSeries _candleSeries_15m;
        private CandleSeries _candleSeries_1m;

        private Security _security;
        private Portfolio _portfolio;
        private readonly LogManager _logManager;
        private ICIStrategy iciStrategy;
        private readonly string _pathHistory = @"C:\Storage";

        private ChartBandElement _pnl;
        private ChartBandElement _unrealizedPnL;
        private ChartBandElement _commissionCurve;

        public MainWindow()
        {
            InitializeComponent();

            _logManager = new LogManager();
            _logManager.Listeners.Add(new FileLogListener("log.txt"));
            _logManager.Listeners.Add(new GuiLogListener(Monitor));
            DatePickerBegin.SelectedDate = new DateTime(2021, 1, 1);
            DatePickerEnd.SelectedDate = new DateTime(2021, 1, 2);

            CandleSettingsEditor.Settings = new CandleSeries
            {
                CandleType = typeof(TimeFrameCandle),
                Arg = TimeSpan.FromMinutes(60),
            };
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            //.................initialize security...................
            _security = new Security
            {
                Id = "BTCUSDT_PERPETUAL@BNB",
                Code = "BTCUSDT",
                PriceStep = 0.01m,
                Board = ExchangeBoard.Binance,
                VolumeStep = 0.01M,
            };

            //.................initialize portfolio...................
            _portfolio = new Portfolio { Name = "test account", BeginValue = 1000 };
            var storageRegistry = new StorageRegistry
            {
                DefaultDrive = new LocalMarketDataDrive(_pathHistory),
            };

            //.................initialize connector...................
            _connector = new HistoryEmulationConnector(new[] { _security }, new[] { _portfolio })
            {
                HistoryMessageAdapter =
                {
                    StorageRegistry = storageRegistry,
                    StorageFormat = StorageFormats.Binary,
                    StartDate = DatePickerBegin.SelectedDate.Value.ChangeKind(DateTimeKind.Utc),
                    StopDate = DatePickerEnd.SelectedDate.Value.ChangeKind(DateTimeKind.Utc),
                },
                LogLevel = LogLevels.Info
            };

            //.................add connector to the log manager...................
            _logManager.Sources.Add(_connector);

            _candleSeries_15m = new CandleSeries(typeof(TimeFrameCandle), _security, TimeSpan.FromMinutes(15))
            {
                BuildCandlesMode = MarketDataBuildModes.Load
            };

            _candleSeries_1h = new CandleSeries(typeof(TimeFrameCandle), _security, TimeSpan.FromMinutes(60))
            {
                BuildCandlesMode = MarketDataBuildModes.Load
            };

            //.................initialize chart...................
            InitChart();

            //.................connector configurations and events handlers...................
            _connector.CandleSeriesProcessing += Connector_CandleSeriesProcessing;

            _connector.NewSecurity += Connector_NewSecurity;
            _connector.MarketDepthChanged += MarketDepthControl.UpdateDepth;
            _connector.NewOrder += OrderGrid.Orders.Add;
            _connector.OrderRegisterFailed += OrderGrid.AddRegistrationFail;

            iciStrategy = new ICIStrategy(_candleSeries_1h, _candleSeries_15m, long.Parse(txtProcessLimit.Text) + 1) // +1 is for TradingView
            {
                Security = _security,
                Connector = _connector,
                Portfolio = _portfolio,
            };

            //.................adding strategy to log manager...................
            _logManager.Sources.Add(iciStrategy);

            //.................setting strategy events...................
            iciStrategy.NewMyTrade += MyTradeGrid.Trades.Add;
            iciStrategy.NewMyTrade += Strategy_NewMyTrade;
            iciStrategy.PnLChanged += Strategy_PnLChanged;

            //.................setting strategy statistics to the gui...................
            StatisticParameterGrid.Parameters.AddRange(iciStrategy.StatisticManager.Parameters);

            //.................connecting connector...................
            _connector.Connect();

            txtReportLeft.Text = "";
            txtReportRight.Text = "";
            _report_temp_counter = 0;
        }
        private void InitChart()
        {
            //.................prepare candle chart...................
            Chart.ClearAreas();

            var area = new ChartArea();
            _candleElement = new ChartCandleElement();
            _tradesElem = new ChartTradeElement { FullTitle = "Trade" };

            Chart.AddArea(area);
            Chart.AddElement(area, _candleElement);
            Chart.AddElement(area, _tradesElem);

            //.................prepare EquityCurveChart...................
            _pnl = EquityCurveChart.CreateCurve("PNL", Colors.Green, ChartIndicatorDrawStyles.Area);
            _unrealizedPnL = EquityCurveChart.CreateCurve("unrealizedPnL", Colors.Black, ChartIndicatorDrawStyles.Line);
            _commissionCurve = EquityCurveChart.CreateCurve("commissionCurve", Colors.Red, ChartIndicatorDrawStyles.Line);
        }

        private void Strategy_PnLChanged()
        {
            var data = new ChartDrawData();
            data.Group(iciStrategy.CurrentTime)
                .Add(_pnl, iciStrategy.PnL)
                .Add(_unrealizedPnL, iciStrategy.PnLManager.UnrealizedPnL ?? 0)
                .Add(_commissionCurve, iciStrategy.Commission ?? 0);
            EquityCurveChart.Draw(data);
        }

        private void Strategy_NewMyTrade(MyTrade myTrade)
        {
            var data = new ChartDrawData();
            data.Group(myTrade.Trade.Time).Add(_tradesElem, myTrade);
            Chart.Draw(data);
        }

        private void Connector_NewSecurity(Security security)
        {
            iciStrategy.Processor.BullishICI += Processor_BullishICI;
            iciStrategy.Processor.BearishICI += Processor_BearishICI;
            iciStrategy.Processor.SignalPatternChanged += Processor_SignalPatternChanged;
            iciStrategy.Start();
            _connector.Start();
        }
        private void Connector_CandleSeriesProcessing(CandleSeries candleSeries, Candle candle)
        {
            if (((TimeFrameCandle)candle).TimeFrame == TimeSpan.FromMinutes(60))
            {
                Chart.Draw(_candleElement, candle);
            }
        }

        private void Processor_SignalPatternChanged(object? sender, ParentPatternEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                txtReportLeft.Text += $"{++_report_temp_counter}-Signal Pattern Changed: {e.ParentPatternType}" +
                $"{Environment.NewLine}";
            }));
        }

        private void Processor_BearishICI(object? sender, ParentPatternEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                txtReportRight.Text += $"{++_report_temp_counter}-{e.ParentPatternType}: " +
                $"{ e.ParentContainer.Candles.LastOrDefault()!.OpenTime:dd-MM-yyyy @ HH:mm}" +
                $"{Environment.NewLine}Slope = {Math.Abs(e.ImpulseSlope)}{Environment.NewLine}";
                txtPriceActionReport.ScrollToEnd();
            }));
        }

        private void Processor_BullishICI(object? sender, ParentPatternEventArgs e)
        {
            Dispatcher.Invoke(new Action(() =>
            {
                txtReportRight.Text += $"{++_report_temp_counter}-{e.ParentPatternType}: " +
                $"{ e.ParentContainer.Candles.LastOrDefault()!.OpenTime:dd-MM-yyyy @ HH:mm}" +
                $"{Environment.NewLine}Slope = {Math.Abs(e.ImpulseSlope)}{Environment.NewLine}";
                txtPriceActionReport.ScrollToEnd();
            }));
        }

        private void btnOpenMainClicked(object sender, RoutedEventArgs e)
        {
            MainWin win = new MainWin();
            win.Show();
        }

        private void btnGetLeg1Childs_Click(object sender, RoutedEventArgs e)
        {
            string report = ((ICIStrategy)iciStrategy).GetChildReport(LegStatus.Leg1, int.Parse(txtParentLevel.Text));
            txtPriceActionReport.Text = report != string.Empty ? report : "[NO DATA AVAILABLE]";
        }

        private void btnGetLeg2Childs_Click(object sender, RoutedEventArgs e)
        {
            string report = ((ICIStrategy)iciStrategy).GetChildReport(LegStatus.Leg2, int.Parse(txtParentLevel.Text));
            txtPriceActionReport.Text = report != string.Empty ? report : "[NO DATA AVAILABLE]"; 
        }

        private void btnGetLeg3Childs_Click(object sender, RoutedEventArgs e)
        {
            string report = ((ICIStrategy)iciStrategy).GetChildReport(LegStatus.Leg3, int.Parse(txtParentLevel.Text));
            txtPriceActionReport.Text = report != string.Empty ? report : "[NO DATA AVAILABLE]";
        }

        private void btnGetParentSwings_Click(object sender, RoutedEventArgs e)
        {
            string report = ((ICIStrategy)iciStrategy).GetParentReport();
            txtPriceActionReport.Text = report != string.Empty ? report : "[NO DATA AVAILABLE]";
        }

        private void btnGetLeg2ChildsCont_Click(object sender, RoutedEventArgs e)
        {
            string report = ((ICIStrategy)iciStrategy).GetChildReport(LegStatus.Leg2, int.Parse(txtParentLevel.Text), true);
            txtPriceActionReport.Text = report != string.Empty ? report : "[NO DATA AVAILABLE]";
        }

        private void btnGetLeg3ChildsCont_Click(object sender, RoutedEventArgs e)
        {
            string report = ((ICIStrategy)iciStrategy).GetChildReport(LegStatus.Leg3, int.Parse(txtParentLevel.Text), true);
            txtPriceActionReport.Text = report != string.Empty ? report : "[NO DATA AVAILABLE]";
        }
    }
}
