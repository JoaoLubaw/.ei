using Android.App;
using Android.Runtime; // Necessário para o .JavaCast<T>()
using AndroidX.Core.Content; // Necessário para o ContextCompat
using AndroidX.Credentials;
using AndroidX.Credentials.Exceptions;
using Google.Android.Libraries.Identity.GoogleId;
using System;
using System.Threading.Tasks;

namespace Pontuei.App.Platforms.Android;

public static class GoogleAuthService
{
    public static Task<string?> GetGoogleIdTokenAsync(Activity activity)
    {
        TaskCompletionSource<string?> tcs = new TaskCompletionSource<string?>();

        GetGoogleIdOption googleIdOption = new GetGoogleIdOption.Builder()
            .SetFilterByAuthorizedAccounts(false)
            .SetServerClientId(AppConfig.GoogleWebClientId)
            .Build();

        GetCredentialRequest request = new GetCredentialRequest.Builder()
            .AddCredentialOption(googleIdOption)
            .Build();

        // CORREÇÃO 1: CredentialManager.Create retorna a interface ICredentialManager
        ICredentialManager credentialManager = CredentialManager.Create(activity);

        // CORREÇÃO 2: Utilizar ContextCompat para pegar o MainExecutor de forma segura
        // Isso evita o erro de colisão de namespace com "Pontuei.App"
        var mainExecutor = ContextCompat.GetMainExecutor(activity);

        credentialManager.GetCredentialAsync(
                    activity,
                    request,
                    null,
                    mainExecutor,
                    new CredentialManagerCallback(tcs)
                );

        return tcs.Task;
    }

    private sealed class CredentialManagerCallback : Java.Lang.Object, ICredentialManagerCallback
    {
        private readonly TaskCompletionSource<string?> _tcs;

        public CredentialManagerCallback(TaskCompletionSource<string?> tcs)
        {
            _tcs = tcs;
        }

        public void OnResult(Java.Lang.Object result)
        {
            if (result is not GetCredentialResponse response)
            {
                _tcs.TrySetResult(null);
                return;
            }

            if (response.Credential is CustomCredential customCred
                && customCred.Type == GoogleIdTokenCredential.TypeGoogleIdTokenCredential)
            {
                GoogleIdTokenCredential googleCred = GoogleIdTokenCredential.CreateFrom(customCred.Data);
                _tcs.TrySetResult(googleCred.IdToken);
            }
            else
            {
                _tcs.TrySetResult(null);
            }
        }

        public void OnError(Java.Lang.Object error)
        {
            string message = "Google credential error";

            // CORREÇÃO 3: Tratar o cast de Java.Lang.Object para uma Exceção usando JavaCast
            try
            {
                var credEx = error.JavaCast<GetCredentialException>();
                message = credEx.Message ?? message;
            }
            catch
            {
                message = error?.ToString() ?? message;
            }

            _tcs.TrySetException(new Exception(message));
        }
    }
}