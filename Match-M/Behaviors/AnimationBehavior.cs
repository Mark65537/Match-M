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
                typeof(AnimationType),
                typeof(AnimationBehavior),
                new PropertyMetadata(AnimationType.None, OnAnimationChanged));

        public static void SetAnimation(UIElement element, AnimationType value)
            => element.SetValue(AnimationProperty, value);

        public static AnimationType GetAnimation(UIElement element)
            => (AnimationType)element.GetValue(AnimationProperty);

        private static void OnAnimationChanged(
            DependencyObject d,
            DependencyPropertyChangedEventArgs e)
        {
            if (d is not UIElement element)
                return;

            var animation = (AnimationType)e.NewValue;

            string? key = animation switch
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

            // Падение на заданное расстояние (до первого не нулевого): анимация из кода
            if (key == "MoveUpDownStoryboard" && element is FrameworkElement fe && fe.DataContext is Cell cell && cell.FallDistancePixels > 0)
            {
                var sb = new Storyboard();
                var duration = TimeSpan.FromMilliseconds(150 + (cell.FallDistancePixels / 2));
                var anim = new DoubleAnimation(0, cell.FallDistancePixels, new Duration(duration));
                Storyboard.SetTarget(anim, element);
                Storyboard.SetTargetProperty(anim, new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.Y)"));
                sb.Children.Add(anim);
                sb.Completed += (_, _) => AnimationCompleted?.Invoke(element, EventArgs.Empty);
                sb.Begin();
                return;
            }

            var storyboard = (Storyboard)AnimationsDictionary[key];
            storyboard = storyboard.Clone();

            Storyboard.SetTarget(storyboard, element);

            storyboard.Completed += (_, _) => AnimationCompleted?.Invoke(element, EventArgs.Empty);

            storyboard.Begin();
        }
    }
}
