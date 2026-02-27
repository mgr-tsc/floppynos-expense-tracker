using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ExpenseTracker.Models.Supabase;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.PageModels;

public partial class PaymentDetailPageModel : ObservableObject, IQueryAttributable
{
    private PaymentDto? _payment;
    private long _householdId;

    private readonly PaymentRepository _paymentRepository;
    private readonly HouseholdRepository _householdRepository;
    private readonly SupabaseService _supabaseService;
    private readonly ModalErrorHandler _errorHandler;
    private readonly ILogger<PaymentDetailPageModel> _logger;

    private static readonly string[] PaymentMethodDisplayNames = ["Cash", "Check", "PayPal", "Zelle", "Apple Pay"];
    private static readonly string[] PaymentMethodDbValues = ["cash", "check", "transfer_paypal", "transfer_zelle", "transfer_applepay"];

    [ObservableProperty]
    private decimal _amount;

    [ObservableProperty]
    private DateTime _date = DateTime.Today;

    [ObservableProperty]
    private int _selectedMethodIndex;

    [ObservableProperty]
    private List<string> _paymentMethods = [.. PaymentMethodDisplayNames];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isExisting;

    [ObservableProperty]
    private bool _canApprove;

    [ObservableProperty]
    private bool _isOwner;

    /// <summary>True when form fields should be interactive (new payment or owner editing a rejected one).</summary>
    [ObservableProperty]
    private bool _isEditable;

    /// <summary>True when an existing rejected payment can be re-submitted by its owner.</summary>
    [ObservableProperty]
    private bool _canResubmit;

    [ObservableProperty]
    private string _pageTitle = "NEW PAYMENT";

    public PaymentDetailPageModel(
        PaymentRepository paymentRepository,
        HouseholdRepository householdRepository,
        SupabaseService supabaseService,
        ModalErrorHandler errorHandler,
        ILogger<PaymentDetailPageModel> logger)
    {
        _paymentRepository = paymentRepository;
        _householdRepository = householdRepository;
        _supabaseService = supabaseService;
        _errorHandler = errorHandler;
        _logger = logger;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var idValue) && long.TryParse(idValue?.ToString(), out var paymentId) && paymentId > 0)
        {
            _logger.LogInformation("Loading existing payment {Id}", paymentId);
            LoadExistingPayment(paymentId).FireAndForgetSafeAsync(_errorHandler);
        }
        else
        {
            _payment = new PaymentDto();
            IsExisting = false;
            CanApprove = false;
            IsOwner = true;
            IsEditable = true;
            CanResubmit = false;
            _logger.LogInformation("Creating new payment");
            LoadFormData().FireAndForgetSafeAsync(_errorHandler);
        }
    }

    private async Task LoadExistingPayment(long paymentId)
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            _payment = await _paymentRepository.GetAsync(paymentId);
            if (_payment is null)
            {
                ErrorMessage = "Payment not found";
                return;
            }

            IsExisting = true;
            PageTitle = "VIEW PAYMENT";

            // Populate form fields
            Amount = _payment.Amount;
            Date = _payment.PaymentDate ?? DateTime.Today;
            SelectedMethodIndex = Array.IndexOf(PaymentMethodDbValues, _payment.PaymentMethod);
            if (SelectedMethodIndex < 0) SelectedMethodIndex = 0;

            // Determine ownership and editability
            var currentUserId = _supabaseService.GetCurrentUserId();
            IsOwner = _payment.IsOwnPayment(currentUserId ?? "");
            var isRejected = string.Equals(_payment.Status?.Trim(), "rejected", StringComparison.OrdinalIgnoreCase);
            CanApprove = !string.IsNullOrEmpty(currentUserId)
                         && !IsOwner
                         && string.Equals(_payment.Status?.Trim(), "pending", StringComparison.OrdinalIgnoreCase);
            IsEditable = IsOwner && isRejected;
            CanResubmit = IsEditable;

            _logger.LogInformation("Loaded payment {Id}, IsOwner={IsOwner}, CanApprove={CanApprove}, CanResubmit={CanResubmit}",
                paymentId, IsOwner, CanApprove, CanResubmit);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load payment {Id}", paymentId);
            ErrorMessage = "Failed to load payment";
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadFormData()
    {
        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            var userId = _supabaseService.GetCurrentUserId();
            if (string.IsNullOrEmpty(userId))
            {
                ErrorMessage = "No authenticated user";
                return;
            }

            var household = await _householdRepository.GetByUserIdAsync(userId);
            if (household is null)
            {
                ErrorMessage = "No household found. Please join or create one first.";
                return;
            }
            _householdId = household.Id;

            _logger.LogInformation("Payment form loaded, Household: {Id}", _householdId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load payment form data");
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Approve()
    {
        if (_payment is null) return;

        try
        {
            IsBusy = true;
            _payment.Status = "approved";
            await _paymentRepository.SaveAsync(_payment);
            await Shell.Current.GoToAsync("..");
            await AppShell.DisplayToastAsync("Payment approved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to approve payment");
            ErrorMessage = "Failed to approve payment";
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Reject()
    {
        if (_payment is null) return;

        try
        {
            IsBusy = true;
            _payment.Status = "rejected";
            await _paymentRepository.SaveAsync(_payment);
            await Shell.Current.GoToAsync("..");
            await AppShell.DisplayToastAsync("Payment rejected");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to reject payment");
            ErrorMessage = "Failed to reject payment";
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (Amount <= 0)
        {
            ErrorMessage = "Please enter a valid amount";
            return;
        }

        if (_payment is null)
        {
            ErrorMessage = "Payment data is missing";
            return;
        }

        try
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            _payment.Amount = Amount;
            _payment.PaymentDate = Date;
            _payment.PaymentMethod = PaymentMethodDbValues[SelectedMethodIndex];
            _payment.UserIdFk = _supabaseService.GetCurrentUserId()!;
            _payment.HouseholdIdFk = _householdId;

            // Re-submitting a rejected payment resets it to pending for partner review
            if (CanResubmit)
                _payment.Status = "pending";

            _logger.LogInformation("Saving payment: Amount={Amount}, Method={Method}, CanResubmit={CanResubmit}",
                Amount, _payment.PaymentMethod, CanResubmit);

            await _paymentRepository.SaveAsync(_payment);

            await Shell.Current.GoToAsync("..");
            await AppShell.DisplayToastAsync(CanResubmit ? "Payment re-submitted" : "Payment saved");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save payment");
            ErrorMessage = "Failed to save payment. Please try again.";
            _errorHandler.HandleError(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
