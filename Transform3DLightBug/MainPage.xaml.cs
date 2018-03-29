using Emilie.UWP.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace Transform3DLightBug
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            LoadItems();
        }

        void LoadItems()
        {
            // Not pulling in a WrapPanel for this demo, so manually create a grid

            Style style = this.Resources["SampleButtonStyle"] as Style;
            double x = 0;
            double y = 0;
            for (int i = 0; i < 5; i++)
            {
                x = 0;

                for (int j = 0; j < 5; j++)
                {
                    Button b = new Button { Style = style };
                    Canvas.SetTop(b, y);
                    Canvas.SetLeft(b, x);
                    Host.Children.Add(b);

                    x += (b.Width + 10);
                }

                y += (150 + 10);
            }

            Host.Width = x;
            Host.Height = y;
        }


        private void AddLights_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in Host.Children)
            {
                child.Lights.Add(new SampleLight());
            }
        }

        private void RemoveTransforms_Click(object sender, RoutedEventArgs e)
        {
            Host.Transform3D = null;
            foreach (var child in Host.Children)
            {
                child.Transform3D = null;
            }
        }

        private void RemoveAllLights_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in Host.Children)
            {
                child.Lights.Clear();
            }
        }

        private void DepthAnimate_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb = CreateDepth3DIn(
                                Host.Children.Cast<FrameworkElement>().OrderBy(f => Guid.NewGuid()),
                                this,
                                500,
                                TimeSpan.FromMilliseconds(20),
                                customEase: new ExponentialEase { Exponent = 6 });
            
            sb.Begin();
        }









        public Storyboard CreateDepth3DIn(
            IEnumerable<FrameworkElement> items,
            FrameworkElement container = null,
            double startDepth = -300,
            TimeSpan? customStagger = null,
            TimeSpan? customOpacityDuration = null,
            TimeSpan? customElementDuration = null,
            EasingFunctionBase customEase = null)
        {
            // 0. Prepare ourselves a nice list of visible framework elements to animate
            var _items = items.ToList();

            // 1. Ensure perspective transform on the target parent to allow depth animations
            //    to work
            container?.GetPerspectiveTransform3D();

            // 2. Create the return storyboard, and the properties we're going to use
            var sb = new Storyboard();
            var startOffset = TimeSpan.FromSeconds(0);
            var staggerTime = customStagger != null && customStagger.HasValue
                ? customStagger.Value
                : TimeSpan.FromMilliseconds(60);
            var duration = customElementDuration != null && customElementDuration.HasValue
                ? customElementDuration.Value
                : TimeSpan.FromMilliseconds(500);
            var durationOpacity = customOpacityDuration != null && customOpacityDuration.HasValue
                ? customOpacityDuration.Value
                : TimeSpan.FromMilliseconds(300);

            // 3. Now let's build the storyboard!
            for (var i = 0; i < _items.Count; i++)
            {
                // 3.0. Get the item and it's opacity 
                var item = _items[i];
                item.GetCompositeTransform3D();

                double targetOpacity = 1;// item.Opacity;
                item.Opacity = 0;

                // 3.1. Check AddedDelay
                startOffset = startOffset.Add(Animation.GetAddedDelay(item));

                // 3.2. Animate the opacity
                sb.CreateTimeline<DoubleAnimationUsingKeyFrames>(item, TargetProperty.Opacity)
                    .AddEasingDoubleKeyFrame(TimeSpan.Zero, 0)
                    .AddEasingDoubleKeyFrame(startOffset, 0)
                    .AddSplineDoubleKeyFrame(startOffset.Add(durationOpacity), targetOpacity, KeySplines.PerspectiveZoomOpacity);

                // 3.3. Animate the 3D depth translation
                if (startDepth != 0)
                {
                    var dbX = sb.CreateTimeline<DoubleAnimationUsingKeyFrames>(item, TargetProperty.CompositeTransform3D.TranslateZ)
                        .AddEasingDoubleKeyFrame(0, startDepth)
                        .AddEasingDoubleKeyFrame(startOffset, startDepth);

                    if (customEase == null)
                        dbX.AddSplineDoubleKeyFrame(startOffset.Add(duration), 0, KeySplines.EntranceTheme);
                    else
                        dbX.AddEasingDoubleKeyFrame(startOffset.Add(duration), 0, customEase);
                }

                // 3.4. Increment start offset
                startOffset = startOffset.Add(staggerTime);
            }

            return sb;
        }

        
    }
}
