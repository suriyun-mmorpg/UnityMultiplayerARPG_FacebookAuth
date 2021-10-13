#if UNITY_STANDALONE && !CLIENT_BUILD
using Cysharp.Threading.Tasks;
using MiniJSON;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
#endif
using LiteNetLibManager;
using UnityEngine;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        [Header("Facebook Login")]
        public ushort facebookLoginRequestType = 210;

#if UNITY_STANDALONE && !CLIENT_BUILD
        [DevExtMethods("RegisterServerMessages")]
        protected void RegisterServerMessages_FacebookLogin()
        {
            RegisterRequestToServer<RequestFacebookLoginMessage, ResponseUserLoginMessage>(facebookLoginRequestType, HandleRequestFacebookLogin);
        }

        protected async UniTaskVoid HandleRequestFacebookLogin(
            RequestHandlerData requestHandler, RequestFacebookLoginMessage request,
            RequestProceedResultDelegate<ResponseUserLoginMessage> result)
        {
            UITextKeys message = UITextKeys.NONE;
            string userId = string.Empty;
            string accessToken = string.Empty;
            long unbanTime = 0;
            // Validate by facebook api
            string url = "https://graph.facebook.com/" + request.id + "?access_token=" + request.accessToken + "&fields=id,name,email";
            WebClient webClient = new WebClient();
            string json = webClient.DownloadString(url);
            json = json.Replace(@"\u0040", "@");
            Dictionary<string, object> dict = Json.Deserialize(json) as Dictionary<string, object>;
            if (dict.ContainsKey("id") && dict.ContainsKey("email"))
            {
                // Send request to database server
                AsyncResponseData<DbFacebookLoginResp> resp = await DbServiceClient.RequestDbFacebookLogin(new DbFacebookLoginReq()
                {
                    id = request.id,
                    email = (string)dict["email"],
                });
                if (resp.ResponseCode == AckResponseCode.Success)
                {
                    userId = resp.Response.userId;
                }
            }
            // Response clients
            if (string.IsNullOrEmpty(userId))
            {
                message = UITextKeys.UI_ERROR_INVALID_USERNAME_OR_PASSWORD;
                userId = string.Empty;
            }
            else if (userPeersByUserId.ContainsKey(userId) || MapContainsUser(userId))
            {
                message = UITextKeys.UI_ERROR_ALREADY_LOGGED_IN;
                userId = string.Empty;
            }
            else
            {
                GetUserUnbanTimeResp resp = await DbServiceClient.GetUserUnbanTimeAsync(new GetUserUnbanTimeReq()
                {
                    UserId = userId
                });
                unbanTime = resp.UnbanTime;
                if (unbanTime > DateTimeOffset.UtcNow.ToUnixTimeSeconds())
                {
                    message = UITextKeys.UI_ERROR_USER_BANNED;
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
            }
            // Response
            result.Invoke(
                 message == UITextKeys.NONE ? AckResponseCode.Success : AckResponseCode.Error,
                new ResponseUserLoginMessage()
                {
                    message = message,
                    userId = userId,
                    accessToken = accessToken,
                    unbanTime = unbanTime,
                });
        }
#endif

        public bool RequestFacebookLogin(string id, string accessToken, ResponseDelegate<ResponseUserLoginMessage> callback)
        {
            return ClientSendRequest(facebookLoginRequestType, new RequestFacebookLoginMessage()
            {
                id = id,
                accessToken = accessToken
            }, responseDelegate: callback);
        }
    }
}
