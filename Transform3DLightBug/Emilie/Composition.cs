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
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Hosting;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

namespace Emilie.UWP.Media
{
    /// <summary>
    /// Helpful extensions methods to enable you to write fluent Composition animations
    /// </summary>
    public static class Composition
    {
        /*
         * NOTE
         * 
         * Type contraints on extension methods do not form part of the method
         * signature used for choosing a correct method. Therefore two extensions
         * with the same parametres but different type contraints will conflict
         * with each other.
         * 
         * Due to this, some methods here use type contraints whereas other that
         * conflict with the XAML storyboarding extensions use explicit type
         * extensions. When adding methods, please keep in mind whether it's 
         * possible some other toolkit might have a similar signature for extensions
         * to form your plan of attack
         */

        #region Fundamentals

        public static T SafeDispose<T>(T disposable) where T : IDisposable
        {
            disposable?.Dispose();
            return default(T);
        }

        public static Compositor Compositor { get; set; } = Window.Current.Compositor;

        public static void CreateScopedBatch(this Compositor compositor,
            CompositionBatchTypes batchType,
            Action<CompositionScopedBatch> action,
            Action<CompositionScopedBatch> onCompleteAction = null)
        {
            if (action == null)
                throw
                  new ArgumentException("Cannot create a scoped batch on an action with null value!",
                  nameof(action));

            // Create ScopedBatch
            var batch = compositor.CreateScopedBatch(batchType);

            //// Handler for the Completed Event
            TypedEventHandler<object, CompositionBatchCompletedEventArgs> handler = null;
            handler = (s, ea) =>
            {
                // Unsubscribe the handler from the Completed Event
                ((CompositionScopedBatch)s).Completed -= handler;

                try
                {
                    // Invoke the post action
                    onCompleteAction?.Invoke(batch);
                }
                finally
                {
                    ((CompositionScopedBatch)s).Dispose();
                }
            };

            batch.Completed += handler;

            // Invoke the action
            action(batch);


            // End Batch
            batch.End();
        }

        #endregion


        #region Element / Base Extensions

        /// <summary>
        /// Returns the Composition Handoff Visual for this framework element
        /// </summary>
        /// <param name="element"></param>
        /// <returns>Composition Handoff Visual</returns>
        public static Visual GetVisual(this UIElement element) => ElementCompositionPreview.GetElementVisual(element);

        public static CompositionPropertySet GetScrollManipulationPropertySet(this ScrollViewer scrollViewer) => ElementCompositionPreview.GetScrollViewerManipulationPropertySet(scrollViewer);

        public static void SetShowAnimation(this UIElement element, ICompositionAnimationBase animation)
        {
            ElementCompositionPreview.SetImplicitShowAnimation(element, animation);
        }

        public static void SetHideAnimation(this UIElement element, ICompositionAnimationBase animation)
        {
            ElementCompositionPreview.SetImplicitHideAnimation(element, animation);
        }

        public static void SetChildVisual(this FrameworkElement element, Visual visual)
        {
            ElementCompositionPreview.SetElementChildVisual(element, visual);
        }

        public static bool SupportsAlphaMask(UIElement element)
        {
            switch (element)
            {
                case TextBlock _:
                case Shape _:
                case Image _:
                    return true;

                default:
                    return false;
            }
        }

        public static CompositionBrush GetAlphaMask(UIElement element)
        {
            switch (element)
            {
                case TextBlock t:
                    return t.GetAlphaMask();

                case Shape s:
                    return s.GetAlphaMask();

                case Image i:
                    return i.GetAlphaMask();

                default:
                    return null;
            }
        }

        #endregion


        #region Translation

        public static FrameworkElement EnableCompositionTranslation(this FrameworkElement element)
        {
            return EnableCompositionTranslation(element, null);
        }

        public static FrameworkElement EnableCompositionTranslation(this FrameworkElement element, Vector3? defaultTranslation)
        {
            ElementCompositionPreview.SetIsTranslationEnabled(element, true);
            Visual visual = element.GetVisual();
            if (defaultTranslation.HasValue)
                visual.Properties.InsertVector3(CompositionProperty.Translation, defaultTranslation.Value);

            return element;
        }

        public static Vector3 GetTranslation(this Visual visual)
        {
            visual.Properties.TryGetVector3(CompositionProperty.Translation, out Vector3 translation);
            return translation;
        }

        public static Visual SetTranslation(this Visual visual, Vector3 translation)
        {
            visual.Properties.InsertVector3(CompositionProperty.Translation, translation);
            return visual;
        }

        #endregion


        #region SetTarget

