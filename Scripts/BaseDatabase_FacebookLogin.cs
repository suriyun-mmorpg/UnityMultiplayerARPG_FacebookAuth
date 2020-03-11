namespace MultiplayerARPG.MMO
{
    public partial class BaseDatabase
    {
        public const byte AUTH_TYPE_FACEBOOK = 2;
        public abstract string FacebookLogin(string fbId, string email);
    }
}
