// See https://aka.ms/new-console-template for more information
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;

string broker = "test.mosquitto.org";
int port = 1883;
string clientId = "mqtt-explorer-6c1ebc07";
string topic = "c200_temperature1";

var factory = new MqttFactory();
var mqttClient = factory.CreateMqttClient();
var options = new MqttClientOptionsBuilder()
	.WithTcpServer(broker, port)
	.WithClientId(clientId)
	.WithCleanSession()
	.Build();

var subscribingOptions = new MqttClientSubscribeOptionsBuilder()
	.WithTopicFilter(topic)
	.Build();

var connectResult = await mqttClient.ConnectAsync(options, CancellationToken.None);

if (connectResult.ResultCode == MQTTnet.Client.Connecting.MqttClientConnectResultCode.Success)
{
	Console.WriteLine("Connected to MQTT broker successfully.");

	// Subscribe to a topic
	await mqttClient.SubscribeAsync(subscribingOptions, CancellationToken.None);
	while (true)
	{
		mqttClient.UseApplicationMessageReceivedHandler(e =>
		{
			Console.WriteLine($"Received message: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");
		});
	}
	// Callback function when a message is received
}