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

        private void Start()
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
            {
                // Show error message
                UISceneGlobal.Singleton.ShowMessageDialog("Error", "Failed to Initialize the Facebook SDK");
            }
        }

        public void OnClickFacebookLogin()
        {
            if (FB.IsLoggedIn)
                RequestFacebookLogin(AccessToken.CurrentAccessToken.UserId, AccessToken.CurrentAccessToken.TokenString);
            else
            {
                List<string> perms = new List<string>() { "public_profile", "email" };
                FB.LogInWithReadPermissions(perms, AuthCallback);
            }
        }

        private void RequestFacebookLogin(string id, string accessToken)
        {
            UISceneGlobal uiSceneGlobal = UISceneGlobal.Singleton;
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
            {
                // When facebook login success, send login request to server
                RequestFacebookLogin(AccessToken.CurrentAccessToken.UserId, AccessToken.CurrentAccessToken.TokenString);
            }
            else
            {
                // Show error message
                UISceneGlobal.Singleton.ShowMessageDialog("Cannot Login", "User cancelled login");
            }
        }

        public void OnLogin(ResponseHandlerData responseHandler, AckResponseCode responseCode, ResponseUserLoginMessage response)
        {
            if (responseCode.ShowUnhandledResponseMessageDialog(response.message))
            {
                if (onLoginFail != null)
                    onLoginFail.Invoke();
                return;
            }
            if (onLoginSuccess != null)
                onLoginSuccess.Invoke();
        }
    }
}
