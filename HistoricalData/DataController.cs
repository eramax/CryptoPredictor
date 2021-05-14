using BinanceExchange.API.Client.Interfaces;
using BinanceExchange.API.Enums;
using BinanceExchange.API.Models.Request;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Shared.Lib;


[ApiVersion("1")]
[ApiController]
[Route("[controller]")]
public class DataController : ControllerBase
{
    //BTCUSDT?tframe=4&limit=H1&startDate=10000&endDate=1619945168
    [ResponseCache(Duration = 50, Location = ResponseCacheLocation.Any, NoStore = false)]
    [HttpGet("{currency}")]
    public async Task<IEnumerable<CandleStick>> Get([FromServices] IBinanceClient binanceClient, string currency,
         int? limit, long? startDate, long? endDate, string tframe)
    {
        KlineInterval interval = tframe switch
        {
            "M1" => KlineInterval.OneMinute,
            "M5" => KlineInterval.FiveMinutes,
            "M15" => KlineInterval.FifteenMinutes,
            "M30" => KlineInterval.ThirtyMinutes,
            "H1" => KlineInterval.OneHour,
            "H4" => KlineInterval.FourHours,
            "D1" => KlineInterval.OneDay,
            "W1" => KlineInterval.OneWeek,
            _ => KlineInterval.OneHour
        };
        var opts = new GetKlinesCandlesticksRequest { Interval = interval, Limit = limit, Symbol = currency };
        opts.StartTime = (startDate.HasValue) ? DateTimeOffset.FromUnixTimeSeconds(startDate.Value).DateTime.ToLocalTime() : null;
        opts.EndTime = (endDate.HasValue) ? DateTimeOffset.FromUnixTimeSeconds(endDate.Value).DateTime.ToLocalTime() : null;

        var data = await binanceClient.GetKlinesCandlesticks(opts);
        var result = data.Select(d => new CandleStick()
        {
            Open = d.Open,
            Close = d.Close,
            High = d.High,
            Low = d.Low,
            Volume = d.Volume,
            Timestamp = d.OpenTime.ToToUnixTimestamp()
        }).ToList();
        return result;
    }
}

