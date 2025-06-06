using System;
using Microsoft.AspNetCore.SignalR;

namespace weatherStationWeb.Hubs;

public class mqttHub : Hub
{
	public async Task SendDataUpdate()
	{
		await Clients.All.SendAsync("ReceiveDataUpdate");
	}
}
