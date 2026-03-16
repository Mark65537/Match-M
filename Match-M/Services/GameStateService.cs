using CommunityToolkit.Mvvm.ComponentModel;
using Match_M.Model;

namespace Match_M.Services;

/// <summary>
/// Класс обертка для удобного переключения состояний
/// </summary>
public class GameStateService : ObservableObject
{
    private GameState _currentState;

    /// <summary>
    /// Событие изменения состояния
    /// </summary>
    public event Action? StateChanged;

    public GameState CurrentState
    {
        get => _currentState;
        set
        {
            if (SetProperty(ref _currentState, value))
            {
                StateChanged?.Invoke();
            }
        }
    }
}
