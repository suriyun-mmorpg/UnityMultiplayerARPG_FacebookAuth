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

        public void OnLogin(ResponseUserLoginMessage message)
        {
            if (message.responseCode == AckResponseCode.Timeout)
            {
                UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), LanguageManager.GetText(UITextKeys.UI_ERROR_CONNECTION_TIMEOUT.ToString()));
                if (onLoginFail != null)
                    onLoginFail.Invoke();
                return;
            }
            switch (message.responseCode)
            {
                case AckResponseCode.Error:
                    string errorMessage = string.Empty;
                    switch (message.error)
                    {
                        case ResponseUserLoginMessage.Error.AlreadyLogin:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_ALREADY_LOGGED_IN.ToString());
                            break;
                        case ResponseUserLoginMessage.Error.InvalidUsernameOrPassword:
                            errorMessage = LanguageManager.GetText(UITextKeys.UI_ERROR_INVALID_USERNAME_OR_PASSWORD.ToString());
                            break;
                    }
                    UISceneGlobal.Singleton.ShowMessageDialog(LanguageManager.GetText(UITextKeys.UI_LABEL_ERROR.ToString()), errorMessage);
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
