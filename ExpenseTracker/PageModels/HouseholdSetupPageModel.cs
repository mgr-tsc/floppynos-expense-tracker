using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ExpenseTracker.PageModels;

public partial class HouseholdSetupPageModel : ObservableObject
{
    [RelayCommand]
    private Task CreateHousehold() => Shell.Current.GoToAsync("createhousehold");

    [RelayCommand]
    private Task JoinHousehold() => Shell.Current.GoToAsync("joinhousehold");
}