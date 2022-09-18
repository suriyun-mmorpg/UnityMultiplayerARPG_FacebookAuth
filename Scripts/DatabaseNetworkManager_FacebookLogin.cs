using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial class DatabaseNetworkManager
    {
        public const int CUSTOM_REQUEST_FACEBOOK_LOGIN = 1010;

        [DevExtMethods("RegisterMessages")]
        protected void RegisterMessages_FacebookLogin()
        {
            RegisterRequestToServer<DbFacebookLoginReq, DbFacebookLoginResp>(CUSTOM_REQUEST_FACEBOOK_LOGIN, DbFacebookLogin);
        }

        public UniTask<AsyncResponseData<DbFacebookLoginResp>> RequestDbFacebookLogin(DbFacebookLoginReq request)
        {
            return Client.SendRequestAsync<DbFacebookLoginReq, DbFacebookLoginResp>(CUSTOM_REQUEST_FACEBOOK_LOGIN, request);
        }

        protected async UniTaskVoid DbFacebookLogin(RequestHandlerData requestHandler, DbFacebookLoginReq request, RequestProceedResultDelegate<DbFacebookLoginResp> result)
        {
#if UNITY_EDITOR || UNITY_SERVER || !MMO_BUILD
            result.Invoke(AckResponseCode.Success, new DbFacebookLoginResp()
            {
                userId = await Database.FacebookLogin(request.id, request.email),
            });
#endif
        }
    }
}
