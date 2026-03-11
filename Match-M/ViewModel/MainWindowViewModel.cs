using CommunityToolkit.Mvvm.ComponentModel;
using Match_M.Model;

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

        Menu = new MenuViewModel(GameStateService);
        Game = new GameViewModel(GameStateService);
        GameOver = new GameOverViewModel(GameStateService);
    }

    public MenuViewModel Menu { get; }
    public GameViewModel Game { get; }
    public GameOverViewModel GameOver { get; }

}
