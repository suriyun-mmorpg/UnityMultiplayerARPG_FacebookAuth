using Cysharp.Threading.Tasks;
using LiteNetLibManager;

namespace MultiplayerARPG.MMO
{
    public partial interface IDatabaseClient
    {
        UniTask<AsyncResponseData<DbFacebookLoginResp>> RequestDbFacebookLogin(DbFacebookLoginReq request);
    }
}
