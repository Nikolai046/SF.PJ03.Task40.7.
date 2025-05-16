using Android.App;
using Android.Content;

namespace SF.PJ03.Task40._7_.Platforms.Android.Helpers;

public static class PendingIntentRequester
{
    private static TaskCompletionSource<bool> _tcs;
    private static int _currentRequestCode; // Динамический код запроса

    public static Task<bool> RequestDeleteAsync(Activity activity, PendingIntent pendingIntent)
    {
        _tcs = new TaskCompletionSource<bool>();
        // Генерируем уникальный код запроса для каждого вызова, чтобы избежать коллизий,
        // если предыдущий запрос еще не завершился или _tcs не был сброшен.
        _currentRequestCode = new Random().Next(10000, 60000); // Допустимый диапазон для request codes

        try
        {
            activity.StartIntentSenderForResult(pendingIntent.IntentSender, _currentRequestCode, null, 0, 0, 0, null);
        }
        catch (IntentSender.SendIntentException sie)
        {
            System.Diagnostics.Debug.WriteLine($"PendingIntentRequester: SendIntentException - {sie.Message}");
            _tcs.SetException(sie);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"PendingIntentRequester: Exception - {ex.Message}");
            _tcs.SetException(ex);
        }
        return _tcs.Task;
    }

    public static void OnActivityResult(int requestCode, Result resultCode, Intent data)
    {
        // Проверяем, что это ответ на наш последний запрос и что TaskCompletionSource еще существует
        if (requestCode == _currentRequestCode && _tcs != null && !_tcs.Task.IsCompleted)
        {
            System.Diagnostics.Debug.WriteLine($"PendingIntentRequester.OnActivityResult: requestCode={requestCode}, resultCode={resultCode}");
            _tcs.SetResult(resultCode == Result.Ok);
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"PendingIntentRequester.OnActivityResult: Unhandled requestCode={requestCode} or TCS already completed/null.");
        }
    }
}