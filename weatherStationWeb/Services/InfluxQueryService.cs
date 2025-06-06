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

		const string sql = "SELECT sensor,temperature,humidity,pressure,wind_speed,time FROM weather ORDER BY time DESC";
		await foreach (var row in client.Query(query: sql))
		{
			rows.Add(new InfluxRow
			{
				sensor = row[0]?.ToString() ?? "",
				temperature = row[1] is null ? null : double.Parse(row[1]!.ToString()!),
				humidity = row[2] is null ? null : double.Parse(row[2]!.ToString()!),
				pressure = row[3] is null ? null : double.Parse(row[3]!.ToString()!),
				wind_speed = row[4] is null ? null : double.Parse(row[4]!.ToString()!),
				time = DateTime.UnixEpoch.AddTicks((long)(BigInteger)(row[5] ?? 0) / 100)
			});
		}
		return rows;
	}

	public async Task<List<string>> QuerySensors()
	{
		var sensorList = new List<string>();

		const string sql = "SELECT DISTINCT sensor FROM weather ORDER BY sensor";
		await foreach (var row in client.Query(query: sql))
		{
			sensorList.Add(row[0]?.ToString() ?? "");
		}
		return sensorList;
	}
}

public class InfluxRow
{
	public string sensor { get; set; } = "";
	public double? temperature { get; set; } = null;
	public double? humidity { get; set; } = null;
	public double? pressure { get; set; } = null;
	public double? wind_speed { get; set; } = null;
	public DateTime time { get; set; }
}