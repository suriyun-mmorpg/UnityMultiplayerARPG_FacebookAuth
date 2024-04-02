#if (UNITY_EDITOR || !EXCLUDE_SERVER_CODES) && UNITY_STANDALONE
using Cysharp.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class BaseDatabase
    {
        public const byte AUTH_TYPE_FACEBOOK = 2;
        public abstract UniTask<string> FacebookLogin(string fbId, string email);
    }
}
#endif