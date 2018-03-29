//*********************************************************
// 
// PROJECT EMILIE
//
// Copyright (c) Johnny Westlake. All rights reserved.
//
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*********************************************************

using System;
using System.Numerics;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Media3D;

namespace Emilie.UWP.Media
{
    /// <summary>
    /// A collection of useful animation-focused extension methods
    /// </summary>
    public static class Animation
    {
        #region Composite Transform

        public static CompositeTransform GetNewCompositeTransform(this FrameworkElement element, bool centreOriginOnCreation = true, bool overwriteOtherTransforms = true)
        {
            element.RenderTransform = null;
            return element.GetCompositeTransform(centreOriginOnCreation, overwriteOtherTransforms);
        }

        public static CompositeTransform GetCompositeTransform(this FrameworkElement element, bool centreOriginOnCreation = true, bool overwriteOtherTransforms = true)
        {
            CompositeTransform ct = null;

            ct = element.RenderTransform as CompositeTransform;

            if (ct != null)
                return ct;

            // 3. If there's nothing there, create a new CompositeTransform
            if (ct == null && element.RenderTransform == null)
            {
                element.RenderTransform = new CompositeTransform();
                ct = (CompositeTransform)element.RenderTransform;
                if (centreOriginOnCreation)
                {
                    ct.CenterX = ct.CenterY = 0.5;
                    element.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
                }
            }
            else
            {
                ct = new CompositeTransform();
                if (centreOriginOnCreation)
                {
                    ct.CenterX = ct.CenterY = 0.5;
                    element.RenderTransformOrigin = new Windows.Foundation.Point(0.5, 0.5);
                }

                // 5. See if the existing item is a singular transform, and convert it to a CompositeTransform
                if (element.RenderTransform is Transform transform)
                {
                    ApplyTransform(ref ct, transform);
                    element.RenderTransform = ct;
                }
                else
                {
                    // 6. If we're a group of transforms, convert each child individually
                    if (element.RenderTransform is TransformGroup group)
                    {
                        foreach (var tran in group.Children)
                            ApplyTransform(ref ct, tran);

                        element.RenderTransform = ct;
                    }
                }
            }
            return ct;
        }

        /// <summary>
        /// Adds the effect of a regular transform to a composite transform
        /// </summary>
        /// <param name="ct"></param>
        /// <param name="t"></param>
        internal static void ApplyTransform(ref CompositeTransform ct, Transform t)
        {
            if (t is TranslateTransform tt)
            {
                ct.TranslateX = tt.X;
                ct.TranslateY = tt.Y;
            }
            else if (t is RotateTransform rt)
            {
                ct.Rotation = rt.Angle;
                ct.CenterX = rt.CenterX;
                ct.CenterY = rt.CenterY;
            }
            else if (t is SkewTransform sK)
            {
                ct.SkewX = sK.AngleX;
                ct.SkewY = sK.AngleY;
                ct.CenterX = sK.CenterX;
                ct.CenterY = sK.CenterY;
            }
            else if (t is ScaleTransform sc)
            {
                ct.ScaleX = sc.ScaleX;
                ct.ScaleY = sc.ScaleY;
                ct.CenterX = sc.CenterX;
                ct.CenterY = sc.CenterY;
            }
        }

        #endregion

        #region Plane Projection

        /// <summary>
        /// Gets the plane projection from a FrameworkElement's projection property. If 
        /// the property is null or not a plane projection, a new plane projection is created
        /// and set as the plane projection and then returned
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        public static PlaneProjection GetPlaneProjection(this FrameworkElement element)
        {
            PlaneProjection projection = null;

            projection = element.Projection as PlaneProjection;
            if (projection == null)
            {
                element.Projection = new PlaneProjection();
                projection = (PlaneProjection)element.Projection;
            }

            return projection;
        }


        #endregion

        #region Storyboard

        /// <summary>
        /// Returns an await-able task that runs the storyboard through to completion
        /// </summary>
        /// <param name="storyboard"></param>
        /// <returns></returns>
        public static Task BeginAsync(this Storyboard storyboard)
        {
            TaskCompletionSource<bool> tcs = new TaskCompletionSource<bool>();
            if (storyboard == null) tcs.SetException(new ArgumentNullException());
            else
            {
                void onComplete(object s, object e)
                {
                    storyboard.Completed -= onComplete;
                    tcs.SetResult(true);
                }

                storyboard.Completed += onComplete;
                storyboard.Begin();
            }
            return tcs.Task;
        }

