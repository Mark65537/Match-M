namespace Match_M.Animations;
/// <summary>
/// Собирает всю информацию для запуска анимации FadeOut.
/// </summary>
public class FadeOutAnimation : ICellAnimation
{
    public FadeOutAnimation() { }
    public FadeOutAnimation(TimeSpan duration) => Duration = duration;
    public AnimationType Type => AnimationType.FadeOut;
    public TimeSpan Duration { get; init; } = TimeSpan.FromMilliseconds(200);
}
