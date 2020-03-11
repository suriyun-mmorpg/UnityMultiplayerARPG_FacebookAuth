using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class MMOClientInstance
    {
        public void RequestFacebookLogin(string id, string accessToken, AckMessageCallback callback)
        {
            centralNetworkManager.RequestFacebookLogin(id, accessToken, (responseCode, messageData) => OnRequestUserLogin(responseCode, messageData, callback));
        }
    }
}
