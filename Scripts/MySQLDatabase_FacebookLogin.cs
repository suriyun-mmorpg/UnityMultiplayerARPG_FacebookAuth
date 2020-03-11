using MySql.Data.MySqlClient;

namespace MultiplayerARPG.MMO
{
    public partial class MySQLDatabase
    {
        public override string FacebookLogin(string fbId, string email)
        {
            string id = string.Empty;
            MySQLRowsReader reader = ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                new MySqlParameter("@username", "fb_" + fbId),
                new MySqlParameter("@password", GenericUtils.GetMD5(fbId)),
                new MySqlParameter("@authType", AUTH_TYPE_FACEBOOK));

            if (reader.Read())
                id = reader.GetString("id");
            else
            {
                ExecuteNonQuery("INSERT INTO userlogin (id, username, password, email, authType) VALUES (@id, @username, @password, @email, @authType)",
                    new MySqlParameter("@id", GenericUtils.GetUniqueId()),
                    new MySqlParameter("@username", "fb_" + fbId),
                    new MySqlParameter("@password", GenericUtils.GetMD5(fbId)),
                    new MySqlParameter("@email", email),
                    new MySqlParameter("@authType", AUTH_TYPE_FACEBOOK));

                // Read last entry
                reader = ExecuteReader("SELECT id FROM userlogin WHERE username=@username AND password=@password AND authType=@authType LIMIT 1",
                    new MySqlParameter("@username", "fb_" + fbId),
                    new MySqlParameter("@password", GenericUtils.GetMD5(fbId)),
                    new MySqlParameter("@authType", AUTH_TYPE_FACEBOOK));

                if (reader.Read())
                    id = reader.GetString("id");
            }
            return id;
        }
    }
}