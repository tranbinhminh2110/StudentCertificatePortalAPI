using Microsoft.AspNetCore.SignalR;

namespace StudentCertificatePortal_API.Utils
{
    public class NotificationHub: Hub
    {
            public async Task SendNotification(string message)
            {
                await Clients.All.SendAsync("ReceiveNotification", message);
            }

            public override async Task OnConnectedAsync()
            {
                string connectionId = Context.ConnectionId;
                await base.OnConnectedAsync();
            }
    }
}
