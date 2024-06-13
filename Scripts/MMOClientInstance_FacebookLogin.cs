using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class MMOClientInstance
    {
        public void RequestFacebookLogin(string id, string accessToken, ResponseDelegate<ResponseUserLoginMessage> callback)
        {
            CentralNetworkManager.RequestFacebookLogin(id, accessToken, (responseHandler, responseCode, response) => OnRequestUserLogin(responseHandler, responseCode, response, callback).Forget());
        }
    }
}
