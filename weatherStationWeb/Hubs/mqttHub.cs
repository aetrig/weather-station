using System;
using Microsoft.AspNetCore.SignalR;

namespace weatherStationWeb.Hubs;

public class mqttHub : Hub
{
	public async Task newData()
	{
		await Clients.All.SendAsync("newData");
	}
}
