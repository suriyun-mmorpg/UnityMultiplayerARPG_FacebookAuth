using LiteNetLib.Utils;

namespace MultiplayerARPG.MMO
{
    public struct RequestFacebookLoginMessage : INetSerializable
    {
        public string id;
        public string accessToken;

        public void Deserialize(NetDataReader reader)
        {
            id = reader.GetString();
            accessToken = reader.GetString();
        }

        public void Serialize(NetDataWriter writer)
        {
            writer.Put(id);
            writer.Put(accessToken);
        }
    }
}
