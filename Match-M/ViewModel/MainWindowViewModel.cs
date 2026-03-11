using CommunityToolkit.Mvvm.ComponentModel;
using Match_M.Model;
using System.ComponentModel;

namespace Match_M.ViewModel;

public class MainWindowViewModel : ObservableObject
{
    public GameStateService GameStateService { get; }

    public MainWindowViewModel()
    {
        GameStateService = new GameStateService
        {
            CurrentState = GameState.Menu
        };

        MenuVM = new MenuViewModel(GameStateService);
        GameVM = new GameViewModel(GameStateService);
        GameOverVM = new GameOverViewModel(GameStateService);

        GameStateService.PropertyChanged += GameStateService_PropertyChanged;
        UpdateCurrentViewModel();
    }

    public MenuViewModel MenuVM { get; }
    public GameViewModel GameVM { get; }
    public GameOverViewModel GameOverVM { get; }

    private ObservableObject? _currentViewModel;
    public ObservableObject? CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    private void GameStateService_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(GameStateService.CurrentState))
            return;

        UpdateCurrentViewModel();
    }

    private void UpdateCurrentViewModel()
    {
        CurrentViewModel = GameStateService.CurrentState switch
        {
            GameState.Menu => MenuVM,
            GameState.InGame => GameVM,
            GameState.GameOver => GameOverVM,
            _ => MenuVM
        };
    }

}
