using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Match_M.Model;

namespace Match_M.ViewModel;

public sealed class MenuViewModel : ObservableObject
{

    public MenuViewModel(GameStateService gameStateService)
    {
        PlayCommand = new RelayCommand(() =>
        {
            gameStateService.CurrentState = GameState.InGame;
        });
    }

    public RelayCommand PlayCommand { get; }
}