        public static void AddTimeline(this Storyboard storyboard, Timeline timeline, DependencyObject target, String targetProperty)
        {
            if (target is FrameworkElement frameworkElement)
            {
                if (targetProperty.StartsWith(TargetProperty.CompositeTransform.Identifier))
                    GetCompositeTransform(frameworkElement);
                else if (targetProperty.StartsWith(TargetProperty.PlaneProjection.Identifier))
                    GetPlaneProjection(frameworkElement);
                else if (targetProperty.StartsWith(TargetProperty.CompositeTransform3D.Identifier))
                    GetCompositeTransform3D(frameworkElement);
            }

            Storyboard.SetTarget(timeline, target);
            Storyboard.SetTargetProperty(timeline, targetProperty);

            storyboard.Children.Add(timeline);
        }

        #endregion

        #region Timelines

        public static DoubleAnimationUsingKeyFrames AddLinearDoubleKeyFrame(this DoubleAnimationUsingKeyFrames doubleAnimation, double seconds, double value)
        {
            doubleAnimation.AddEasingDoubleKeyFrame(TimeSpan.FromSeconds(seconds), value);
            return doubleAnimation;
        }

        public static DoubleAnimationUsingKeyFrames AddLinearDoubleKeyFrame(this DoubleAnimationUsingKeyFrames doubleAnimation, TimeSpan time, double value)
        {
            doubleAnimation.KeyFrames.Add(new LinearDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(time),
                Value = value
            });

            return doubleAnimation;
        }

        public static DoubleAnimationUsingKeyFrames AddEasingDoubleKeyFrame(this DoubleAnimationUsingKeyFrames doubleAnimation, double seconds, double value, EasingFunctionBase ease = null)
        {
            doubleAnimation.AddEasingDoubleKeyFrame(TimeSpan.FromSeconds(seconds), value, ease);
            return doubleAnimation;
        }

        public static DoubleAnimationUsingKeyFrames AddEasingDoubleKeyFrame(this DoubleAnimationUsingKeyFrames doubleAnimation, TimeSpan time, double value, EasingFunctionBase ease = null)
        {
            doubleAnimation.KeyFrames.Add(new EasingDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(time),
                Value = value,
                EasingFunction = ease
            });

