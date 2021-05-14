using BinanceExchange.API.Client;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Websockets;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Shared.Lib;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

var config = new ConfigurationBuilder().AddJsonFile($"appsettings.json", true, true).AddEnvironmentVariables().Build();
string apiKey = config.GetValue<string>("apiKey");
string secretKey = secretKey = config.GetValue<string>("secretKey");
var SignalRConnection = new HubConnectionBuilder().WithUrl("http://localhost:5100/ws",
    ops => ops.AccessTokenProvider = () => Task.FromResult("Ticker")).Build();
SignalRConnection.Closed += async (error) => { await Task.Delay(new Random().Next(0, 5) * 1000); await SignalRConnection.StartAsync(); };
await SignalRConnection.StartAsync().ContinueWith(t => Console.WriteLine("Connection Started"));
var binanceClient = new BinanceClient(new ClientConfiguration { ApiKey = apiKey, SecretKey = secretKey });
var binanceWebSocketClient = new DisposableBinanceWebSocketClient(binanceClient);

var currencies = new List<string> { "BTCUSDT", "ETHUSDT" };
var timeframe = new Dictionary<string, KlineInterval> { {"M1", KlineInterval.OneMinute } , {"M5", KlineInterval.FiveMinutes },
    {"M15", KlineInterval.FifteenMinutes },{"M30", KlineInterval.ThirtyMinutes }, {"H1", KlineInterval.OneHour },
    {"H4", KlineInterval.FourHours },{"D1", KlineInterval.OneDay } };

currencies.ForEach(c =>
timeframe.ToList().ForEach(t =>
    binanceWebSocketClient.ConnectToKlineWebSocket(c, t.Value, data =>
    {
        var klineObj = new CandleStick()
        {
            Close = data.Kline.Close,
            High = data.Kline.High,
            Low = data.Kline.Low,
            Open = data.Kline.Open,
            Volume = data.Kline.Volume,
            Timestamp = data.Kline.StartTime.ToToUnixTimestamp()
        };
        SignalRConnection.InvokeAsync("Publish", $"{c}/{t.Key}", klineObj);
    })
));
new ManualResetEvent(false).WaitOne();