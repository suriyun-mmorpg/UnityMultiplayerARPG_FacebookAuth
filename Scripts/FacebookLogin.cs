using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using LiteNetLibManager;
using Facebook.Unity;

namespace MultiplayerARPG.MMO
{
    public class FacebookLogin : MonoBehaviour
    {
        public UnityEvent onLoginSuccess;
        public UnityEvent onLoginFail;

        private void Awake()
        {
            if (!FB.IsInitialized)
            {
                // Initialize the Facebook SDK
                FB.Init(InitCallback);
            }
            else
            {
                // Already initialized, signal an app activation App Event
                FB.ActivateApp();
            }
        }

        private void InitCallback()
        {
            if (FB.IsInitialized)
            {
                // Signal an app activation App Event
                FB.ActivateApp();
            }
            else
                Debug.LogError("Failed to Initialize the Facebook SDK");
        }

        public void OnClickFacebookLogin()
        {
            if (FB.IsLoggedIn)
                RequestFacebookLogin(AccessToken.CurrentAccessToken.UserId, AccessToken.CurrentAccessToken.TokenString);
            else
            {
                var perms = new List<string>() { "public_profile", "email" };
                FB.LogInWithReadPermissions(perms, AuthCallback);
            }
        }

        private void RequestFacebookLogin(string id, string accessToken)
        {
            var uiSceneGlobal = UISceneGlobal.Singleton;
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(accessToken))
            {
                uiSceneGlobal.ShowMessageDialog("Cannot login", "User ID or access token is empty");
                return;
            }
            MMOClientInstance.Singleton.RequestFacebookLogin(id, accessToken, OnLogin);
        }

        private void AuthCallback(ILoginResult result)
        {
            if (FB.IsLoggedIn)
                RequestFacebookLogin(AccessToken.CurrentAccessToken.UserId, AccessToken.CurrentAccessToken.TokenString);
            else
                Debug.Log("User cancelled login");
        }

        public void OnLogin(AckResponseCode responseCode, BaseAckMessage message)
        {
            var castedMessage = (ResponseUserLoginMessage)message;
            switch (responseCode)
            {
                case AckResponseCode.Error:
                    var errorMessage = string.Empty;
                    switch (castedMessage.error)
                    {
                        case ResponseUserLoginMessage.Error.AlreadyLogin:
                            errorMessage = "User already logged in";
                            break;
                        case ResponseUserLoginMessage.Error.InvalidUsernameOrPassword:
                            errorMessage = "Invalid username or password";
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog("Cannot Login", errorMessage);
                    if (onLoginFail != null)
                        onLoginFail.Invoke();
                    break;
                case AckResponseCode.Timeout:
                    UISceneGlobal.Singleton.ShowMessageDialog("Cannot Login", "Connection timeout");
                    if (onLoginFail != null)
                        onLoginFail.Invoke();
                    break;
                default:
                    if (onLoginSuccess != null)
                        onLoginSuccess.Invoke();
                    break;
            }
        }
    }
}