            return doubleAnimation;
        }

        public static DoubleAnimationUsingKeyFrames AddSplineDoubleKeyFrame(this DoubleAnimationUsingKeyFrames doubleAnimation, double seconds, double value, KeySpline spline = null)
        {
            doubleAnimation.AddSplineDoubleKeyFrame(TimeSpan.FromSeconds(seconds), value, spline);
            return doubleAnimation;
        }

        public static DoubleAnimationUsingKeyFrames AddSplineDoubleKeyFrame(this DoubleAnimationUsingKeyFrames doubleAnimation, TimeSpan time, double value, KeySpline spline = null)
        {
            doubleAnimation.KeyFrames.Add(new SplineDoubleKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(time),
                Value = value,
                KeySpline = spline
            });
            return doubleAnimation;
        }

        public static ObjectAnimationUsingKeyFrames AddDiscreteObjectKeyFrame(this ObjectAnimationUsingKeyFrames objectAnimation, double seconds, object value)
        {
            objectAnimation.AddDiscreteObjectKeyFrame(TimeSpan.FromSeconds(seconds), value);
            return objectAnimation;
        }

        public static ObjectAnimationUsingKeyFrames AddDiscreteObjectKeyFrame(this ObjectAnimationUsingKeyFrames objectAnimation, TimeSpan time, object value)
        {
            objectAnimation.KeyFrames.Add(new DiscreteObjectKeyFrame
            {
                KeyTime = KeyTime.FromTimeSpan(time),
                Value = value
            });
            return objectAnimation;
        }

        #endregion

        #region Fluency

        public static DoubleAnimation To(this DoubleAnimation animation, double? value)
        {
            animation.To = value;
            return animation;
        }

        public static DoubleAnimation By(this DoubleAnimation animation, double? value)
        {
            animation.By = value;
            return animation;
        }

        public static DoubleAnimation From(this DoubleAnimation animation, double? value)
        {
            animation.From = value;
            return animation;
        }

        public static T SetSpeedRatio<T>(this T storyboard, double speedRatio) where T : Timeline
        {
            storyboard.SpeedRatio = speedRatio;
            return storyboard;
        }

        public static T SetBeginTime<T>(this T storyboard, double beginTime) where T : Timeline
        {
            return SetBeginTime(storyboard, TimeSpan.FromSeconds(beginTime));
        }

        public static T SetBeginTime<T>(this T storyboard, TimeSpan beginTime) where T : Timeline
        {
            storyboard.BeginTime = beginTime;
            return storyboard;
        }

        public static T SetTimelineDuration<T>(this T storyboard, double duration) where T : Timeline
        {
            return SetTimelineDuration(storyboard, TimeSpan.FromSeconds(duration));
        }

        public static T SetTimelineDuration<T>(this T storyboard, TimeSpan duration) where T : Timeline
        {
            storyboard.Duration = duration;
            return storyboard;
        }

        public static T SetRepeatBehavior<T>(this T storyboard, RepeatBehavior behavior) where T : Timeline
        {
            storyboard.RepeatBehavior = behavior;
            return storyboard;
        }


        #endregion


        #region Transform3D

        public static CompositeTransform3D GetCompositeTransform3D(this FrameworkElement target)
        {
            CompositeTransform3D transform = target.Transform3D as CompositeTransform3D;

            if (transform == null)
            {
                transform = new CompositeTransform3D();
                target.Transform3D = transform;
            }

            return transform;
        }

        public static PerspectiveTransform3D GetPerspectiveTransform3D(this FrameworkElement target)
        {
            PerspectiveTransform3D transform = target.Transform3D as PerspectiveTransform3D;

            if (transform == null)
            {
                transform = new PerspectiveTransform3D();
                target.Transform3D = transform;
            }

            return transform;
        }

        #endregion


        public static KeySpline CreateKeySpline(Double x1, Double y1, Double x2, Double y2)
        {
            KeySpline keyspline = new KeySpline();
            keyspline.SetPoints(x1, y1, x2, y2);
            return keyspline;
        }

        public static void SetPoints(this KeySpline keySpline, Double x1, Double y1, Double x2, Double y2)
        {
            keySpline.ControlPoint1 = new Windows.Foundation.Point(x1, y1);
            keySpline.ControlPoint2 = new Windows.Foundation.Point(x2, y2);
        }


        public static T CreateTimeline<T>(DependencyObject target, String targetProperty, Storyboard parent) where T : Timeline, new()
        {
            T timeline = new T();
            parent.AddTimeline(timeline, target, targetProperty);
            return timeline;
        }

        public static T CreateTimeline<T>(this Storyboard parent, DependencyObject target, String targetProperty) where T : Timeline, new()
        {
            T timeline = new T();
            parent.AddTimeline(timeline, target, targetProperty);
            return timeline;
        }



        #region Attached Properties

        #region ShouldAnimate

        /// <summary>
        /// Used by Preset Animations. If false, element will not be animated and will not affect stagger timings.
        /// </summary>
        public static bool GetShouldAnimate(DependencyObject obj)
        {
            return (bool)obj.GetValue(ShouldAnimateProperty);
        }
        /// <summary>
        /// Used by Preset Animations. If false, element will not be animated and will not affect stagger timings.
        /// </summary>
        public static void SetShouldAnimate(DependencyObject obj, bool value)
        {
            obj.SetValue(ShouldAnimateProperty, value);
        }

        /// <summary>
        /// Used by Preset Animations. If false, element will not be animated and will not affect stagger timings.
        /// </summary>
        public static readonly DependencyProperty ShouldAnimateProperty =
            DependencyProperty.RegisterAttached("ShouldAnimate", typeof(bool), typeof(FrameworkElement), new PropertyMetadata(true));


        #endregion

        #region AddedDelay

        /// <summary>
        /// Used by Preset Animations. Allows specific elements to cause additional stagger.
        /// </summary>
        public static TimeSpan GetAddedDelay(DependencyObject obj)
        {
            return (TimeSpan)obj.GetValue(AddedDelayProperty);
        }

        /// <summary>
        /// Used by Preset Animations. Allows specific elements to cause additional stagger.
        /// </summary>
        public static void SetAddedDelay(DependencyObject obj, TimeSpan value)
        {
            obj.SetValue(AddedDelayProperty, value);
        }

        /// <summary>
        /// Used by Preset Animations. Allows you to specific an additional stagger delay for a single element
        /// </summary>
        public static readonly DependencyProperty AddedDelayProperty =
            DependencyProperty.RegisterAttached("AddedDelay", typeof(TimeSpan), typeof(FrameworkElement), new PropertyMetadata(TimeSpan.Zero));


        #endregion

        #region IsDepthTarget

        public static bool GetIsDepthTarget(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDepthTargetProperty);
        }

        public static void SetIsDepthTarget(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDepthTargetProperty, value);
        }

        public static readonly DependencyProperty IsDepthTargetProperty =
            DependencyProperty.RegisterAttached("IsDepthTarget", typeof(bool), typeof(FrameworkElement), new PropertyMetadata(false));

        #endregion

        #region Group

        public static int GetGroup(DependencyObject obj)
        {
            return (int)obj.GetValue(GroupProperty);
        }

        public static void SetGroup(DependencyObject obj, int value)
        {
            obj.SetValue(GroupProperty, value);
        }

        public static readonly DependencyProperty GroupProperty =
            DependencyProperty.RegisterAttached("Group", typeof(int), typeof(FrameworkElement), new PropertyMetadata(0));

        #endregion

        #endregion
    }
}
