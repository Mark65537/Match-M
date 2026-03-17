using Match_M.Animations;
using Match_M.Model;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Match_M.Behaviors
{
    public static class AnimationBehavior
    {
        public static event EventHandler? AnimationCompleted;

        private static readonly ResourceDictionary AnimationsDictionary =
            new()
            {
                Source = new Uri("/Match-M;component/View/Animations.xaml", UriKind.Relative)
            };

        public static readonly DependencyProperty AnimationProperty =
            DependencyProperty.RegisterAttached(
                "Animation",
                typeof(ICellAnimation),
                typeof(AnimationBehavior),
                new PropertyMetadata(null, OnAnimationChanged));

        public static void SetAnimation(UIElement element, ICellAnimation? value)
            => element.SetValue(AnimationProperty, value);

        public static ICellAnimation? GetAnimation(UIElement element)
            => (ICellAnimation?)element.GetValue(AnimationProperty);

        private static void OnAnimationChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is not UIElement element)
                return;

            var animation = (ICellAnimation?)e.NewValue;
            var type = animation?.Type ?? AnimationType.None;

            string? key = type switch
            {
                AnimationType.None => null,
                AnimationType.MoveUpDown => "MoveUpDownStoryboard",
                AnimationType.FadeOut => "FadeOutStoryboard",
                AnimationType.FadeIn => "FadeInStoryboard",
                _ => null
            };

            if (key == null)
            {
                // Снимаем анимацию с Opacity (WPF держит 0 после FadeOut), чтобы вернуть значение 1
                element.BeginAnimation(UIElement.OpacityProperty, null);
                element.RenderTransform = new TranslateTransform(0, 0);
                return;
            }

            if (element.RenderTransform is not TranslateTransform)
                element.RenderTransform = new TranslateTransform();

            if (type == AnimationType.MoveUpDown)
            {
                var move = animation as MoveAnimation;

                double toX = move?.ToX ?? 0;
                double toY = move?.ToY ?? 0;

                // If ToY isn't provided, derive from DeltaRows (in cells).
                if (toY == 0 &&
                    move is not null &&
                    move.DeltaRows != 0 &&
                    element is FrameworkElement fe)
                {
                    var pitch = GetCellPitchPixels(fe);
                    toY = move.DeltaRows * pitch;
                }

                var duration = move?.Duration ?? TimeSpan.FromMilliseconds(200);

                var sb = new Storyboard();

                var animY = new DoubleAnimation(0, toY, new Duration(duration));
                Storyboard.SetTarget(animY, element);
                Storyboard.SetTargetProperty(animY, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
                sb.Children.Add(animY);

                if (toX != 0)
                {
                    var animX = new DoubleAnimation(0, toX, new Duration(duration));
                    Storyboard.SetTarget(animX, element);
                    Storyboard.SetTargetProperty(animX, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));
                    sb.Children.Add(animX);
                }

                sb.Completed += (_, _) => AnimationCompleted?.Invoke(element, EventArgs.Empty);
                sb.Begin();
                return;
            }

            var storyboard = (Storyboard)AnimationsDictionary[key];
            storyboard = storyboard.Clone();

            // ВАЖНО: цель нужно назначать КАЖДОЙ анимации внутри storyboard.
            // Storyboard.SetTarget(storyboard, element) сам по себе не задаёт Target дочерним Timeline.
            foreach (var child in storyboard.Children)
                Storyboard.SetTarget(child, element);

            if (animation is not null)
                ApplyDurationOverrides(storyboard, animation.Duration);
            storyboard.Completed += (_, _) => AnimationCompleted?.Invoke(element, EventArgs.Empty);
            storyboard.Begin();
        }

        private static void ApplyDurationOverrides(Timeline timeline, TimeSpan duration)
        {
            if (duration <= TimeSpan.Zero)
                return;

            timeline.Duration = new Duration(duration);

            if (timeline is Storyboard sb)
            {
                foreach (var child in sb.Children)
                    ApplyDurationOverrides(child, duration);
            }
        }

        private static double GetCellPitchPixels(FrameworkElement element)
        {
            // Height + Margin даёт "шаг" в UniformGrid (для вашего Border Height=75, Margin=3 => ~81).
            // Если ActualHeight ещё не измерен, подстрахуемся минимальным значением.
            var h = element.ActualHeight;
            if (h <= 0)
                h = element.Height;
            if (double.IsNaN(h) || h <= 0)
                h = 75;

            var m = element.Margin;
            return h + m.Top + m.Bottom;
        }
    }
}
