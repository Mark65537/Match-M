using Match_M.Animations;
using System.Windows;
using System.Windows.Media.Animation;

namespace Match_M.Behaviors
{
    public static class AnimationBehavior
    {
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

            string key = animation switch
            {
                AnimationType.None => null,
                AnimationType.MoveUpDown => "MoveUpDownStoryboard",
                AnimationType.FadeOut => "FadeOutStoryboard",
                _ => null
            };

            if (key == null)
                return;

            var storyboard =
                (Storyboard)AnimationsDictionary[key];

            storyboard = storyboard.Clone();

            Storyboard.SetTarget(storyboard, element);

            storyboard.Begin();
        }
    }
}
