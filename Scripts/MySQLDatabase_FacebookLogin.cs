﻿#if UNITY_SERVER || !MMO_BUILD
using Cysharp.Threading.Tasks;
using MySqlConnector;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        public override async UniTask<string> FacebookLogin(string fbId, string email)
        {
            string id = string.Empty;
            await ExecuteReader((reader) =>
            {
                if (reader.Read())
                    id = reader.GetString(0);
            }, "SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                new MySqlParameter("@username", "fb_" + fbId),
                new MySqlParameter("@password", fbId.PasswordHash()),
                new MySqlParameter("@authType", AUTH_TYPE_FACEBOOK));

            if (string.IsNullOrEmpty(id))
            {
                id = GenericUtils.GetUniqueId();
                await ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                    new MySqlParameter("@id", id),
                    new MySqlParameter("@username", "fb_" + fbId),
                    new MySqlParameter("@password", fbId.PasswordHash()),
                    new MySqlParameter("@email", email),
                    new MySqlParameter("@authType", AUTH_TYPE_FACEBOOK));
            }
            return id;
        }
    }
}
#endif