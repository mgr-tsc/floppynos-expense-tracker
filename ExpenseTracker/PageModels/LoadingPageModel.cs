using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ExpenseTracker.PageModels;

public partial class LoadingPageModel : ObservableObject
{
    [RelayCommand]
    private async Task Appearing()
    {
        await AppSettings.InitializeAsync();
        await Shell.Current.GoToAsync("//signin");
    }
}
