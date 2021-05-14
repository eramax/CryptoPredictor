using BinanceExchange.API.Models.WebSocket;
using Newtonsoft.Json;

namespace Shared.Models
{
    public class CandleStick : KlineCandleStick
    {
        public long Timestamp { get; set; }
    }
}
