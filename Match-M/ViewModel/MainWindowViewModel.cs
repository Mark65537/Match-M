using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;

namespace Match_M.ViewModel;

public class MainWindowViewModel : ObservableObject
{
    private bool _isMainMenuVisible = true;
    private bool _isGameScreenVisible;

    public MainWindowViewModel()
    {
        PlayCommand = new RelayCommand(OnPlay);
    }

    public Visibility MainMenuVisibility =>
        _isMainMenuVisible ? Visibility.Visible : Visibility.Collapsed;

    public Visibility GameScreenVisibility =>
        _isGameScreenVisible ? Visibility.Visible : Visibility.Collapsed;

    public RelayCommand PlayCommand { get; }

    private void OnPlay()
    {
        _isMainMenuVisible = false;
        _isGameScreenVisible = true;
    }
}
