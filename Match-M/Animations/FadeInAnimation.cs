namespace Match_M.Animations;
/// <summary>
/// Собирает всю информацию для запуска анимации FadeIn.
/// </summary>
public class FadeInAnimation : ICellAnimation
{
    public FadeInAnimation() { }
    public FadeInAnimation(TimeSpan duration) => Duration = duration;
    public AnimationType Type => AnimationType.FadeIn;
    public TimeSpan Duration { get; init; } = TimeSpan.FromMilliseconds(200);
}
