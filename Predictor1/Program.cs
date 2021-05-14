using MassTransit;
using System;
using System.Threading;

Console.WriteLine("Predictor1 started");
var busControl = Bus.Factory.CreateUsingRabbitMq(cfg => cfg.ReceiveEndpoint("prediction-requests", e => e.Consumer<PredictionRequestConsumer>()));
await busControl.StartAsync();
new ManualResetEvent(false).WaitOne();
