using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Shared.Models;
using System.Threading.Tasks;

[ApiController]
[Route("[controller]")]
public class PredictController : ControllerBase
{
    [HttpGet("{currency}")]
    public async Task<ActionResult<PredictionResult>> Get
        ([FromServices] IRequestClient<PredictionRequest> client, string currency, int? limit, long? endDate, string tframe)
    {
        var payload = new PredictionRequest(currency, limit, endDate, tframe);
        var request = client.Create(payload);
        var response = await request.GetResponse<PredictionResult>();
        return Ok(response);
    }
}

