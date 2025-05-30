using System;
using dotenv.net;
using InfluxDB3.Client;
using MQTTnet.Client;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using System.Text;
using InfluxDB3.Client.Write;

namespace weatherStationWeb.Services;

public class MqttInfluxService : BackgroundService
{
	private IMqttClient mqttClient = null!;
	private InfluxDBClient DbClient = null!;

	public override async Task StartAsync(CancellationToken cancellationToken)
	{
		DotEnv.Load();

		//Setting up DB Client 
		const string host = "http://localhost:8181";
		string token = Environment.GetEnvironmentVariable("INFLUX_AUTH_TOKEN") ?? "";
		const string database = "weather-station";
		DbClient = new InfluxDBClient($"{host}?token={token}&database={database}");

		//Setting up MQTT Broker connection
		string broker = Environment.GetEnvironmentVariable("BROKER_IP") ?? "";
		int port = 1883;
		string clientId = "mqtt-explorer-6c1ebc07";
		string topic = "weather/#";

		var factory = new MqttFactory();
		var mqttClient = factory.CreateMqttClient();
		var options = new MqttClientOptionsBuilder()
			.WithTcpServer(broker, port)
			.WithClientId(clientId)
			.WithCleanSession()
			.WithCredentials(Environment.GetEnvironmentVariable("USERNAME"), Environment.GetEnvironmentVariable("PASS"))
			.Build();

		mqttClient.UseApplicationMessageReceivedHandler(async e =>
		{
			string topic = e.ApplicationMessage.Topic;
			string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
			string[] splitTopic = topic.Split("/");

			var point = PointData.Measurement("weather").SetTag("sensor", splitTopic[1]).SetField(splitTopic[2], payload).SetTimestamp(DateTime.UtcNow);
			await DbClient.WritePointAsync(point: point);
		});

		var connectResult = await mqttClient.ConnectAsync(options, CancellationToken.None);
		if (connectResult.ResultCode == MQTTnet.Client.Connecting.MqttClientConnectResultCode.Success)
		{
			// Subscribe to a topic
			var subscribingOptions = new MqttClientSubscribeOptionsBuilder()
						.WithTopicFilter(topic)
						.Build();
			await mqttClient.SubscribeAsync(subscribingOptions, CancellationToken.None);
		}
	}

	protected override Task ExecuteAsync(CancellationToken stoppingToken)
	{
		return Task.CompletedTask;
	}

	public override async Task StopAsync(CancellationToken cancellationToken)
	{
		if (mqttClient.IsConnected)
			await mqttClient.DisconnectAsync();
		DbClient.Dispose();

		await base.StopAsync(cancellationToken);
	}
}
