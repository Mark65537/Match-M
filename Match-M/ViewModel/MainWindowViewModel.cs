using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Match_M.Model;
using Match_M.Services;

namespace Match_M.ViewModel;

public class MainWindowViewModel : ObservableObject
{

    public MainWindowViewModel()
    {
        GameStateService = new GameStateService
        {
#if DEBUG
            CurrentState = GameState.InGame
#else
            CurrentState = GameState.Menu
#endif
        };

        MenuVM = new MenuViewModel(GameStateService);
        GameVM = new GameViewModel(GameStateService);
        GameOverVM = new GameOverViewModel(GameStateService);

#if DEBUG
        ToMenuCommand = new RelayCommand(() => GameStateService.CurrentState = GameState.Menu);
        ToInGameCommand = new RelayCommand(() => GameStateService.CurrentState = GameState.InGame);
        ToGameOverCommand = new RelayCommand(() => GameStateService.CurrentState = GameState.GameOver);
#endif

        GameStateService.StateChanged += GameState_PropertyChanged;
        UpdateCurrentViewModel();
    }

    public GameStateService GameStateService { get; }
    public MenuViewModel MenuVM { get; }
    public GameViewModel GameVM { get; }
    public GameOverViewModel GameOverVM { get; }

#if DEBUG
    public RelayCommand ToMenuCommand { get; }
    public RelayCommand ToInGameCommand { get; }
    public RelayCommand ToGameOverCommand { get; }
#endif

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
