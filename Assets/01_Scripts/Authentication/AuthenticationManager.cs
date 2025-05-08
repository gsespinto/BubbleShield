using System;
using System.Threading.Tasks;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.Events;

public class AuthenticationManager : MonoBehaviour
{
    [SerializeField] private UnityEvent onAuthenticated;

    private string Token;
    private string Error;

    private static AuthenticationManager instance;

	async void Awake()
	{
        if (instance != null){
            Destroy(this);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);

		try
		{
			await UnityServices.InitializeAsync();
            SetupEvents();

#if UNITY_EDITOR
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            onAuthenticated?.Invoke();
#elif UNITY_ANDROID
            LoginGooglePlayGames();
#endif
		}
		catch (Exception e)
		{
			Logger.Exception(e);
		}
	}

    // Setup authentication event handlers if desired
    void SetupEvents() {
        AuthenticationService.Instance.SignedIn += () => {
            // Shows how to get a playerID
            Logger.Info($"PlayerID: {AuthenticationService.Instance.PlayerId}");

            // Shows how to get an access token
            Logger.Info($"Access Token: {AuthenticationService.Instance.AccessToken}");
        };

        AuthenticationService.Instance.SignInFailed += (err) => {
            Logger.Error($"{ err.ErrorCode } - { err.Message } ");
        };

        AuthenticationService.Instance.SignedOut += () => {
            Logger.Info("Player signed out.");
        };

        AuthenticationService.Instance.Expired += () =>{
            Logger.Info("Player session could not be refreshed and expired.");
        };
    }

    public void LoginGooglePlayGames()
    {
        PlayGamesPlatform.Instance.Authenticate(async (success) =>
        {
            if (success == SignInStatus.Success)
            {
                Logger.Info("Login with Google Play games successful.");

                PlayGamesPlatform.Instance.RequestServerSideAccess(true, async code =>
                {
                    Logger.Info("Authorization code: " + code);
                    Token = code;
                    await SignInWithGooglePlayGamesAsync(Token);
                    onAuthenticated?.Invoke();
                    // This token serves as an example to be used for SignInWithGooglePlayGames
                });
            }
            else
            {
                Error = "Failed to retrieve Google play games authorization code";
                Logger.Info("Login Unsuccessful");
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                onAuthenticated?.Invoke();
            }
        });
    }

    async Task SignInWithGooglePlayGamesAsync(string authCode)
    {
        try
        {
            await AuthenticationService.Instance.SignInWithGooglePlayGamesAsync(authCode);
            Debug.Log("SignIn is successful.");
        }
        catch (AuthenticationException ex)
        {
            // Compare error code to AuthenticationErrorCodes
            // Notify the player with the proper error message
            Logger.Exception(ex);
        }
        catch (RequestFailedException ex)
        {
            // Compare error code to CommonErrorCodes
            // Notify the player with the proper error message
            Logger.Exception(ex);
        }
    }
}