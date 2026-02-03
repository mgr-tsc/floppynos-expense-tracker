using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ExpenseTracker.PageModels;

public partial class CardDetailPageModel : ObservableObject, IQueryAttributable
{
    private Card? _card;
    private readonly ModalErrorHandler _errorHandler;

    [ObservableProperty] private string _provider = string.Empty;

    [ObservableProperty] private string _alias = string.Empty;

    [ObservableProperty] private string _lastFourDigits = string.Empty;

    [ObservableProperty] private List<Profile> _profiles = [];

    [ObservableProperty] private Profile? _selectedProfile;

    [ObservableProperty] private int _profileIndex = -1;

    [ObservableProperty] bool _isBusy;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        
    }

}