        public static ExpressionAnimation SetTarget(this ExpressionAnimation animation, string target)
        {
            animation.Target = target;
            return animation;
        }

        public static ColorKeyFrameAnimation SetTarget(this ColorKeyFrameAnimation animation, string target)
        {
            animation.Target = target;
            return animation;
        }

        public static ScalarKeyFrameAnimation SetTarget(this ScalarKeyFrameAnimation animation, string target)
        {
            animation.Target = target;
            return animation;
        }

        public static Vector2KeyFrameAnimation SetTarget(this Vector2KeyFrameAnimation animation, string target)
        {
            animation.Target = target;

            return animation;
        }

        public static Vector3KeyFrameAnimation SetTarget(this Vector3KeyFrameAnimation animation, string target)
        {
            animation.Target = target;
            return animation;
        }

        public static Vector4KeyFrameAnimation SetTarget(this Vector4KeyFrameAnimation animation, string target)
        {
            animation.Target = target;
            return animation;
        }

        public static QuaternionKeyFrameAnimation SetTarget(this QuaternionKeyFrameAnimation animation, string target)
        {
            animation.Target = target;
            return animation;
        }

        private static T SetSafeTarget<T>(this T animation, string target) where T : KeyFrameAnimation
        {
            if (!String.IsNullOrEmpty(target))
                animation.Target = target;

            return animation;
        }

        #endregion


        #region SetDelayTime

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="animation"></param>
        /// <param name="delayTime">Delay Time in seconds</param>
        /// <returns></returns>
        public static T SetDelayTime<T>(this T animation, double delayTime) where T : KeyFrameAnimation
        {
            SetDelayTime(animation, TimeSpan.FromSeconds(delayTime));
            return animation;
        }

        public static T SetDelayTime<T>(this T animation, TimeSpan delayTime) where T : KeyFrameAnimation
        {
            animation.DelayTime = delayTime;
            return animation;
        }

        #endregion


        #region Set Duration

        public static KeyFrameAnimation SetDuration(this KeyFrameAnimation animation, double duration)
        {
            return SetDuration(animation, TimeSpan.FromSeconds(duration));
        }

        public static KeyFrameAnimation SetDuration(this KeyFrameAnimation animation, TimeSpan duration)
        {
            animation.Duration = duration;
            return animation;
        }

        public static QuaternionKeyFrameAnimation SetDuration(this QuaternionKeyFrameAnimation animation, double duration)
        {
            return SetDuration(animation, TimeSpan.FromSeconds(duration));
        }

