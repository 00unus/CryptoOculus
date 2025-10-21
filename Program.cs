using CryptoOculus;
using CryptoOculus.Controllers;
using CryptoOculus.Models;
using CryptoOculus.Services;
using Serilog;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "Cache/"));
Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, "Logs/"));

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Host.UseSerilog((context, services, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

new ClientService().ConfigurateClientsAsync(builder, [.. File.ReadAllLines("Data/proxyList.txt").Where(line => !string.IsNullOrWhiteSpace(line))]);

builder.Configuration.AddJsonFile("Data/apiKeys.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("Data/rateLimiting.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("Data/networkList.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("Data/spreadsBlacklist.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("Data/coinsBlacklist.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("Data/languages.json", optional: false, reloadOnChange: true);
builder.Configuration.AddJsonFile("Data/tokenTransferTaxes.json", optional: true, reloadOnChange: true);
builder.Configuration.AddJsonFile("Cache/honeypotIsCheckedTokens.json", optional: true, reloadOnChange: true);

builder.Services.Configure<ApiKeysOptions>(builder.Configuration.GetSection("ApiKeys"));
builder.Services.Configure<RateLimitingOptions>(builder.Configuration.GetSection("RateLimiting"));
builder.Services.Configure<List<NetworkList>>(builder.Configuration.GetSection("NetworkList"));
builder.Services.Configure<Dictionary<string, SpreadsBlacklistItem>>(builder.Configuration.GetSection("SpreadsBlacklist"));
builder.Services.Configure<Dictionary<string, CoinsBlacklistItem>>(builder.Configuration.GetSection("CoinsBlacklist"));
builder.Services.Configure<Languages>(builder.Configuration.GetSection("Languages"));
builder.Services.Configure<List<TokenTransferTax>>(builder.Configuration.GetSection("TokenTransferTaxes"));
builder.Services.Configure<List<CheckedTokens>>(builder.Configuration.GetSection("CheckedTokens"));

builder.Services.AddHostedService<CexCompareService>();
builder.Services.AddSingleton<RateLimiterManager>();
builder.Services.AddSingleton<CexsLoadService>();
builder.Services.AddTransient<LanguagesService>();
builder.Services.AddTransient<ApiKeysService>();
builder.Services.AddTransient<HoneypotIsService>();
builder.Services.AddTransient<DataService>();
builder.Services.AddTransient<LocalCexCompareService>();
builder.Services.AddTransient<CustomHandler>();
builder.Services.AddTransient<RateLimitHandler>();
builder.Services.AddTransient<ValidationHandler>();
builder.Services.AddTransient<TelegramController>();
builder.Services.AddTransient<TelegramService>();
builder.Services.AddTransient<UserService>();
builder.Services.AddTransient<Helper>();
builder.Services.AddTransient<BinanceService>();
builder.Services.AddTransient<MexcService>();
builder.Services.AddTransient<BybitService>();
builder.Services.AddTransient<GateService>();
builder.Services.AddTransient<BitgetService>();
builder.Services.AddTransient<BitmartService>();
builder.Services.AddTransient<BingxService>();
builder.Services.AddTransient<KucoinService>();
builder.Services.AddTransient<HtxService>();
builder.Services.AddTransient<OkxService>();
builder.Services.AddTransient<LbankService>();
builder.Services.AddTransient<CoinwService>();
builder.Services.AddTransient<CoinexService>();
builder.Services.AddTransient<BitfinexService>();
builder.Services.AddTransient<XtcomService>();
builder.Services.AddTransient<DigifinexService>();
builder.Services.AddTransient<ProbitService>();
builder.Services.AddTransient<PhemexService>();
builder.Services.AddTransient<TapbitService>();
builder.Services.AddTransient<AscendexService>();
builder.Services.AddTransient<PoloniexService>();
builder.Services.AddTransient<KrakenService>();
builder.Services.AddTransient<Testing>();
builder.Services.AddTransient<DnsUpdateService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    //app.UseHsts();
}

LanguagesService languagesService = app.Services.GetRequiredService<LanguagesService>();
ApiKeysService apiKeysService = app.Services.GetRequiredService<ApiKeysService>();

//app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
//app.UseAuthorization();
app.MapRazorPages();
app.MapControllers();
app.Run();