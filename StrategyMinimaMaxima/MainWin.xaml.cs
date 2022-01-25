using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Ecng.Serialization;
using Ecng.Configuration;

using StockSharp.Algo;
using StockSharp.BusinessEntities;
using StockSharp.Configuration;
using StockSharp.Messages;
using StockSharp.Xaml;
using StockSharp.Algo.Candles;
using StockSharp.Xaml.Charting;
using Ecng.Xaml;

namespace StrategyMinimaMaxima
{
    /// <summary>
    /// Interaction logic for MainWin.xaml
    /// </summary>
    public partial class MainWin : Window
    {
        private readonly Connector _connector = new Connector();
        private const string _connectorFile = "_ConnectorFile.xml";
        private readonly List<Subscription> _subscriptions = new();
        private Security _selectedSecurity;
        private CandleSeries _candleSeries;
        private ChartCandleElement _candleElement;

        public MainWin()
        {
            InitializeComponent();

            _connector.PositionChanged += _connector_PositionChanged;
            _connector.TickTradeReceived += _connector_TickTradeReceived;
            _connector.MarketDepthReceived += _connector_MarketDepthReceived;

            _connector.NewOrder += _connector_NewOrder;
            _connector.NewOrder += OrderGrid.Orders.Add;

            _connector.OrderRegisterFailed += _connector_OrderRegisterFailed;
            _connector.OrderRegisterFailed += OrderGrid.AddRegistrationFail;

            _connector.NewMyTrade += _connector_NewMyTrade;
            _connector.NewMyTrade += MyTradeGrid.Trades.Add;

            _connector.CandleSeriesProcessing += _connector_CandleSeriesProcessing;

            CandleSettingsEditor.Settings = new CandleSeries
            {
                CandleType = typeof(TimeFrameCandle),
                Arg = TimeSpan.FromMinutes(60),
            };
        }

        private void _connector_PositionChanged(Position obj)
        {

        }



        #region Private Methods

        private void UnsubscribeAll()
        {
            if (_candleSeries != null)
                _connector.UnSubscribeCandles(_candleSeries);

            foreach (var sub in _subscriptions)
                _connector.UnSubscribe(sub);

            _subscriptions.Clear();
        }

        private void UpdateInfoTextBox()
        {
            StringBuilder str = new StringBuilder();

            if (PortfolioEditor.SelectedPortfolio != null && SecurityPicker.SelectedSecurity != null)
            {
                var pos = _connector.GetPosition(PortfolioEditor.SelectedPortfolio, SecurityPicker.SelectedSecurity);

                str.AppendLine($"BeginValue: {pos.BeginValue}");
                str.AppendLine($"AveragePrice: {pos.AveragePrice}");
                str.AppendLine($"BlockedValue: {pos.BlockedValue}");
                str.AppendLine($"BuyOrdersCount: {pos.BuyOrdersCount}");
                str.AppendLine($"BuyOrdersMargin: {pos.BuyOrdersMargin}");
                str.AppendLine($"ClientCode: {pos.ClientCode}");
                str.AppendLine($"Commission: {pos.Commission}");
                str.AppendLine($"CommissionMaker: {pos.CommissionMaker}");
                str.AppendLine($"CommissionTaker: {pos.CommissionTaker}");
                str.AppendLine($"Currency: {pos.Currency}");
                str.AppendLine($"CurrentPrice: {pos.CurrentPrice}");
                str.AppendLine($"CurrentValue: {pos.CurrentValue}");
                str.AppendLine($"DepoName: {pos.DepoName}");
                str.AppendLine($"Description: {pos.Description}");
                str.AppendLine($"ExpirationDate: {pos.ExpirationDate}");
                str.AppendLine($"LastChangeTime: {pos.LastChangeTime}");
                str.AppendLine($"Leverage: {pos.Leverage}");
                str.AppendLine($"LimitType: {pos.LimitType}");
                str.AppendLine($"LocalTime: {pos.LocalTime}");
                str.AppendLine($"OrdersCount: {pos.OrdersCount}");
                str.AppendLine($"OrdersMargin: {pos.OrdersMargin}");
                str.AppendLine($"Portfolio: {pos.Portfolio}");
                str.AppendLine($"RealizedPnL: {pos.RealizedPnL}");
                str.AppendLine($"Security: {pos.Security}");
                str.AppendLine($"SellOrdersCount: {pos.SellOrdersCount}");
                str.AppendLine($"SellOrdersMargin: {pos.SellOrdersMargin}");
                str.AppendLine($"SettlementPrice: {pos.SettlementPrice}");
                str.AppendLine($"Side: {pos.Side}");
                str.AppendLine($"StrategyId: {pos.StrategyId}");
                str.AppendLine($"TradesCount: {pos.TradesCount}");
                str.AppendLine($"UnrealizedPnL: {pos.UnrealizedPnL}");
                str.AppendLine($"VariationMargin: {pos.VariationMargin}");

                txtInfo.Text = str.ToString();
            }
        }

