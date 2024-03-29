﻿using System;
using System.Windows;
using System.Windows.Media;
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

namespace StrategyMinimaMaxima
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HistoryEmulationConnector _connector;
        private ChartCandleElement _candleElement;
        private ChartTradeElement _tradesElem;
        private CandleSeries _1hCandleSeries;
        private CandleSeries _15mCandleSeries;
        private Security _security;
        private Portfolio _portfolio;
        private readonly LogManager _logManager;
        private Strategy _strategy;
        private readonly string _pathHistory = @"D:\StockSharp\StockSharpData\Storage"; //Paths.HistoryDataPath;
        private ChartBandElement _pnl;
        private ChartBandElement _unrealizedPnL;
        private ChartBandElement _commissionCurve;

        public MainWindow()
        {
            InitializeComponent();

            _logManager = new LogManager();
            _logManager.Listeners.Add(new FileLogListener("log.txt"));
            _logManager.Listeners.Add(new GuiLogListener(Monitor));

            DatePickerBegin.SelectedDate = new DateTime(2021, 11, 18);
            DatePickerEnd.SelectedDate = new DateTime(2021, 11, 19);

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
                Board = ExchangeBoard.Binance
            };

            //.................initialize portfolio...................
            _portfolio = new Portfolio { Name = "test account", BeginValue = 1000000 };
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
                LogLevel = LogLevels.Info,
            };

            //.................add connector to the log manager...................
            _logManager.Sources.Add(_connector);

            _15mCandleSeries = new CandleSeries(typeof(TimeFrameCandle), _security, TimeSpan.FromMinutes(15))
            {
                BuildCandlesMode = MarketDataBuildModes.Load
            };

            //.................specify candle series information...................
            _1hCandleSeries = new CandleSeries(typeof(TimeFrameCandle), _security, TimeSpan.FromMinutes(60))
            {
                BuildCandlesMode = MarketDataBuildModes.LoadAndBuild
            };



            //.................initialize chart...................
            InitChart();

            //.................connector configurations and events handlers...................
            _connector.CandleSeriesProcessing += Connector_CandleSeriesProcessing;
            
            _connector.NewSecurity += Connector_NewSecurity;
            _connector.MarketDepthChanged += MarketDepthControl.UpdateDepth;
            _connector.NewOrder += OrderGrid.Orders.Add;
            _connector.OrderRegisterFailed += OrderGrid.AddRegistrationFail;

            //.................initializing strategy...................
            //_strategy = new MinimaMaximaStrategy(_1hCandleSeries, long.Parse(txtProcessLimit.Text))
            //{
            //    Security = _security,
            //    Connector = _connector,
            //    Portfolio = _portfolio,
            //};

            _strategy = new FirstStrategy(_1hCandleSeries, _15mCandleSeries, long.Parse(txtProcessLimit.Text))
            {
                Security = _security,
                Connector = _connector,
                Portfolio = _portfolio,
            };

            //.................adding strategy to log manager...................
            _logManager.Sources.Add(_strategy);

            //.................setting strategy events...................
            _strategy.NewMyTrade += MyTradeGrid.Trades.Add;
            _strategy.NewMyTrade += Strategy_NewMyTrade;
            _strategy.PnLChanged += Strategy_PnLChanged;

            //.................setting strategy statistics to the gui...................
            StatisticParameterGrid.Parameters.AddRange(_strategy.StatisticManager.Parameters);

            //.................connecting connector...................
            _connector.Connect();

            _connector.SendInMessage(new CommissionRuleMessage
            {
                Rule = new CommissionPerTradeRule { Value = 0.07m }
            });
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
            data.Group(_strategy.CurrentTime)
                .Add(_pnl, _strategy.PnL)
                .Add(_unrealizedPnL, _strategy.PnLManager.UnrealizedPnL ?? 0)
                .Add(_commissionCurve, _strategy.Commission ?? 0);
            EquityCurveChart.Draw(data);
        }

        private void Strategy_NewMyTrade(MyTrade myTrade)
        {
            var data = new ChartDrawData();
            data.Group(myTrade.Trade.Time)
                .Add(_tradesElem, myTrade);
            Chart.Draw(data);
        }

        private void Connector_NewSecurity(Security security)
        {
            _strategy.Start();
            _connector.Start();
        }

        private void Connector_CandleSeriesProcessing(CandleSeries candleSeries, Candle candle)
        {
            Chart.Draw(_candleElement, candle);
        }
    }
}
