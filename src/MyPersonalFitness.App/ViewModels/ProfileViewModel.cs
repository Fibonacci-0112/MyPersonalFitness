using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyPersonalFitness.Core.Interfaces;
using MyPersonalFitness.Core.Models;

namespace MyPersonalFitness.App.ViewModels;

public partial class ProfileViewModel : ObservableObject
{
    private readonly IUserProfileRepository _userRepo;

    [ObservableProperty]
    private UserProfile _profile = new()
    {
        DateOfBirth = DateTime.Today.AddYears(-30),
        Gender = Gender.PreferNotToSay
    };

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _statusMessage;

    public ProfileViewModel(IUserProfileRepository userRepo)
    {
        _userRepo = userRepo;
    }

    [RelayCommand]
    public async Task LoadAsync()
    {
        IsBusy = true;
        var existing = await _userRepo.GetCurrentUserAsync();
        if (existing != null) Profile = existing;
        IsBusy = false;
    }

    [RelayCommand]
    public async Task SaveAsync()
    {
        IsBusy = true;
        if (Profile.Id == 0)
        {
            Profile.StartDate = DateTime.UtcNow;
            await _userRepo.AddAsync(Profile);
        }
        else
        {
            Profile.UpdatedAt = DateTime.UtcNow;
            await _userRepo.UpdateAsync(Profile);
        }
        StatusMessage = "Profile saved!";
        IsBusy = false;
    }
}
