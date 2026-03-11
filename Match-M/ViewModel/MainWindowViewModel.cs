using CommunityToolkit.Mvvm.ComponentModel;
using Match_M.Model;

namespace Match_M.ViewModel;

public class MainWindowViewModel : ObservableObject
{

    public MainWindowViewModel()
    {
        GameStateService = new GameStateService
        {
            CurrentState = GameState.Menu
        };

        MenuVM = new MenuViewModel(GameStateService);
        GameVM = new GameViewModel(GameStateService);
        GameOverVM = new GameOverViewModel(GameStateService);

        GameStateService.StateChanged += GameState_PropertyChanged;
        UpdateCurrentViewModel();
    }

    public GameStateService GameStateService { get; }
    public MenuViewModel MenuVM { get; }
    public GameViewModel GameVM { get; }
    public GameOverViewModel GameOverVM { get; }

    private ObservableObject? _currentViewModel;
    public ObservableObject? CurrentViewModel
    {
        get => _currentViewModel;
        private set => SetProperty(ref _currentViewModel, value);
    }

    private void GameState_PropertyChanged()
    {
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
