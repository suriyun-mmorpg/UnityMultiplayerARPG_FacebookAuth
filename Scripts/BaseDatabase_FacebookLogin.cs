using System.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class BaseDatabase
    {
        public const byte AUTH_TYPE_FACEBOOK = 2;
        public abstract Task<string> FacebookLogin(string fbId, string email);
    }
}
