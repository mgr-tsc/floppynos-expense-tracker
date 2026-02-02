using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ExpenseTracker.PageModels;

public partial class LoadingPageModel : ObservableObject
{
    [RelayCommand]
    private async Task Appearing()
    {
        await Task.Delay(1500);
        await Shell.Current.GoToAsync("//signin");
    }
}
