namespace Lisa.Services;

public class SentryService
{
    public void CaptureException(Exception exception)
    {
        SentrySdk.CaptureException(exception);
    }

    public void CaptureMessage(string message)
    {
        SentrySdk.CaptureMessage(message);
    }
}