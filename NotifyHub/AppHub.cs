using Microsoft.AspNetCore.SignalR;
using Shared.Models;
using System;
using System.Threading.Tasks;
public class AppHub : Hub
{
    public async Task Subscribe(string topic) => await Groups.AddToGroupAsync(Context.ConnectionId, topic);
    public async Task Unsubscribe(string topic) => await Groups.RemoveFromGroupAsync(Context.ConnectionId, topic);
    public override async Task OnConnectedAsync()
    {
        Console.WriteLine($"{Context.ConnectionId} joined the conversation");
        await base.OnConnectedAsync();
    }
    public void Publish(string currency, CandleStick kline)
    {
        Console.WriteLine($"publishing on {currency}");
        Clients.Group(currency).SendAsync(currency, kline);
    }
}