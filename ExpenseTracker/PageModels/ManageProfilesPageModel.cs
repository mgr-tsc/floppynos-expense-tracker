using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ExpenseTracker.PageModels;

public partial class ManageProfilesPageModel : ObservableObject
{

    [ObservableProperty] private ObservableCollection<Profile> _profiles = [];

    public ManageProfilesPageModel()
    {
        
    }

    private async Task LoadData()
    {
        
    }

    [RelayCommand]
    private Task Appearing()
        => LoadData();

    [RelayCommand]
    private async Task SaveProfiles()
    {
        foreach (var profile in Profiles)
        {
            // await _profileRepository.SaveItemAsync(profile);
        }

        await AppShell.DisplayToastAsync("Profiles saved");
        SemanticScreenReader.Announce("Profiles saved");
    }

    [RelayCommand]
    private async Task Reset()
    {
        //Preferences.Default.Remove(AppSettings.SeedPreferencesKey);
        //await _seedDataService.LoadSeedDataAsync();
        //Preferences.Default.Set(AppSettings.SeedPreferencesKey, true);
        await Shell.Current.GoToAsync("//main");
    }
}
