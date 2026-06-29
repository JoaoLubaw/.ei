using Android.App;
using Android.Content;
using Android.OS;
using AndroidX.Core.App;
using Firebase.Messaging;
using System;

namespace Pontuei.App.Platforms.Android.Services
{
    // O Exported = true e o IntentFilter dizem ao Android que esta classe é a responsável por ouvir o Firebase
    [Service(Exported = true)]
    [IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
    public class PontueiFirebaseMessagingService : FirebaseMessagingService
    {
        // Este método é engatilhado automaticamente quando o celular recebe um push e o app está em segundo plano/fechado
        public override void OnMessageReceived(RemoteMessage message)
        {
            base.OnMessageReceived(message);

            // 1. Extraímos os dados que a sua API enviou pelo Firebase
            var notification = message.GetNotification();
            string title = notification?.Title ?? "Nova Notificação";
            string body = notification?.Body ?? "Você tem uma nova atualização no Pontuei!";

            // (Opcional) Se a sua API enviar como "Data Payload" em vez de "Notification Payload"
            if (message.Data.TryGetValue("title", out var dataTitle)) title = dataTitle;
            if (message.Data.TryGetValue("body", out var dataBody)) body = dataBody;

            // 2. Disparamos o alerta visual no sistema operacional
            SendLocalNotification(title, body);
        }

        // Este método é chamado sempre que o Firebase gera um novo token para este aparelho
        public override void OnNewToken(string token)
        {
            base.OnNewToken(token);
            // Idealmente, você pode disparar um evento ou usar injeção aqui para avisar a API
            System.Diagnostics.Debug.WriteLine($"[FCM] Novo token gerado: {token}");
        }

        private void SendLocalNotification(string title, string body)
        {
            // Intent para abrir o aplicativo (MainActivity) quando o usuário tocar na notificação
            var intent = new Intent(this, typeof(MainActivity));
            intent.AddFlags(ActivityFlags.ClearTop);

            // PendingIntentFlags.Immutable é obrigatório nas versões mais recentes do Android
            var pendingIntent = PendingIntent.GetActivity(this, 0, intent, PendingIntentFlags.Immutable | PendingIntentFlags.OneShot);

            var channelId = "pontuei_default_channel";
            var notificationBuilder = new NotificationCompat.Builder(this, channelId)
                .SetSmallIcon(Resource.Mipmap.appicon) // Se o seu ícone no Android tiver outro nome, altere aqui
                .SetContentTitle(title)
                .SetContentText(body)
                .SetAutoCancel(true) // A notificação some quando o usuário clica
                .SetPriority(NotificationCompat.PriorityHigh)
                .SetContentIntent(pendingIntent);

            var notificationManager = NotificationManagerCompat.From(this);

            // A partir do Android 8.0 (Oreo), é obrigatório criar um "Canal de Notificação"
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channel = new NotificationChannel(channelId, "Notificações Pontuei", NotificationImportance.High)
                {
                    Description = "Canal principal para alertas de pontos e transações"
                };
                notificationManager.CreateNotificationChannel(channel);
            }

            // O ID randômico garante que as notificações não se sobrescrevam se chegarem várias juntas
            int notificationId = new Random().Next();
            notificationManager.Notify(notificationId, notificationBuilder.Build());
        }
    }
}