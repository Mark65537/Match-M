namespace Match_M.Animations;

public enum AnimationType
{
    /// <summary>
    /// Анимация не запускается.
    /// </summary>
    None,

    /// <summary>
    /// Перемещение элемента вверх-вниз (ключ MoveUpDownStoryboard в Animations.xaml).
    /// </summary>
    MoveUpDown,

    /// <summary>
    /// Краткое затухание.
    /// </summary>
    FadeOut
}

