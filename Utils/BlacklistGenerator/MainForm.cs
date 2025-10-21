using CryptoOculus;
using CryptoOculus.Models;
using System.ComponentModel;
using System.Text.Json;

namespace BlacklistGenerator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private async void ReloadButton_Click(object sender, EventArgs e)
        {
            resultsDataGridView.Rows.Clear();

            Spread[] spreads = JsonSerializer.Deserialize<Spread[]>(await File.ReadAllTextAsync("../../../../../Cache/ResultsLast.json"))!;

            foreach (Spread spread in spreads)
            {
                int index = resultsDataGridView.Rows.Add(
                    Math.Round(spread.SpreadPercent * 100, 2),
                    Math.Round(spread.Profit, 2),
                    Math.Round(spread.BuyExchangePair.AskAmount, 2),
                    $"{spread.BuyExchangePair.ExchangeName} ({spread.BuyExchangePair.BaseAsset}/{spread.BuyExchangePair.QuoteAsset})",
                    $"{spread.SellExchangePair.ExchangeName} ({spread.SellExchangePair.BaseAsset}/{spread.SellExchangePair.QuoteAsset})",
                    string.Join(",", spread.BuyExchangePair.BaseAssetNetworks.Select(m => m.NetworkName)),
                    string.Join(",", spread.SellExchangePair.BaseAssetNetworks.Select(m => m.NetworkName)),
                    JsonSerializer.Serialize(spread));

                resultsDataGridView.Rows[index].Cells["BuyPair"].Tag = GetExchangePairLink(spread.BuyExchangeId, spread.BuyExchangePair.BaseAsset, spread.BuyExchangePair.QuoteAsset);
                resultsDataGridView.Rows[index].Cells["SellPair"].Tag = GetExchangePairLink(spread.SellExchangeId, spread.SellExchangePair.BaseAsset, spread.SellExchangePair.QuoteAsset);
            }

            resultsDataGridView.Sort(resultsDataGridView.Columns["Spread"]!, ListSortDirection.Descending);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void ResultsDataGridView_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (resultsDataGridView.Columns[e.ColumnIndex] is DataGridViewLinkColumn)
            {
                string? url = resultsDataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag?.ToString();
                if (!String.IsNullOrWhiteSpace(url))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = url,
                        UseShellExecute = true
                    });
                }
            }
        }

        public static string GetExchangePairLink(int id, string baseAsset, string quoteAsset)
        {
            return id switch
            {
                0 => $"https://www.binance.com/trade/{baseAsset}_{quoteAsset}?type=spot",
                1 => $"https://www.mexc.com/exchange/{baseAsset}_{quoteAsset}",
                2 => $"https://www.bybit.com/trade/spot/{baseAsset}/{quoteAsset}",
                3 => $"https://www.gate.io/trade/{baseAsset}_{quoteAsset}",
                4 => $"https://www.bitget.com/spot/{baseAsset}{quoteAsset}",
                5 => $"https://www.bitmart.com/trade?type=spot&symbol={baseAsset}_{quoteAsset}",
                6 => $"https://bingx.com/spot/{baseAsset}{quoteAsset}",
                7 => $"https://www.kucoin.com/trade/{baseAsset}-{quoteAsset}",
                8 => $"https://www.htx.com/trade/{baseAsset.ToLower()}_{quoteAsset.ToLower()}?type=spot",
                9 => $"https://www.okx.com/trade-spot/{baseAsset.ToLower()}-{quoteAsset.ToLower()}",
                10 => $"https://www.lbank.com/trade/{baseAsset.ToLower()}_{quoteAsset.ToLower()}",
                11 => $"https://www.coinw.com/spot/{baseAsset.ToLower()}{quoteAsset.ToLower()}",
                12 => $"https://www.coinex.com/exchange/{baseAsset.ToLower()}-{quoteAsset.ToLower()}#spot",
                //13 => $"https://trading.bitfinex.com/t/{bitfinexPair}",
                14 => $"https://www.xt.com/trade/{baseAsset.ToLower()}_{quoteAsset.ToLower()}",
                15 => $"https://www.digifinex.com/en-ww/trade/{quoteAsset.ToUpper()}/{baseAsset.ToUpper()}",
                16 => $"https://www.probit.com/app/exchange/{baseAsset.ToUpper()}-{quoteAsset.ToUpper()}",
                17 => $"https://phemex.com/spot/trade/{baseAsset.ToUpper()}{quoteAsset.ToUpper()}",
                18 => $"https://www.tapbit.com/spot/exchange/{baseAsset.ToUpper()}_{quoteAsset.ToUpper()}",
                19 => $"https://ascendex.com/cashtrade-spottrading/{quoteAsset.ToLower()}/{baseAsset.ToLower()}",
                20 => $"https://poloniex.com/trade/{baseAsset.ToUpper()}_{quoteAsset.ToUpper()}",
                21 => $"https://pro.kraken.com/app/trade/{baseAsset.ToLower()}-{quoteAsset.ToLower()}",
                _ => "Exchange pair link get error (id not found)"
            };
        }

        private async void AddSpread_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Add this spread to blacklist?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
            {
                Spread compareResult = JsonSerializer.Deserialize<Spread>(resultsDataGridView.CurrentRow!.Cells[^1].Value!.ToString()!)!;
                Dictionary<string, SpreadsBlacklistItem> spreadsBlacklist = JsonSerializer.Deserialize<SpreadsBlacklistWrapper>(await File.ReadAllTextAsync("../../../../../Data/spreadsBlacklist.json"))!.SpreadsBlacklist;
                spreadsBlacklist.Add(compareResult.Id, new()
                {
                    Date = DateTimeOffset.UtcNow,
                    BuyExchangeId = compareResult.BuyExchangeId,
                    SellExchangeId = compareResult.SellExchangeId,
                    BuyExchangePairBaseAsset = compareResult.BuyExchangePair.BaseAsset,
                    BuyExchangePairQuoteAsset = compareResult.BuyExchangePair.QuoteAsset,
                    SellExchangePairBaseAsset = compareResult.SellExchangePair.BaseAsset,
                    SellExchangePairQuoteAsset = compareResult.SellExchangePair.QuoteAsset
                });
                await File.WriteAllTextAsync("../../../../../Data/spreadsBlacklist.json", JsonSerializer.Serialize(new SpreadsBlacklistWrapper() { SpreadsBlacklist = spreadsBlacklist }, Helper.serializeOptions));
                resultsDataGridView.Rows.Remove(resultsDataGridView.CurrentRow);
            }
        }

        private async void AddBuyExCoin_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Add this coin to blacklist?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Spread compareResult = JsonSerializer.Deserialize<Spread>(resultsDataGridView.CurrentRow!.Cells[^1].Value!.ToString()!)!;
                Dictionary<string, CoinsBlacklistItem> coinsBlacklist = JsonSerializer.Deserialize<CoinsBlacklistWrapper>(await File.ReadAllTextAsync("../../../../../Data/coinsBlacklist.json"))!.CoinsBlacklist;
                coinsBlacklist.Add($"{compareResult.BuyExchangePair.BaseAsset}%||%{compareResult.BuyExchangeId}", new()
                {
                    Date = DateTimeOffset.UtcNow,
                    ExchangeId = compareResult.BuyExchangeId,
                    BaseAsset = compareResult.BuyExchangePair.BaseAsset
                });
                await File.WriteAllTextAsync("../../../../../Data/coinsBlacklist.json", JsonSerializer.Serialize(new CoinsBlacklistWrapper() { CoinsBlacklist = coinsBlacklist }, Helper.serializeOptions));
                //resultsDataGridView.Rows.Remove(resultsDataGridView.CurrentRow);
            }
        }

        private async void AddSellExCoin_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Add this coin to blacklist?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                Spread compareResult = JsonSerializer.Deserialize<Spread>(resultsDataGridView.CurrentRow!.Cells[^1].Value!.ToString()!)!;
                Dictionary<string, CoinsBlacklistItem> coinsBlacklist = JsonSerializer.Deserialize<CoinsBlacklistWrapper>(await File.ReadAllTextAsync("../../../../../Data/coinsBlacklist.json"))!.CoinsBlacklist;
                coinsBlacklist.Add($"{compareResult.SellExchangePair.BaseAsset}%||%{compareResult.SellExchangeId}", new()
                {
                    Date = DateTimeOffset.UtcNow,
                    ExchangeId = compareResult.SellExchangeId,
                    BaseAsset = compareResult.SellExchangePair.BaseAsset
                });
                await File.WriteAllTextAsync("../../../../../Data/coinsBlacklist.json", JsonSerializer.Serialize(new CoinsBlacklistWrapper() { CoinsBlacklist = coinsBlacklist }, Helper.serializeOptions));
                //resultsDataGridView.Rows.Remove(resultsDataGridView.CurrentRow);
            }
        }

        private async void AddCoinForAll_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Add this coin to blacklist for all exchanges?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
            {
                Spread compareResult = JsonSerializer.Deserialize<Spread>(resultsDataGridView.CurrentRow!.Cells[^1].Value!.ToString()!)!;
                Dictionary<string, CoinsBlacklistItem> coinsBlacklist = JsonSerializer.Deserialize<CoinsBlacklistWrapper>(await File.ReadAllTextAsync("../../../../../Data/coinsBlacklist.json"))!.CoinsBlacklist;
                coinsBlacklist.Add($"{compareResult.BuyExchangePair.BaseAsset}%||%ALL", new()
                {
                    Date = DateTimeOffset.UtcNow,
                    BaseAsset = compareResult.BuyExchangePair.BaseAsset
                });
                await File.WriteAllTextAsync("../../../../../Data/coinsBlacklist.json", JsonSerializer.Serialize(new CoinsBlacklistWrapper() { CoinsBlacklist = coinsBlacklist }, Helper.serializeOptions));
                //resultsDataGridView.Rows.Remove(resultsDataGridView.CurrentRow);
            }
        }
    }
    public class SpreadsBlacklistWrapper
    {
        public required Dictionary<string, SpreadsBlacklistItem> SpreadsBlacklist { get; set; }
    }
    public class CoinsBlacklistWrapper
    {
        public required Dictionary<string, CoinsBlacklistItem> CoinsBlacklist { get; set; }
    }
}
