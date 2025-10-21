using System.Text.Json.Serialization;

namespace CryptoOculus.Models
{
    public class DigifinexExchangeInfo
    {
        [JsonPropertyName("symbol_list")]
        public DigifinexSymbolList[]? SymbolList { get; set; }
        [JsonPropertyName("code")]
        public int Code { get; set; }
    }
    public class DigifinexSymbolList
    {
        [JsonPropertyName("order_types")]
        public string[]? OrderTypes { get; set; }
        [JsonPropertyName("quote_asset")]
        public required string QuoteAsset { get; set; }
        [JsonPropertyName("minimum_value")]
        public double MinimumValue { get; set; }
        [JsonPropertyName("amount_precision")]
        public int AmountPrecision { get; set; }
        [JsonPropertyName("status")]
        public required string Status { get; set; }
        [JsonPropertyName("minimum_amount")]
        public double MinimumAmount { get; set; }
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }
        [JsonPropertyName("zone")]
        public string? Zone { get; set; }
        [JsonPropertyName("base_asset")]
        public required string BaseAsset { get; set; }
        [JsonPropertyName("price_precision")]
        public int PricePrecision { get; set; }
    }


    public class DigifinexContractAddresses
    {
        public required long Timestamp { get; set; }
        public required DigifinexCurrency[] Currencies { get; set; }
    }
    public class DigifinexCurrency
    {
        public required string Currency { get; set; }
        public required DigifinexAssetNetwork[] Networks { get; set; }
    }
    public class DigifinexAssetNetwork
    {
        public int? NetworkId { get; set; }
        public string? NetworkName { get; set; }
        public string? Address { get; set; }
        public bool DepositEnable { get; set; }
        public bool WithdrawEnable { get; set; }
        public double? TransferTax { get; set; }
        public double? WithdrawFee { get; set; }
        public double? DepositTax { get; set; }
        public string? WithdrawFeeCurrency { get; set; }
    }


    public class DigifinexCurrencySpot
    {
        [JsonPropertyName("errcode")]
        public int Errcode { get; set; }

        [JsonPropertyName("data")]
        public DigifinexCurrencySpotData? Data { get; set; }
    }
    public class DigifinexCurrencySpotData
    {
        [JsonPropertyName("bcounts")]
        public string? Bcounts { get; set; }

        [JsonPropertyName("blist")]
        public DigifinexCurrencySpotBlist[]? Blist { get; set; }

        [JsonPropertyName("pcounts")]
        public string? Pcounts { get; set; }

        [JsonPropertyName("plist")]
        public DigifinexCurrencySpotPlist[]? Plist { get; set; }

        [JsonPropertyName("total_usdt_estimation")]
        public string? TotalUsdtEstimation { get; set; }
    }
    public class DigifinexCurrencySpotBlist
    {
        [JsonPropertyName("area_type")]
        public int AreaType { get; set; }

        [JsonPropertyName("block_type")]
        public int BlockType { get; set; }

        [JsonPropertyName("count")]
        public string? Count { get; set; }

        [JsonPropertyName("currency_english")]
        public string? CurrencyEnglish { get; set; }

        [JsonPropertyName("currency_id")]
        public int CurrencyId { get; set; }

        [JsonPropertyName("currency_logo")]
        public string? CurrencyLogo { get; set; }

        [JsonPropertyName("currency_mark")]
        public required string CurrencyMark { get; set; }

        [JsonPropertyName("currency_name")]
        public string? CurrencyName { get; set; }

        [JsonPropertyName("forzen_num")]
        public string? ForzenNum { get; set; }

        [JsonPropertyName("is_instation")]
        public int IsInstation { get; set; }

        [JsonPropertyName("is_lock")]
        public int IsLock { get; set; }

        [JsonPropertyName("is_recharge")]
        public int IsRecharge { get; set; }

        [JsonPropertyName("is_withdraw")]
        public int IsWithdraw { get; set; }

        [JsonPropertyName("num")]
        public string? Num { get; set; }

        [JsonPropertyName("rmb")]
        public string? Rmb { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("usdt_estimation")]
        public string? UsdtEstimation { get; set; }
    }
    public class DigifinexCurrencySpotPlist
    {
        [JsonPropertyName("area_type")]
        public int AreaType { get; set; }

        [JsonPropertyName("block_type")]
        public int BlockType { get; set; }

        [JsonPropertyName("count")]
        public string? Count { get; set; }

        [JsonPropertyName("currency_english")]
        public string? CurrencyEnglish { get; set; }

        [JsonPropertyName("currency_id")]
        public int CurrencyId { get; set; }

        [JsonPropertyName("currency_logo")]
        public string? CurrencyLogo { get; set; }

        [JsonPropertyName("currency_mark")]
        public required string CurrencyMark { get; set; }

        [JsonPropertyName("currency_name")]
        public string? CurrencyName { get; set; }

        [JsonPropertyName("forzen_num")]
        public string? ForzenNum { get; set; }

        [JsonPropertyName("is_instation")]
        public int IsInstation { get; set; }

        [JsonPropertyName("is_lock")]
        public int IsLock { get; set; }

        [JsonPropertyName("is_recharge")]
        public int IsRecharge { get; set; }

        [JsonPropertyName("is_withdraw")]
        public int IsWithdraw { get; set; }

        [JsonPropertyName("num")]
        public string? Num { get; set; }

        [JsonPropertyName("rmb")]
        public string? Rmb { get; set; }

        [JsonPropertyName("type")]
        public int Type { get; set; }

        [JsonPropertyName("usdt_estimation")]
        public string? UsdtEstimation { get; set; }
    }


    public class DigifinexDepositDetail
    {
        [JsonPropertyName("errcode")]
        public int Errcode { get; set; }

        [JsonPropertyName("data")]
        public DigifinexDepositDetailData? Data { get; set; }
    }
    public class DigifinexDepositDetailData
    {
        [JsonPropertyName("address_type")]
        public string? AddressType { get; set; }

        [JsonPropertyName("address_type_conf")]
        public required DigifinexDepositDetailAddressTypeConf[] AddressTypeConf { get; set; }

        [JsonPropertyName("close_reason")]
        public string? CloseReason { get; set; }

        [JsonPropertyName("close_reason_url")]
        public string? CloseReasonUrl { get; set; }

        [JsonPropertyName("confirms")]
        public int Confirms { get; set; }

        [JsonPropertyName("contract_address")]
        public string? ContractAddress { get; set; }

        [JsonPropertyName("contract_address_query_url")]
        public string? ContractAddressQueryUrl { get; set; }

        [JsonPropertyName("currency_id")]
        public int CurrencyId { get; set; }

        [JsonPropertyName("currency_mark")]
        public string? CurrencyMark { get; set; }

        [JsonPropertyName("currency_type")]
        public int CurrencyType { get; set; }

        [JsonPropertyName("cz_url")]
        public string? CzUrl { get; set; }

        [JsonPropertyName("front_page_show_percent")]
        public string? FrontPageShowPercent { get; set; }

        [JsonPropertyName("if_tag")]
        public int IfTag { get; set; }

        [JsonPropertyName("is_popup_tips_recharge")]
        public int IsPopupTipsRecharge { get; set; }

        [JsonPropertyName("min_cb")]
        public string? MinCb { get; set; }

        [JsonPropertyName("popup_tips_recharge")]
        public string? PopupTipsRecharge { get; set; }

        [JsonPropertyName("recharge_entry_percent")]
        public string? RechargeEntryPercent { get; set; }

        [JsonPropertyName("site_label")]
        public string? SiteLabel { get; set; }

        [JsonPropertyName("tag_name")]
        public string? TagName { get; set; }
    }
    public class DigifinexDepositDetailAddressTypeConf
    {
        [JsonPropertyName("address_type")]
        public required string AddressType { get; set; }

        [JsonPropertyName("close_reason")]
        public string? CloseReason { get; set; }

        [JsonPropertyName("close_reason_url")]
        public string? CloseReasonUrl { get; set; }

        [JsonPropertyName("confirms")]
        public int Confirms { get; set; }

        [JsonPropertyName("fee")]
        public string? Fee { get; set; }

        [JsonPropertyName("fee_currency_mark")]
        public string? FeeCurrencyMark { get; set; }

        [JsonPropertyName("is_enabled")]
        public int IsEnabled { get; set; }

        [JsonPropertyName("min_cb")]
        public string? MinCb { get; set; }

        [JsonPropertyName("min_tb")]
        public string? MinTb { get; set; }

        [JsonPropertyName("receive_time")]
        public int ReceiveTime { get; set; }
    }


    public class DigifinexWithdrawDetail
    {
        [JsonPropertyName("errcode")]
        public int Errcode { get; set; }

        [JsonPropertyName("data")]
        public DigifinexWithdrawDetailData? Data { get; set; }
    }
    public class DigifinexWithdrawDetailData
    {
        [JsonPropertyName("currency_id")]
        public int CurrencyId { get; set; }

        [JsonPropertyName("currency_mark")]
        public string? CurrencyMark { get; set; }

        [JsonPropertyName("currency_type")]
        public int CurrencyType { get; set; }

        [JsonPropertyName("currency_logo")]
        public string? CurrencyLogo { get; set; }

        [JsonPropertyName("is_withdraw")]
        public int IsWithdraw { get; set; }

        [JsonPropertyName("close_reason")]
        public DigifinexWithdrawDetailCloseReason? CloseReason { get; set; }

        [JsonPropertyName("is_eth_token")]
        public int IsEthToken { get; set; }

        [JsonPropertyName("is_line")]
        public int IsLine { get; set; }

        [JsonPropertyName("if_tag")]
        public int IfTag { get; set; }

        [JsonPropertyName("min_tb")]
        public string? MinTb { get; set; }

        [JsonPropertyName("tb_precision")]
        public int TbPrecision { get; set; }

        [JsonPropertyName("reg_withdrawal")]
        public string? RegWithdrawal { get; set; }

        [JsonPropertyName("reg_tag")]
        public string? RegTag { get; set; }

        [JsonPropertyName("tag_name")]
        public string? TagName { get; set; }

        [JsonPropertyName("address_type")]
        public string? AddressType { get; set; }

        [JsonPropertyName("address_type_conf")]
        public required DigifinexWithdrawDetailAddressTypeConf[] AddressTypeConf { get; set; }

        [JsonPropertyName("today_tb_num")]
        public string? TodayTbNum { get; set; }

        [JsonPropertyName("max_tb_num")]
        public string? MaxTbNum { get; set; }

        [JsonPropertyName("available_tb_num")]
        public string? AvailableTbNum { get; set; }

        [JsonPropertyName("used_tb_num")]
        public string? UsedTbNum { get; set; }

        [JsonPropertyName("quota_currency")]
        public string? QuotaCurrency { get; set; }

        [JsonPropertyName("available_num")]
        public string? AvailableNum { get; set; }

        [JsonPropertyName("num")]
        public string? Num { get; set; }

        [JsonPropertyName("forzen_num")]
        public string? ForzenNum { get; set; }

        [JsonPropertyName("fee")]
        public required string Fee { get; set; }

        [JsonPropertyName("fee_currency_mark")]
        public required string FeeCurrencyMark { get; set; }

        [JsonPropertyName("is_fee_less")]
        public int IsFeeLess { get; set; }

        [JsonPropertyName("reg_type")]
        public int RegType { get; set; }

        [JsonPropertyName("extra_tibi_fee_per")]
        public string? ExtraTibiFeePer { get; set; }

        [JsonPropertyName("destroy_tibi_fee_per")]
        public string? DestroyTibiFeePer { get; set; }

        [JsonPropertyName("fee_conf")]
        public DigifinexWithdrawDetailFeeConf[]? FeeConf { get; set; }

        [JsonPropertyName("list")]
        public object[]? List { get; set; }

        [JsonPropertyName("invalid_num")]
        public int InvalidNum { get; set; }

        [JsonPropertyName("is_popup_tips_withdraw")]
        public int IsPopupTipsWithdraw { get; set; }

        [JsonPropertyName("popup_tips_withdraw")]
        public string? PopupTipsWithdraw { get; set; }

        [JsonPropertyName("withdraw_entry_percent")]
        public string? WithdrawEntryPercent { get; set; }

        [JsonPropertyName("chain_name")]
        public string? ChainName { get; set; }

        [JsonPropertyName("receive_time")]
        public int ReceiveTime { get; set; }
    }
    public class DigifinexWithdrawDetailAddressTypeConf
    {
        [JsonPropertyName("address_type")]
        public required string AddressType { get; set; }

        [JsonPropertyName("is_enabled")]
        public int IsEnabled { get; set; }

        [JsonPropertyName("confirms")]
        public int Confirms { get; set; }

        [JsonPropertyName("min_tb")]
        public string? MinTb { get; set; }

        [JsonPropertyName("receive_time")]
        public int ReceiveTime { get; set; }

        [JsonPropertyName("fee")]
        public required string Fee { get; set; }

        [JsonPropertyName("block_status")]
        public int BlockStatus { get; set; }

        [JsonPropertyName("fee_currency_mark")]
        public required string FeeCurrencyMark { get; set; }

        [JsonPropertyName("close_reason")]
        public string? CloseReason { get; set; }

        [JsonPropertyName("close_reason_url")]
        public string? CloseReasonUrl { get; set; }
    }
    public class DigifinexWithdrawDetailCloseReason
    {
        [JsonPropertyName("close_reason")]
        public string? CloseReason { get; set; }

        [JsonPropertyName("close_reason_url")]
        public string? CloseReasonUrl { get; set; }
    }
    public class DigifinexWithdrawDetailFeeConf
    {
        [JsonPropertyName("fee_type")]
        public int FeeType { get; set; }

        [JsonPropertyName("fee")]
        public string? Fee { get; set; }

        [JsonPropertyName("extra_tibi_fee_per")]
        public string? ExtraTibiFeePer { get; set; }

        [JsonPropertyName("block_status")]
        public int BlockStatus { get; set; }

        [JsonPropertyName("status")]
        public int Status { get; set; }

        [JsonPropertyName("is_fee_less")]
        public int IsFeeLess { get; set; }

        [JsonPropertyName("num")]
        public string? Num { get; set; }
    }


    public class DigifinexPrices
    {
        [JsonPropertyName("ticker")]
        public DigifinexPricesTicker[]? Ticker { get; set; }
        [JsonPropertyName("date")]
        public int Date { get; set; }
        [JsonPropertyName("code")]
        public int Code { get; set; }
    }
    public class DigifinexPricesTicker
    {
        [JsonPropertyName("vol")]
        public double Vol { get; set; }
        [JsonPropertyName("change")]
        public double Change { get; set; }
        [JsonPropertyName("base_vol")]
        public double BaseVol { get; set; }
        [JsonPropertyName("sell")]
        public double Sell { get; set; }
        [JsonPropertyName("last")]
        public double Last { get; set; }
        [JsonPropertyName("symbol")]
        public required string Symbol { get; set; }
        [JsonPropertyName("low")]
        public double Low { get; set; }
        [JsonPropertyName("buy")]
        public double Buy { get; set; }
        [JsonPropertyName("high")]
        public double High { get; set; }
    }


    public class DigifinexRefreshToken
    {
        [JsonPropertyName("errcode")]
        public int Errcode { get; set; }

        [JsonPropertyName("data")]
        public DigifinexRefreshTokenData? Data { get; set; }
    }
    public class DigifinexRefreshTokenData
    {
        [JsonPropertyName("access_token")]
        public required string AccessToken { get; set; }

        [JsonPropertyName("expires")]
        public int Expires { get; set; }

        [JsonPropertyName("extra")]
        public DigifinexRefreshTokenExtra? Extra { get; set; }
    }
    public class DigifinexRefreshTokenExtra
    {
        [JsonPropertyName("dm")]
        public DigifinexRefreshTokenDm? Dm { get; set; }
    }
    public class DigifinexRefreshTokenDm
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expires")]
        public int Expires { get; set; }

        [JsonPropertyName("sign_secret")]
        public string? SignSecret { get; set; }
    }


    public class DigifinexOrderBook
    {
        [JsonPropertyName("bids")]
        public double[][]? Bids { get; set; }

        [JsonPropertyName("asks")]
        public double[][]? Asks { get; set; }

        [JsonPropertyName("date")]
        public long Date { get; set; }

        [JsonPropertyName("code")]
        public int Code { get; set; }
    }
}
