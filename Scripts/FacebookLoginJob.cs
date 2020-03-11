using System;

namespace MultiplayerARPG.MMO
{
    public class FacebookLoginJob : DatabaseJob<string>
    {
        private string id;
        private string email;
        public FacebookLoginJob(BaseDatabase database, string id, string email, Action<string> onFinished = null) : base(database, onFinished)
        {
            this.id = id;
            this.email = email;
        }

        protected override void ThreadFunction()
        {
            result = database.FacebookLogin(id, email);
        }
    }
}
