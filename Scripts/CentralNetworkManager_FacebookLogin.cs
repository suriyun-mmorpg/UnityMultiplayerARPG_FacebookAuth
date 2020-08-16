using Google.Protobuf;
using LiteNetLib.Utils;
using LiteNetLibManager;
using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public const int CUSTOM_REQUEST_FACEBOOK_LOGIN = 110;

        [DevExtMethods("RegisterServerMessages")]
        protected void RegisterServerMessages_FacebookLogin()
        {
            RegisterServerMessage(MMOMessageTypes.RequestFacebookLogin, HandleRequestFacebookLogin);
        }

        [DevExtMethods("OnStartServer")]
        protected void OnStartServer_FacebookLogin()
        {
            DatabaseServiceImplement.onCustomRequest -= onCustomRequest_FacebookLogin;
            DatabaseServiceImplement.onCustomRequest += onCustomRequest_FacebookLogin;
        }

        public async Task<CustomResp> onCustomRequest_FacebookLogin(int type, ByteString data)
        {
            string userId = string.Empty;
            if (type == CUSTOM_REQUEST_FACEBOOK_LOGIN)
            {
                NetDataReader reader = new NetDataReader(data.ToByteArray());
                userId = await MMOServerInstance.Singleton.DatabaseNetworkManager.Database.FacebookLogin(reader.GetString(), reader.GetString());
            }
            NetDataWriter writer = new NetDataWriter();
            writer.Put(userId);
            return new CustomResp()
            {
                Type = CUSTOM_REQUEST_FACEBOOK_LOGIN,
                Data = ByteString.CopyFrom(writer.Data)
            };
        }

        public uint RequestFacebookLogin(string id, string accessToken, AckMessageCallback callback)
        {
            RequestFacebookLoginMessage message = new RequestFacebookLoginMessage();
            message.id = id;
            message.accessToken = accessToken;
            return ClientSendRequest(MMOMessageTypes.RequestFacebookLogin, message, callback);
        }
        protected void HandleRequestFacebookLogin(LiteNetLibMessageHandler messageHandler)
        {
            HandleRequestFacebookLoginRoutine(messageHandler);
        }

        async void HandleRequestFacebookLoginRoutine(LiteNetLibMessageHandler messageHandler)
        {
            long connectionId = messageHandler.connectionId;
            RequestFacebookLoginMessage message = messageHandler.ReadMessage<RequestFacebookLoginMessage>();
            ResponseUserLoginMessage.Error error = ResponseUserLoginMessage.Error.None;
            string userId = string.Empty;
            string accessToken = string.Empty;
            // Validate by facebook api
            string url = "https://graph.facebook.com/" + message.id + "?access_token=" + message.accessToken + "&fields=id,name,email";
            WebClient webClient = new WebClient();
            string json = webClient.DownloadString(url);
            json = json.Replace(@"\u0040", "@");
            Dictionary<string, object> dict = Json.Deserialize(json) as Dictionary<string, object>;
            if (dict.ContainsKey("id") && dict.ContainsKey("email"))
            {
                string fbId = message.id;
                string email = (string)dict["email"];
                // Send request to database server
                NetDataWriter writer = new NetDataWriter();
                writer.Put(fbId);
                writer.Put(email);
                CustomResp resp = await DbServiceClient.CustomAsync(new CustomReq()
                {
                    Type = CUSTOM_REQUEST_FACEBOOK_LOGIN,
                    Data = ByteString.CopyFrom(writer.Data)
                });
                // Receive response from database server
                NetDataReader reader = new NetDataReader(resp.Data.ToByteArray());
                userId = reader.GetString();
            }
            // Response clients
            if (string.IsNullOrEmpty(userId))
            {
                error = ResponseUserLoginMessage.Error.InvalidUsernameOrPassword;
                userId = string.Empty;
            }
            else if (userPeersByUserId.ContainsKey(userId) || MapContainsUser(userId))
            {
                error = ResponseUserLoginMessage.Error.AlreadyLogin;
                userId = string.Empty;
            }
            else
            {
                CentralUserPeerInfo userPeerInfo = new CentralUserPeerInfo();
                userPeerInfo.connectionId = connectionId;
                userPeerInfo.userId = userId;
                userPeerInfo.accessToken = accessToken = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
                userPeersByUserId[userId] = userPeerInfo;
                userPeers[connectionId] = userPeerInfo;
                await DbServiceClient.UpdateAccessTokenAsync(new UpdateAccessTokenReq()
                {
                    UserId = userId,
                    AccessToken = accessToken
                });
            }
            ResponseUserLoginMessage responseMessage = new ResponseUserLoginMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseUserLoginMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.userId = userId;
            responseMessage.accessToken = accessToken;
            ServerSendResponse(connectionId, MMOMessageTypes.ResponseUserLogin, responseMessage);
        }
    }
}
