using System.Threading.Tasks;
using InfluxDB3.Client;
using InfluxDB3.Client.Config;
using InfluxDB3.Client.Write;


// // See https://aka.ms/new-console-template for more information
using System.Text;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Client.Subscribing;
using dotenv.net;


//Getting data from .env
var root = Directory.GetCurrentDirectory();
Console.WriteLine(root);
var dotenv = Path.Combine(root.Replace(@"bin\Debug\net8.0", ""), ".env");
Console.WriteLine(dotenv);
var envs = DotEnv.Read(options: new DotEnvOptions(envFilePaths: new[] { dotenv.ToString() }));
var keys = envs.Keys;

//Setting up database connection
const string host = "http://localhost:8181";
string token = envs["INFLUX_AUTH_TOKEN"];
const string database = "weather-station";

using var InfluxDBclient = new InfluxDBClient($"{host}?token={token}&database={database}");

//Setting up MQTT Broker connection
string broker = envs["BROKER_IP"];
int port = 1883;
string clientId = "mqtt-explorer-6c1ebc07";
string topic = "weather/#";

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


//Reading data from MQTT Broker
if (connectResult.ResultCode == MQTTnet.Client.Connecting.MqttClientConnectResultCode.Success)
{
	Console.WriteLine("Connected to MQTT broker successfully.");

	// Subscribe to a topic
	await mqttClient.SubscribeAsync(subscribingOptions, CancellationToken.None);
	while (true)
	{
		mqttClient.UseApplicationMessageReceivedHandler(async e =>
		{
			string topic = e.ApplicationMessage.Topic;
			string payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
			string[] splitTopic = topic.Split("/");

			var point = PointData.Measurement("weather").SetTag("sensor", splitTopic[1]).SetField(splitTopic[2], payload).SetTimestamp(DateTime.UtcNow);
			await InfluxDBclient.WritePointAsync(point: point);


			Console.WriteLine($"Message received -> Full Topic: {topic}\nSensor Name: {splitTopic[1]}\nMeasurement: {splitTopic[2]} Value: {payload}");
		});
	}
}




// const string sql = "SELECT sensor,temperature,humidity,pressure FROM weather";
// Console.WriteLine("{0,-30}{1,-15}{2,-15}{3,-15}", "sensor", "temperature", "humidity", "pressure");
// await foreach (var row in client.Query(query: sql))
// {
// 	Console.WriteLine("{0,-30}{1,-15}{2,-15}{3,-15}", row[0], row[1], row[2], row[3]);
// }
// Console.WriteLine();