        #endregion

        #region StockSharp Events

        private void _connector_CandleSeriesProcessing(CandleSeries candleSeries, Candle candle)
        {
            Chart.Draw(_candleElement, candle);

            Dispatcher.Invoke(() => 
            {
                Title = candle.ToString();
            });
        }

        private void _connector_MarketDepthReceived(Subscription sub, MarketDepth depth)
        {
            if (depth.Security == _selectedSecurity)
                MarketDepthControl.UpdateDepth(depth);
        }

        private void _connector_TickTradeReceived(Subscription sub, Trade trade)
        {
            if (trade.Security == _selectedSecurity)
            {
                TradeGrid.Trades.Add(trade);
            }
        }

        private void _connector_NewMyTrade(MyTrade obj)
        {
            //throw new NotImplementedException();
        }

        private void _connector_OrderRegisterFailed(OrderFail obj)
        {
            //throw new NotImplementedException();
        }

        private void _connector_NewOrder(Order obj)
        {
            //throw new NotImplementedException();
        }

        private void SecurityPicker_SecuritySelected(Security security)
        {
            // cancel old subscriptions
            UnsubscribeAll();
            _selectedSecurity = security;
            if (_selectedSecurity == null)
                return;

            //-----------------Chart--------------------------------
            _candleSeries = new CandleSeries(CandleSettingsEditor.Settings.CandleType, security, CandleSettingsEditor.Settings.Arg)
            {
                BuildCandlesMode = MarketDataBuildModes.LoadAndBuild,
            };

            Chart.ClearAreas();

            var area = new ChartArea();
            _candleElement = new ChartCandleElement();

            Chart.AddArea(area);
            Chart.AddElement(area, _candleElement, _candleSeries);

            _connector.SubscribeCandles(_candleSeries, DateTime.Today.Subtract(TimeSpan.FromDays(10)));


            //-----------------Trades and Depth----------------------

            _subscriptions.Add(_connector.SubscribeLevel1(security));

            _subscriptions.Add(_connector.SubscribeTrades(security));

            _subscriptions.Add(_connector.SubscribePositions(security));

            MarketDepthControl.Clear();
            _subscriptions.Add(_connector.SubscribeMarketDepth(security));
        }

        #endregion

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            if (_connector.Configure(this))
            {
                //_connector.Save().Serialize(_connectorFile);
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            SecurityPicker.SecurityProvider = _connector;
            SecurityPicker.SelectedType = SecurityTypes.Future;
            SecurityPicker.MarketDataProvider = _connector;

            PortfolioEditor.Portfolios = new PortfolioDataSource(_connector);

            _connector.Connect();
        }

        private void Sell_Click(object sender, RoutedEventArgs e)
        {
            var order = new Order
            {
                Type = OrderTypes.Market,
                Security = SecurityPicker.SelectedSecurity,
                Portfolio = PortfolioEditor.SelectedPortfolio,
                Volume = decimal.Parse(TextBoxLotSize.Text),
                Direction = Sides.Sell,
            };
            _connector.RegisterOrder(order);
        }

        private void Buy_Click(object sender, RoutedEventArgs e)
        {
            var order = new Order
            {
                Type= OrderTypes.Market,
                Security = SecurityPicker.SelectedSecurity,
                Portfolio = PortfolioEditor.SelectedPortfolio,
                Volume = decimal.Parse(TextBoxLotSize.Text),
                Direction = Sides.Buy,
            };
            _connector.RegisterOrder(order);
        }

        private void btnInfo_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
