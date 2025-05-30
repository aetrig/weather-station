using System;
using System.Numerics;
using dotenv.net;
using InfluxDB3.Client;

namespace weatherStationWeb.Services;

public class InfluxQueryService
{
	private InfluxDBClient client;

	public InfluxQueryService()
	{
		DotEnv.Load();
		const string host = "http://localhost:8181";
		string token = Environment.GetEnvironmentVariable("INFLUX_AUTH_TOKEN") ?? "";
		const string database = "weather-station";
		client = new InfluxDBClient($"{host}?token={token}&database={database}");
	}

	public async Task<List<InfluxRow>> QueryAsync()
	{
		var rows = new List<InfluxRow>();

		const string sql = "SELECT sensor,temperature,humidity,pressure,time FROM weather ORDER BY time DESC";
		await foreach (var row in client.Query(query: sql))
		{
			rows.Add(new InfluxRow
			{
				sensor = row[0]?.ToString() ?? "",
				temperature = double.Parse(row[1]?.ToString() ?? "0.0"),
				humidity = double.Parse(row[2]?.ToString() ?? "0.0"),
				pressure = double.Parse(row[3]?.ToString() ?? "0.0"),
				time = DateTime.UnixEpoch.AddTicks((long)(BigInteger)(row[4] ?? 0) / 100)
			});
		}
		return rows;
	}
}

public class InfluxRow
{
	public string sensor { get; set; } = "";
	public double temperature { get; set; } = 0.0;
	public double humidity { get; set; } = 0.0;
	public double pressure { get; set; } = 0.0;
	public DateTime time { get; set; }
}