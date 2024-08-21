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
                var fillTask = FillEmptyProductNames();
                var updatePriceTask = UpdateProductPrice();
                var notifyUsersTask = CheckAndNotifyUsersAsync();
                await Task.WhenAll(fillTask, updatePriceTask, notifyUsersTask);

                // Atualiza o registro de execução como sucesso
                UpdateExecutionLogSuccess(IdLogExecution);
            }

            catch (Exception ex)
            {
                UpdateExecutionLogError(IdLogExecution, ex.ToString());
            }

            //await page.RouteAsync("**/*", (route) =>
            //{
            //    var url = route.Request.Url;
            //    if (url.Contains("https://aax-us-east-retail-direct.amazon.com/e/xsp/getAd?placementId") || url.Contains("https://unagi.amazon.com.br/1/events/com.amazon.csm.csa.prod") || url.Contains("https://completion.amazon.com.br/api/2017/suggestions") || url.Contains("https://unagi-na.amazon.com/1/events/com.amazon.eel.SponsoredProductsEventTracking.prod"))
            //    {
            //        return route.AbortAsync();
            //    }
            //    return route.ContinueAsync();
            //});

            //await page.RouteAsync("https://www.amazon.com.br/dram/renderLazyLoaded", (route) =>
            //{
            //    return route.AbortAsync();
            //});
            //await page.RouteAsync("https://aax-us-east-retail-direct.amazon.com/e/xsp/getAd?placementId", (route) =>
            //{
            //    return route.AbortAsync();
            //});

            await Task.Delay(TimeSpan.FromMinutes(10));
        }
    }
}
