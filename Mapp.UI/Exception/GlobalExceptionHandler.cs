using System.Threading.Tasks;
using Shmap.CommonServices;
using Shmap.CommonServices.Logging;

namespace Shmap.UI.Exception;

public interface IGlobalExceptionHandler
{
    Task HandleExceptionAsync(System.Exception exception);
}

public class GlobalExceptionHandler : IGlobalExceptionHandler
{
    private readonly IDialogService _dialogService;
    //private readonly IAppCenterIntegration _appCenter;
    private readonly ILogger _logger;

    public GlobalExceptionHandler(IDialogService dialogService, ILogger logger)
    {
        _dialogService = dialogService;
        //_appCenter = appCenter;
        _logger = logger;
    }

    public async Task HandleExceptionAsync(System.Exception exception)
    {
        try
        {
            _dialogService.ShowMessage("Unexpected error occurred: " + exception.Message);
        }
        finally
        {
            _logger.Error(exception);
            //_appCenter.TrackException(exception);
        }
    }
}