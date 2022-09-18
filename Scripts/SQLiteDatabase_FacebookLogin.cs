#if UNITY_SERVER || !MMO_BUILD
using Cysharp.Threading.Tasks;
using Mono.Data.Sqlite;

namespace MultiplayerARPG.MMO
{
    public partial class SQLiteDatabase
    {
        public override async UniTask<string> FacebookLogin(string fbId, string email)
        {
            await UniTask.Yield();
            string id = string.Empty;
            ExecuteReader((reader) =>
            {
                if (reader.Read())
                    id = reader.GetString(0);
            }, "SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                new SqliteParameter("@username", "fb_" + fbId),
                new SqliteParameter("@password", fbId.PasswordHash()),
                new SqliteParameter("@authType", AUTH_TYPE_FACEBOOK));

            if (string.IsNullOrEmpty(id))
            {
                id = GenericUtils.GetUniqueId();
                ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                    new SqliteParameter("@id", id),
                    new SqliteParameter("@username", "fb_" + fbId),
                    new SqliteParameter("@password", fbId.PasswordHash()),
                    new SqliteParameter("@email", email),
                    new SqliteParameter("@authType", AUTH_TYPE_FACEBOOK));
            }
            return id;
        }
    }
}
#endif