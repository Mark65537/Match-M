using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Match_M.Model;
using Match_M.Services;

namespace Match_M.ViewModel;

public sealed class GameOverViewModel : ObservableObject
{

    public GameOverViewModel(GameStateService gameStateService)
    {
        GameOverOkCommand = new RelayCommand(() =>
        {
            gameStateService.CurrentState = GameState.Menu;
        });
    }

    public RelayCommand GameOverOkCommand { get; }
}

