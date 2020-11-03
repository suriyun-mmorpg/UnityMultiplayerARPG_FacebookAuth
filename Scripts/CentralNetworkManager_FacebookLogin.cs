#if UNITY_STANDALONE && !CLIENT_BUILD
using Cysharp.Threading.Tasks;
using Google.Protobuf;
using LiteNetLib.Utils;
using MiniJSON;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
#endif
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
#if UNITY_STANDALONE && !CLIENT_BUILD
        public const int CUSTOM_REQUEST_FACEBOOK_LOGIN = 110;
        [Header("Facebook Login")]
        public ushort facebookLoginRequestType = 210;

        [DevExtMethods("RegisterServerMessages")]
        protected void RegisterServerMessages_FacebookLogin()
        {
            RegisterServerRequest<RequestFacebookLoginMessage, ResponseUserLoginMessage>(facebookLoginRequestType, HandleRequestFacebookLogin);
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

        protected async UniTaskVoid HandleRequestFacebookLogin(
            RequestHandlerData requestHandler, RequestFacebookLoginMessage request,
            RequestProceedResultDelegate<ResponseUserLoginMessage> result)
        {
            ResponseUserLoginMessage.Error error = ResponseUserLoginMessage.Error.None;
            string userId = string.Empty;
            string accessToken = string.Empty;
            // Validate by facebook api
            string url = "https://graph.facebook.com/" + request.id + "?access_token=" + request.accessToken + "&fields=id,name,email";
            WebClient webClient = new WebClient();
            string json = webClient.DownloadString(url);
            json = json.Replace(@"\u0040", "@");
            Dictionary<string, object> dict = Json.Deserialize(json) as Dictionary<string, object>;
            if (dict.ContainsKey("id") && dict.ContainsKey("email"))
            {
                string fbId = request.id;
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
                NetDataReader dbReader = new NetDataReader(resp.Data.ToByteArray());
                userId = dbReader.GetString();
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
                userPeerInfo.connectionId = requestHandler.ConnectionId;
                userPeerInfo.userId = userId;
                userPeerInfo.accessToken = accessToken = Regex.Replace(Convert.ToBase64String(Guid.NewGuid().ToByteArray()), "[/+=]", "");
                userPeersByUserId[userId] = userPeerInfo;
                userPeers[requestHandler.ConnectionId] = userPeerInfo;
                await DbServiceClient.UpdateAccessTokenAsync(new UpdateAccessTokenReq()
                {
                    UserId = userId,
                    AccessToken = accessToken
                });
            }
            // Response
            result.Invoke(
                 error == ResponseUserLoginMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseUserLoginMessage()
                {
                    error = error,
                    userId = userId,
                    accessToken = accessToken,
                });
        }
#endif

        public bool RequestFacebookLogin(string id, string accessToken, ResponseDelegate extraResponseCallback)
        {
            return ClientSendRequest(facebookLoginRequestType, new RequestFacebookLoginMessage()
            {
                id = id,
                accessToken = accessToken
            }, responseDelegate: extraResponseCallback);
        }
    }
}
