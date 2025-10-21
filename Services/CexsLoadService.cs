using System.Collections.Concurrent;

namespace CryptoOculus.Services
{
    public class CexsLoadService(BinanceService binance, MexcService mexc, BybitService bybit, GateService gate, BitgetService bitget, BitmartService bitmart, BingxService bingx, KucoinService kucoin, HtxService htx, OkxService okx, LbankService lbank, CoinwService coinw, CoinexService coinex, BitfinexService bitfinex, XtcomService xtcom, DigifinexService digifinex, ProbitService probit, PhemexService phemex, AscendexService ascendex, PoloniexService poloniex, KrakenService kraken)
    {
        private readonly ConcurrentDictionary<int, IExchange> _exchangeServices = new()
        {
            [binance.ExchangeId] = binance,
            [mexc.ExchangeId] = mexc,
            [bybit.ExchangeId] = bybit,
            [gate.ExchangeId] = gate,
            [bitget.ExchangeId] = bitget,
            [bitmart.ExchangeId] = bitmart,
            [bingx.ExchangeId] = bingx,
            [kucoin.ExchangeId] = kucoin,
            [htx.ExchangeId] = htx,
            [okx.ExchangeId] = okx,
            [lbank.ExchangeId] = lbank,
            [coinw.ExchangeId] = coinw,
            [coinex.ExchangeId] = coinex,
            [bitfinex.ExchangeId] = bitfinex,
            [xtcom.ExchangeId] = xtcom,
            [digifinex.ExchangeId] = digifinex,
            [probit.ExchangeId] = probit,
            [phemex.ExchangeId] = phemex,
            ///[tapbit.ExchangeId] = tapbit,
            [ascendex.ExchangeId] = ascendex,
            [poloniex.ExchangeId] = poloniex,
            [kraken.ExchangeId] = kraken
        };

        public ConcurrentDictionary<int, IExchange> GetCexs()
        {
            return _exchangeServices;
        }

        public int CexsCount()
        {
            return _exchangeServices.Count;
        }

        public string[] CexsNames()
        {
            return [.. _exchangeServices.Values.Select(ex => ex.ExchangeName)];
        }
    }
}
