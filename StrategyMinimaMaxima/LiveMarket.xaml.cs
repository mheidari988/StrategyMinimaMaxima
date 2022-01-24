using StockSharp.Algo;
using StockSharp.BusinessEntities;
using StockSharp.Xaml;
using StockSharp.Binance;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace StrategyMinimaMaxima
{
    /// <summary>
    /// Interaction logic for LiveMarket.xaml
    /// </summary>
    public partial class LiveMarket : Window
    {
        private readonly Connector _connector = new Connector();

        public LiveMarket()
        {
            InitializeComponent();
        }

        private void Setting_Click(object sender, RoutedEventArgs e)
        {
            if (_connector.Configure(this))
            {

            }
        }
        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            SecurityEditor.SecurityProvider = _connector;
            PortfolioEditor.Portfolios = new PortfolioDataSource(_connector);
            _connector.NewOrder += OrderGrid.Orders.Add;
            _connector.OrderRegisterFailed += OrderGrid.AddRegistrationFail;
            _connector.NewMyTrade += MyTradeGrid.Trades.Add;
            _connector.Connect();
        }


    }
}
