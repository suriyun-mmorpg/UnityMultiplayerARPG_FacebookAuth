using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class MMOClientInstance
    {
        public void RequestFacebookLogin(string id, string accessToken, AckMessageCallback<ResponseUserLoginMessage> callback)
        {
            centralNetworkManager.RequestFacebookLogin(id, accessToken, (messageData) => OnRequestUserLogin(messageData, callback));
        }
    }
}
