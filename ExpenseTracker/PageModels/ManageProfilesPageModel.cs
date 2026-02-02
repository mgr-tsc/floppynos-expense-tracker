using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models;

namespace ExpenseTracker.PageModels;

public partial class ManageProfilesPageModel : ObservableObject
{
    private readonly ProfileRepository _profileRepository;
    private readonly SeedDataService _seedDataService;

    [ObservableProperty] private ObservableCollection<Profile> _profiles = [];

    public ManageProfilesPageModel(ProfileRepository profileRepository, SeedDataService seedDataService)
    {
        _profileRepository = profileRepository;
        _seedDataService = seedDataService;
    }

    private async Task LoadData()
    {
        var profilesList = await _profileRepository.ListAsync();
        Profiles = new ObservableCollection<Profile>(profilesList);
    }

    [RelayCommand]
    private Task Appearing()
        => LoadData();

    [RelayCommand]
    private async Task SaveProfiles()
    {
        foreach (var profile in Profiles)
        {
            await _profileRepository.SaveItemAsync(profile);
        }

        await AppShell.DisplayToastAsync("Profiles saved");
        SemanticScreenReader.Announce("Profiles saved");
    }

    [RelayCommand]
    private async Task Reset()
    {
        Preferences.Default.Remove("is_seeded");
        await _seedDataService.LoadSeedDataAsync();
        Preferences.Default.Set("is_seeded", true);
        await Shell.Current.GoToAsync("//main");
    }
}
