// See https://aka.ms/new-console-template for more information
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using dotenv.net;

var root = Directory.GetCurrentDirectory();
Console.WriteLine(root);
var dotenv = Path.Combine(root.Replace(@"bin\Debug\net8.0", ""), ".env");
Console.WriteLine(dotenv);
//DotEnv.Load(options: new DotEnvOptions(envFilePaths: new[] { dotenv.ToString() }));
var envs = DotEnv.Read(options: new DotEnvOptions(envFilePaths: new[] { dotenv.ToString() }));
var keys = envs.Keys;

string broker = envs["BROKER_IP"];
int port = 1883;
string clientId = "mqtt-explorer-6c1ebc07";
string topic = "test_topic";

var factory = new MqttFactory();
var mqttClient = factory.CreateMqttClient();
var options = new MqttClientOptionsBuilder()
	.WithTcpServer(broker, port)
	.WithClientId(clientId)
	.WithCleanSession()
	.WithCredentials(envs["USERNAME"], envs["PASS"])
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