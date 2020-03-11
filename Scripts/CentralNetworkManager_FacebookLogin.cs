using LiteNetLib;
using LiteNetLibManager;
using MiniJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace MultiplayerARPG.MMO
{
    public partial class CentralNetworkManager
    {
        public uint RequestFacebookLogin(string id, string accessToken, AckMessageCallback callback)
        {
            RequestFacebookLoginMessage message = new RequestFacebookLoginMessage();
            message.id = id;
            message.accessToken = accessToken;
            return Client.ClientSendAckPacket(DeliveryMethod.ReliableOrdered, MMOMessageTypes.RequestFacebookLogin, message, callback);
        }
        protected void HandleRequestFacebookLogin(LiteNetLibMessageHandler messageHandler)
        {
            StartCoroutine(HandleRequestFacebookLoginRoutine(messageHandler));
        }

        private IEnumerator HandleRequestFacebookLoginRoutine(LiteNetLibMessageHandler messageHandler)
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
                string email = (string)dict["email"];
                FacebookLoginJob job = new FacebookLoginJob(Database, message.id, email);
                job.Start();
                yield return StartCoroutine(job.WaitFor());
                userId = job.result;
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
                UpdateAccessTokenJob updateAccessTokenJob = new UpdateAccessTokenJob(Database, userId, accessToken);
                updateAccessTokenJob.Start();
                yield return StartCoroutine(updateAccessTokenJob.WaitFor());
            }
            ResponseUserLoginMessage responseMessage = new ResponseUserLoginMessage();
            responseMessage.ackId = message.ackId;
            responseMessage.responseCode = error == ResponseUserLoginMessage.Error.None ? AckResponseCode.Success : AckResponseCode.Error;
            responseMessage.error = error;
            responseMessage.userId = userId;
            responseMessage.accessToken = accessToken;
            ServerSendPacket(connectionId, DeliveryMethod.ReliableOrdered, MMOMessageTypes.ResponseUserLogin, responseMessage);
        }
    }
}
