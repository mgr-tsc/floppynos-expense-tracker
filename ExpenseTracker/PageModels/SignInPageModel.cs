using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ExpenseTracker.PageModels;

public partial class SignInPageModel : ObservableObject
{
    [ObservableProperty] private string _email = string.Empty;

    [ObservableProperty] private string _password = string.Empty;

    [RelayCommand]
    private Task SignIn()
        => Shell.Current.GoToAsync("//main");
}
