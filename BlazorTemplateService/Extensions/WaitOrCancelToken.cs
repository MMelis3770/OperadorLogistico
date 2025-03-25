namespace BlazorTemplateService.Extensions
{
    public static class WaitOrCancel
    {
        public static async Task<T> WithTimeOutAsyncWithReturn<T>(int milliSeconds, Func<T> function)
        {
            using (var cts = new CancellationTokenSource())
            {
                cts.CancelAfter(milliSeconds);

                try
                {
                    return await Task.Run(function.Invoke, cts.Token);
                }
                catch (OperationCanceledException)
                {
                    throw new TimeoutException();
                }
            }
        }
    }
}