        public static QuaternionKeyFrameAnimation SetDuration(this QuaternionKeyFrameAnimation animation, TimeSpan duration)
        {
            animation.Duration = duration;
            return animation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="animation"></param>
        /// <param name="duration">Duration in seconds</param>
        /// <returns></returns>
        public static ScalarKeyFrameAnimation SetDuration(this ScalarKeyFrameAnimation animation, double duration)
        {
            return SetDuration(animation, TimeSpan.FromSeconds(duration));
        }

        public static ScalarKeyFrameAnimation SetDuration(this ScalarKeyFrameAnimation animation, TimeSpan duration)
        {
            animation.Duration = duration;
            return animation;
        }

        public static ColorKeyFrameAnimation SetDuration(this ColorKeyFrameAnimation animation, double duration)
        {
            return SetDuration(animation, TimeSpan.FromSeconds(duration));
        }

        public static ColorKeyFrameAnimation SetDuration(this ColorKeyFrameAnimation animation, TimeSpan duration)
        {
            animation.Duration = duration;
            return animation;
        }

        public static Vector2KeyFrameAnimation SetDuration(this Vector2KeyFrameAnimation animation, double duration)
        {
            return SetDuration(animation, TimeSpan.FromSeconds(duration));
        }

        public static Vector2KeyFrameAnimation SetDuration(this Vector2KeyFrameAnimation animation, TimeSpan duration)
        {
            animation.Duration = duration;
            return animation;
        }

        public static Vector3KeyFrameAnimation SetDuration(this Vector3KeyFrameAnimation animation, double duration)
        {
            return SetDuration(animation, TimeSpan.FromSeconds(duration));
        }

        public static Vector3KeyFrameAnimation SetDuration(this Vector3KeyFrameAnimation animation, TimeSpan duration)
        {
            animation.Duration = duration;
            return animation;
        }

        public static Vector4KeyFrameAnimation SetDuration(this Vector4KeyFrameAnimation animation, double duration)
        {
            return SetDuration(animation, TimeSpan.FromSeconds(duration));
        }

        public static Vector4KeyFrameAnimation SetDuration(this Vector4KeyFrameAnimation animation, TimeSpan duration)
        {
            animation.Duration = duration;
            return animation;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="animation"></param>
        /// <param name="duration">Duration in seconds</param>
        /// <returns></returns>
        //private static T SetDurationInternal<T>(this T animation, double duration) where T : KeyFrameAnimation
        //{
        //    return SetDurationInternal(animation, TimeSpan.FromSeconds(duration));
        //}

        //private static T SetDurationInternal<T>(this T animation, TimeSpan duration) where T : KeyFrameAnimation
        //{
        //    animation.Duration = duration;
        //    return animation;
        //}

        #endregion


        #region StopBehaviour

        public static T SetStopBehavior<T>(this T animation, AnimationStopBehavior stopBehavior) where T : KeyFrameAnimation
        {
            animation.StopBehavior = stopBehavior;
            return animation;
        }

        #endregion


        #region Direction

        public static T SetDirection<T>(this T animation, AnimationDirection direction) where T : KeyFrameAnimation
        {
            animation.Direction = direction;
            return animation;
        }

        #endregion


        #region IterationBehavior

        public static T SetIterationBehavior<T>(this T animation, AnimationIterationBehavior iterationBehavior) where T : KeyFrameAnimation
        {
            animation.IterationBehavior = iterationBehavior;
            return animation;
        }

        #endregion


        #region AddKeyFrame

        public static T AddExpressionKeyFrame<T>(this T animation, float normalizedProgressKey, string value, CompositionEasingFunction ease = null) where T : KeyFrameAnimation
        {
            animation.InsertExpressionKeyFrame(normalizedProgressKey, value, ease);
            return animation;
        }

        public static ScalarKeyFrameAnimation AddKeyFrame(this ScalarKeyFrameAnimation animation, float normalizedProgressKey, float value, CompositionEasingFunction ease = null)
        {
            animation.InsertKeyFrame(normalizedProgressKey, value, ease);
            return animation;
        }

        public static ColorKeyFrameAnimation AddKeyFrame(this ColorKeyFrameAnimation animation, float normalizedProgressKey, Color value, CompositionEasingFunction ease = null)
        {
            animation.InsertKeyFrame(normalizedProgressKey, value, ease);
            return animation;
        }

        public static Vector2KeyFrameAnimation AddKeyFrame(this Vector2KeyFrameAnimation animation, float normalizedProgressKey, Vector2 value, CompositionEasingFunction ease = null)
        {
            animation.InsertKeyFrame(normalizedProgressKey, value, ease);
            return animation;
        }

        public static Vector3KeyFrameAnimation AddKeyFrame(this Vector3KeyFrameAnimation animation, float normalizedProgressKey, Vector3 value, CompositionEasingFunction ease = null)
        {
            animation.InsertKeyFrame(normalizedProgressKey, value, ease);
            return animation;
        }

        /// <summary>
        /// Z Component defaults to 1f
        /// </summary>
        /// <param name="animation"></param>
        /// <param name="normalizedProgressKey"></param>
        /// <param name="value"></param>
        /// <param name="ease"></param>
        /// <returns></returns>
        public static Vector3KeyFrameAnimation AddKeyFrame(this Vector3KeyFrameAnimation animation, float normalizedProgressKey, float value, CompositionEasingFunction ease = null)
        {
            animation.InsertKeyFrame(normalizedProgressKey, new Vector3(value, value, 1f), ease);
            return animation;
        }

        /// <summary>
        /// Z Component defaults to 1f
        /// </summary>
        public static Vector3KeyFrameAnimation AddKeyFrame(this Vector3KeyFrameAnimation animation, float normalizedProgressKey, float x, float y, CompositionEasingFunction ease = null)
        {
            animation.InsertKeyFrame(normalizedProgressKey, new Vector3(x, y, 1f), ease);
            return animation;
        }

        public static Vector3KeyFrameAnimation AddKeyFrame(this Vector3KeyFrameAnimation animation, float normalizedProgressKey, float x, float y, float z, CompositionEasingFunction ease = null)
        {
            animation.InsertKeyFrame(normalizedProgressKey, new Vector3(x, y, z), ease);
            return animation;
        }

        public static Vector4KeyFrameAnimation AddKeyFrame(this Vector4KeyFrameAnimation animation, float normalizedProgressKey, Vector4 value, CompositionEasingFunction ease = null)
        {
            animation.InsertKeyFrame(normalizedProgressKey, value, ease);
            return animation;
        }

        public static QuaternionKeyFrameAnimation AddKeyFrame(this QuaternionKeyFrameAnimation animation, float normalizedProgressKey, Quaternion value, CompositionEasingFunction ease = null)
        {
            animation.InsertKeyFrame(normalizedProgressKey, value, ease);
            return animation;
        }

        #endregion


        #region SetComment

        public static T SetComment<T>(this T obj, string comment) where T : CompositionObject
        {
            obj.Comment = comment;
            return obj;
        }

        #endregion


        #region Create

        public static ColorKeyFrameAnimation CreateColorKeyFrameAnimation(this Visual visual, string targetProperty = null)
        {
            return visual.Compositor.CreateColorKeyFrameAnimation().SetSafeTarget(targetProperty);
        }

        public static ScalarKeyFrameAnimation CreateScalarKeyFrameAnimation(this Visual visual, string targetProperty = null)
        {
            return visual.Compositor.CreateScalarKeyFrameAnimation().SetSafeTarget(targetProperty);
        }

        public static Vector2KeyFrameAnimation CreateVector2KeyFrameAnimation(this Visual visual, string targetProperty = null)
        {
            return visual.Compositor.CreateVector2KeyFrameAnimation().SetSafeTarget(targetProperty);
        }

        public static Vector3KeyFrameAnimation CreateVector3KeyFrameAnimation(this Visual visual, string targetProperty = null)
        {
            return visual.Compositor.CreateVector3KeyFrameAnimation().SetSafeTarget(targetProperty);
        }

        public static Vector4KeyFrameAnimation CreateVector4KeyFrameAnimation(this Visual visual, string targetProperty = null)
        {
            return visual.Compositor.CreateVector4KeyFrameAnimation().SetSafeTarget(targetProperty);
        }

        public static QuaternionKeyFrameAnimation CreateQuaternionKeyFrameAnimation(this Visual visual, string targetProperty = null)
        {
            return visual.Compositor.CreateQuaternionKeyFrameAnimation().SetSafeTarget(targetProperty);
        }

        public static ExpressionAnimation CreateExpressionAnimation(this Visual visual)
        {
            return visual.Compositor.CreateExpressionAnimation();
        }

        public static ExpressionAnimation CreateExpressionAnimation(this Visual visual, string targetProperty)
        {
            return visual.Compositor.CreateExpressionAnimation().SetTarget(targetProperty);
        }

        #endregion


        #region SetExpression

        public static ExpressionAnimation SetExpression(this ExpressionAnimation animation, string expression)
        {
            animation.Expression = expression;
            return animation;
        }

        #endregion  


        #region AddParametre

        public static T AddReferenceParameter<T>(this T animation, string key, UIElement parametre) where T : CompositionAnimation
        {
            animation.SetReferenceParameter(key, parametre.GetVisual());
            return animation;
        }


        public static T AddReferenceParameter<T>(this T animation, string key, CompositionObject parametre) where T : CompositionAnimation
        {
            animation.SetReferenceParameter(key, parametre);
            return animation;
        }

        public static T AddScalarParameter<T>(this T animation, string key, float value) where T : CompositionAnimation
        {
            animation.SetScalarParameter(key, value);
            return animation;
        }

        public static T AddVector2Parameter<T>(this T animation, string key, Vector2 value) where T : CompositionAnimation
        {
            animation.SetVector2Parameter(key, value);
            return animation;
        }

        public static T AddVector3Parameter<T>(this T animation, string key, Vector3 value) where T : CompositionAnimation
        {
            animation.SetVector3Parameter(key, value);
            return animation;
        }

        public static T AddVector4Parameter<T>(this T animation, string key, Vector4 value) where T : CompositionAnimation
        {
            animation.SetVector4Parameter(key, value);
            return animation;
        }

        #endregion


        #region Animation Start / Stop

        public static void StartAnimation(this CompositionObject compositionObject, CompositionAnimation animation)
        {
            if (string.IsNullOrWhiteSpace(animation.Target))
                throw new ArgumentNullException("Animation has no target");

            compositionObject.StartAnimation(animation.Target, animation);
        }

        public static void StopAnimation(this CompositionObject compositionObject, CompositionAnimation animation)
        {
            if (string.IsNullOrWhiteSpace(animation.Target))
                throw new ArgumentNullException("Animation has no target");

            compositionObject.StopAnimation(animation.Target);
        }

        #endregion

        #region Brushes

        public static CompositionGradientBrush AsCompositionBrush(this LinearGradientBrush brush, Compositor compositor)
        {
            var compBrush = compositor.CreateLinearGradientBrush();

            foreach (var stop in brush.GradientStops)
            {
                compBrush.ColorStops.Add(compositor.CreateColorGradientStop((float)stop.Offset, stop.Color));
            }

            // todo : try and copy transforms?

            return compBrush;
        }

        #endregion
    }
}