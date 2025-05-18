using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace SF.PJ03.Task40._7_.Platforms.Android;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // Логика запроса разрешений
        // Для Android 13+ (API 33)
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
        {
            if (CheckSelfPermission(Manifest.Permission.ReadMediaImages) != Permission.Granted)
            {
                RequestPermissions([Manifest.Permission.ReadMediaImages], 1001);
            }
        }
        // Для Android <13
        else
        {
            if (CheckSelfPermission(Manifest.Permission.ReadExternalStorage) != Permission.Granted)
            {
                RequestPermissions([Manifest.Permission.ReadExternalStorage], 1002);
            }
        }
    }

    // Обрабатывает результат, полученный от другой Activity (например, от диалога подтверждения удаления).
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        // Передаем результат в обработчик
        SF.PJ03.Task40._7_.Platforms.Android.Helpers.PendingIntentRequester.OnActivityResult(requestCode, resultCode, data);
    }
}