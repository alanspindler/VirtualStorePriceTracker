class VirtualStoresPriceTracker : Functions.Functions
{
    public static async Task Main()
    {
        int IdLogExecution = 0;
        while (true)
        {
            try
            {
                IdLogExecution = InsertExecutionLogStart();
                CloseChromiumAndNodeProcesses();
                var updateShortenedUrls = ExpandAndSaveShortenedUrls();
                var fillTask = FillEmptyProductNames();
                var updatePriceTask = UpdateProductPrices();
                var notifyUsersTask = CheckAndNotifyUsersAsync();
                var sendWelcomeEmailTask = NotifyUsersWithWelcomeEmailAsync();
                await Task.WhenAll(fillTask, updatePriceTask, notifyUsersTask, sendWelcomeEmailTask);
                UpdateExecutionLogSuccess(IdLogExecution);
            }

            catch (Exception ex)
            {
                UpdateExecutionLogError(IdLogExecution, ex.ToString());
                await CheckLogsAndRestartIfNeeded();
            }

            await Task.Delay(TimeSpan.FromMinutes(1));
        }
    }
}
