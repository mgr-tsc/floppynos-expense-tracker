using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.PageModels;

public partial class SignInPageModel : ObservableObject
{
    private readonly ISigInInThirdParty _thirdParty;
    private readonly ILogger<SignInPageModel> _logger;

    public SignInPageModel(ISigInInThirdParty thirdParty, ILogger<SignInPageModel> logger)
    {
        _thirdParty = thirdParty;
        _logger = logger;
    }

    [RelayCommand]
    private async Task SignInWithGoogle()
    {
        try
        {
            _logger.LogDebug("SignInWithGoogle: starting third party sign-in");
            var result = await _thirdParty.SignInWithGoogleAsync();
            if (result is not null)
            {
                _logger.LogInformation("SignInWithGoogle: sign-in successful, navigating to main");
                await Shell.Current.GoToAsync("//main");
            }
            else
            {
                _logger.LogWarning("SignInWithGoogle: sign-in returned null result");
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "SignInWithGoogle failed");
            Console.WriteLine(e);
        }
    }
}
