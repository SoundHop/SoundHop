using System;
using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Hosting;
using SoundHop.Core.Interop;

namespace SoundHop.UI.Helpers
{
    /// <summary>
    /// Provides flyout entrance and exit animations using WinUI Composition.
    /// Adapted from EarTrumpet's WindowAnimationLibrary (https://github.com/File-New-Project/EarTrumpet) - MIT License.
    /// </summary>
    public static class FlyoutAnimationHelper
    {
        private const int AnimationOffsetPixels = 25;
        private const int EntranceDurationMs = 200;
        private const int ExitDurationMs = 150;

        /// <summary>
        /// Plays the flyout entrance animation (slide + fade in).
        /// </summary>
        public static void BeginEntranceAnimation(UIElement element, WindowsTaskbar.Position taskbarPosition, Action? completed = null)
        {
            var visual = ElementCompositionPreview.GetElementVisual(element);
            var compositor = visual.Compositor;

            // Stop any existing animations
            visual.StopAnimation("Offset");
            visual.StopAnimation("Opacity");

            // Calculate slide direction based on taskbar position
            Vector3 startOffset = taskbarPosition switch
            {
                WindowsTaskbar.Position.Left => new Vector3(-AnimationOffsetPixels, 0, 0),
                WindowsTaskbar.Position.Right => new Vector3(AnimationOffsetPixels, 0, 0),
                WindowsTaskbar.Position.Top => new Vector3(0, -AnimationOffsetPixels, 0),
                WindowsTaskbar.Position.Bottom => new Vector3(0, AnimationOffsetPixels, 0),
                _ => new Vector3(0, AnimationOffsetPixels, 0)
            };

            // Set initial state immediately
            visual.Offset = startOffset;
            visual.Opacity = 0.5f;

            // Create slide animation
            var slideAnimation = compositor.CreateVector3KeyFrameAnimation();
            slideAnimation.InsertKeyFrame(0f, startOffset);
            slideAnimation.InsertKeyFrame(1f, Vector3.Zero, compositor.CreateCubicBezierEasingFunction(new Vector2(0.1f, 0.9f), new Vector2(0.2f, 1f)));
            slideAnimation.Duration = TimeSpan.FromMilliseconds(EntranceDurationMs);

            // Create fade animation
            var fadeAnimation = compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(0f, 0.5f);
            fadeAnimation.InsertKeyFrame(1f, 1f, compositor.CreateCubicBezierEasingFunction(new Vector2(0.1f, 0.9f), new Vector2(0.2f, 1f)));
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(EntranceDurationMs);

            // Create animation batch to track completion
            var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += (s, e) =>
            {
                visual.Offset = Vector3.Zero;
                visual.Opacity = 1f;
                completed?.Invoke();
            };

            visual.StartAnimation("Offset", slideAnimation);
            visual.StartAnimation("Opacity", fadeAnimation);

            batch.End();
        }

        /// <summary>
        /// Plays the flyout exit animation (slide + fade out).
        /// </summary>
        public static void BeginExitAnimation(UIElement element, WindowsTaskbar.Position taskbarPosition, Action? completed = null)
        {
            var visual = ElementCompositionPreview.GetElementVisual(element);
            var compositor = visual.Compositor;

            // Stop any existing animations
            visual.StopAnimation("Offset");
            visual.StopAnimation("Opacity");

            // Calculate slide direction based on taskbar position
            Vector3 endOffset = taskbarPosition switch
            {
                WindowsTaskbar.Position.Left => new Vector3(-AnimationOffsetPixels, 0, 0),
                WindowsTaskbar.Position.Right => new Vector3(AnimationOffsetPixels, 0, 0),
                WindowsTaskbar.Position.Top => new Vector3(0, -AnimationOffsetPixels, 0),
                WindowsTaskbar.Position.Bottom => new Vector3(0, AnimationOffsetPixels, 0),
                _ => new Vector3(0, AnimationOffsetPixels, 0)
            };

            // Create slide animation
            var slideAnimation = compositor.CreateVector3KeyFrameAnimation();
            slideAnimation.InsertKeyFrame(0f, Vector3.Zero);
            slideAnimation.InsertKeyFrame(1f, endOffset, compositor.CreateCubicBezierEasingFunction(new Vector2(0.1f, 0.9f), new Vector2(0.2f, 1f)));
            slideAnimation.Duration = TimeSpan.FromMilliseconds(ExitDurationMs);

            // Create fade animation
            var fadeAnimation = compositor.CreateScalarKeyFrameAnimation();
            fadeAnimation.InsertKeyFrame(0f, 1f);
            fadeAnimation.InsertKeyFrame(1f, 0f, compositor.CreateCubicBezierEasingFunction(new Vector2(0.1f, 0.9f), new Vector2(0.2f, 1f)));
            fadeAnimation.Duration = TimeSpan.FromMilliseconds(ExitDurationMs);

            // Create animation batch to track completion
            var batch = compositor.CreateScopedBatch(CompositionBatchTypes.Animation);
            batch.Completed += (s, e) =>
            {
                completed?.Invoke();
            };

            visual.StartAnimation("Offset", slideAnimation);
            visual.StartAnimation("Opacity", fadeAnimation);

            batch.End();
        }

        /// <summary>
        /// Resets the visual to its default state (no offset, full opacity).
        /// </summary>
        public static void ResetVisual(UIElement element)
        {
            var visual = ElementCompositionPreview.GetElementVisual(element);
            visual.Offset = Vector3.Zero;
            visual.Opacity = 1f;
        }
    }
}
