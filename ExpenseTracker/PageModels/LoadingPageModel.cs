using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Services;

namespace ExpenseTracker.PageModels;

public partial class LoadingPageModel : ObservableObject
{
    private readonly SupabaseService _supabaseService;

    public LoadingPageModel(SupabaseService supabaseService)
    {
        _supabaseService = supabaseService;
    }

    [RelayCommand]
    private async Task Appearing()
    {
        await AppSettings.InitializeAsync();
        await _supabaseService.InitializeAsync();
        await Shell.Current.GoToAsync("//signin");
    }
}
