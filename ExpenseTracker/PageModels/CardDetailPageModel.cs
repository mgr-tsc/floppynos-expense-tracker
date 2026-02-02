using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models;

namespace ExpenseTracker.PageModels;

public partial class CardDetailPageModel : ObservableObject, IQueryAttributable
{
    private Card? _card;
    private readonly CardRepository _cardRepository;
    private readonly ProfileRepository _profileRepository;
    private readonly ModalErrorHandler _errorHandler;

    [ObservableProperty] private string _provider = string.Empty;

    [ObservableProperty] private string _alias = string.Empty;

    [ObservableProperty] private string _lastFourDigits = string.Empty;

    [ObservableProperty] private List<Profile> _profiles = [];

    [ObservableProperty] private Profile? _selectedProfile;

    [ObservableProperty] private int _profileIndex = -1;

    [ObservableProperty] bool _isBusy;

    private bool _canDelete;

    public bool CanDelete
    {
        get => _canDelete;
        set
        {
            _canDelete = value;
            DeleteCommand.NotifyCanExecuteChanged();
        }
    }

    public CardDetailPageModel(CardRepository cardRepository, ProfileRepository profileRepository,
        ModalErrorHandler errorHandler)
    {
        _cardRepository = cardRepository;
        _profileRepository = profileRepository;
        _errorHandler = errorHandler;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.ContainsKey("id"))
        {
            int id = Convert.ToInt32(query["id"]);
            LoadData(id).FireAndForgetSafeAsync(_errorHandler);
        }
        else
        {
            LoadProfiles().FireAndForgetSafeAsync(_errorHandler);
            _card = new();
        }
    }

    private async Task LoadProfiles() =>
        Profiles = await _profileRepository.ListAsync();

    private async Task LoadData(int id)
    {
        try
        {
            IsBusy = true;

            _card = await _cardRepository.GetAsync(id);

            if (_card.IsNullOrNew())
            {
                _errorHandler.HandleError(new Exception($"Card with id {id} could not be found."));
                return;
            }

            Provider = _card.Provider;
            Alias = _card.Alias;
            LastFourDigits = _card.LastFourDigits;

            Profiles = await _profileRepository.ListAsync();
            SelectedProfile = Profiles.FirstOrDefault(p => p.ID == _card.ProfileID);
            ProfileIndex = Profiles.FindIndex(p => p.ID == _card.ProfileID);
        }
        catch (Exception e)
        {
            _errorHandler.HandleError(e);
        }
        finally
        {
            IsBusy = false;
            CanDelete = !_card.IsNullOrNew();
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (_card is null)
        {
            _errorHandler.HandleError(new Exception("Card is null. Cannot Save."));
            return;
        }

        _card.Provider = Provider;
        _card.Alias = Alias;
        _card.LastFourDigits = LastFourDigits;
        _card.ProfileID = SelectedProfile?.ID ?? 0;
        await _cardRepository.SaveItemAsync(_card);

        await Shell.Current.GoToAsync("..");
        await AppShell.DisplayToastAsync("Card saved");
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private async Task Delete()
    {
        if (_card.IsNullOrNew())
        {
            await Shell.Current.GoToAsync("..");
            return;
        }

        await _cardRepository.DeleteItemAsync(_card);
        await Shell.Current.GoToAsync("..");
        await AppShell.DisplayToastAsync("Card deleted");
    }
}
