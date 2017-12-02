// Based on the OrientationStateTrigger of Morten Nielsen.
//https://github.com/dotMorten/WindowsStateTriggers

using System;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace VirtuosoClient.TestHarness.Common
{
    /// <summary>
    /// Trigger for switching when the screen display state changes
    /// </summary>
	public class DisplayStateTrigger : StateTriggerBase, ITriggerValue
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DisplayStateTrigger"/> class.
        /// </summary>
        public DisplayStateTrigger()
        {
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                var weakEvent =
                    new WeakEventListener<DisplayStateTrigger, DisplayInformation, object>(this)
                    {
                        OnEventAction = (instance, source, eventArgs) => OrientationStateTrigger_OrientationChanged(source, eventArgs),
                        OnDetachAction = (instance, weakEventListener) => DisplayInformation.GetForCurrentView().OrientationChanged -= weakEventListener.OnEvent
                    };
                DisplayInformation.GetForCurrentView().OrientationChanged += weakEvent.OnEvent;
            }
            var window = CoreApplication.GetCurrentView()?.CoreWindow;
            if (window != null)
            {
                var weakEvent = new WeakEventListener<DisplayStateTrigger, CoreWindow, WindowSizeChangedEventArgs>(this)
                {
                    OnEventAction = (instance, s, e) => OnCoreWindowOnSizeChanged(s, e),
                    OnDetachAction = (instance, weakEventListener) => window.SizeChanged -= weakEventListener.OnEvent
                };
                window.SizeChanged += weakEvent.OnEvent;
            }
        }

        private void OnCoreWindowOnSizeChanged(CoreWindow sender, WindowSizeChangedEventArgs args)
        {
            UpdateTrigger(sender?.Bounds);
        }

        private void OrientationStateTrigger_OrientationChanged(DisplayInformation sender, object args)
        {
            UpdateTrigger(sender.CurrentOrientation);
        }

        private void UpdateTrigger(Rect? rect)
        {
            ActiveSize = rect;
        }


        private void UpdateTrigger()
        {
            EDisplayState ds = CalculateDisplayState();
            if (ds == EDisplayState.None)
            {
                IsActive = false;
                return;
            }
            IsActive = DisplayState == ds;
        }

        private EDisplayState CalculateDisplayState()
        {
            Size s = ActiveSize;
            ulong l = s.Width;
            DisplayOrientations o = ActiveOrientation;
            if (o == DisplayOrientations.None) return EDisplayState.None;
            if (l <= 360)
            {
                if (s.IsLandscape)
                    return EDisplayState.SmallLandscape;
                else
                    return EDisplayState.SmallPortrait;
            }
            else if (l <= 720)
            {
                if (s.IsLandscape)
                    return EDisplayState.MediumLandscape;
                else
                    return EDisplayState.MediumPortrait;
            }
            else if (l <= 1360)
            {
                //if (o == DisplayOrientations.Landscape 
                //    || o == DisplayOrientations.LandscapeFlipped
                //    || s.IsLandscape)
                if (s.IsLandscape)
                    return EDisplayState.LargeLandscape;
                else
                    return EDisplayState.LargePortrait;
            }
            else
            {
                if (s.IsLandscape)
                    return EDisplayState.WideLandcape;
                else
                    return EDisplayState.WidePortrait;
            }
        }

        private void UpdateTrigger(DisplayOrientations orientation)
        {
            ActiveOrientation = orientation;
        }

        object locker = new object();
        private DisplayOrientations _DisplayOrientations;
        private DisplayOrientations _activeOrientation
        {
            get { return _DisplayOrientations; }
            set
            {
                if (_DisplayOrientations != value)
                {
                    _DisplayOrientations = value;
                    UpdateTrigger();
                }

            }
        }
        private DisplayOrientations ActiveOrientation
        {
            get { return _activeOrientation; }
            set
            {
                lock (locker)
                    if (_activeOrientation != value)
                    {
                        _activeOrientation = value;
                        UpdateDisplaySize();
                    }
            }
        }

        private struct Size : IEquatable<Size>, IEquatable<Rect>, IEquatable<Rect?>
        {
            public readonly ulong Width;
            public readonly ulong Height;

            public bool IsLandscape => Width > Height;

            public Size(ulong width, ulong height)
            {
                Width = width;
                Height = height;
            }
            public Size(ulong val) : this(val, val) { }
            public Size(Size other) : this(other.Width, other.Height) { }
            public override bool Equals(object obj)
            {
                return obj is Size && this == (Size)obj;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    // Choose large primes to avoid hashing collisions
                    const int HashingBase = (int)2166136261;
                    const int HashingMultiplier = 16777619;

                    int hash = HashingBase;
                    hash = (hash * HashingMultiplier) ^ Width.GetHashCode();
                    hash = (hash * HashingMultiplier) ^ Height.GetHashCode();
                    return hash;
                }
            }

            public static bool operator ==(Size x, Size y)
            {
                return x.Width == y.Width && x.Height == y.Height;
            }

            public static bool operator !=(Size x, Size y)
            {
                return !(x == y);
            }

            public static implicit operator Size(Rect? rect)
            {
                if (rect != null && rect.HasValue)
                    return rect.Value;
                else
                    return new Size(0);
            }

            public static implicit operator Size(Rect rect)
            {
                return new Size((ulong)rect.Width, (ulong)rect.Height);
            }

            private bool Equals(Rect rect)
            {
                return Width == (ulong)rect.Width && Height == (ulong)rect.Height;
            }

            private bool Equals(Rect? rect)
            {
                if (rect.HasValue) Equals(rect.Value);
                return false;
            }

            private bool Equals(Size b)
            {
                return this == b;
            }

            bool IEquatable<Size>.Equals(Size other)
            {
                return Equals(other);
            }

            bool IEquatable<Rect>.Equals(Rect other)
            {
                return Equals(other);
            }

            bool IEquatable<Rect?>.Equals(Rect? other)
            {
                return Equals(other);
            }
        };

        private Size _size;
        private Size _InnerSize
        {
            get => _size;
            set
            {
                if (_size.Equals(value)) return;
                _size = value;
                UpdateTrigger();
            }
        }

        private Size ActiveSize
        {
            get => _InnerSize;
            set
            {
                lock (locker)
                {
                    if (_InnerSize.Equals(value)) return;
                    _InnerSize = value;
                    UpdateDisplayOrientation();
                }
            }
        }

        private void UpdateDisplayOrientation()
        {
            _activeOrientation = DisplayInformation.GetForCurrentView().CurrentOrientation;
        }

        private void UpdateDisplaySize()
        {
            _InnerSize = CoreApplication.GetCurrentView()?.CoreWindow.Bounds;
        }

        /// <summary>
        /// Gets or sets the orientation to trigger on.
        /// </summary>
        public EDisplayState DisplayState
        {
            get { return (EDisplayState)GetValue(DisplayStateProperty); }
            set { SetValue(DisplayStateProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="DisplayState"/> orientation parameter.
        /// </summary>
        public static readonly DependencyProperty DisplayStateProperty =
            DependencyProperty.Register("DisplayState", typeof(EDisplayState), typeof(DisplayStateTrigger),
            new PropertyMetadata(EDisplayState.None, OnDisplayStatePropertyChanged));

        private static void OnDisplayStatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var obj = (DisplayStateTrigger)d;
            if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
            {
                obj.UpdateTrigger(DisplayInformation.GetForCurrentView().CurrentOrientation);
            }
        }

        #region ITriggerValue

        private bool m_IsActive;

        /// <summary>
        /// Gets a value indicating whether this trigger is active.
        /// </summary>
        /// <value><c>true</c> if this trigger is active; otherwise, <c>false</c>.</value>
        public bool IsActive
        {
            get { return m_IsActive; }
            private set
            {
                if (m_IsActive != value)
                {
                    m_IsActive = value;
                    base.SetActive(value);
                    if (IsActiveChanged != null)
                        IsActiveChanged(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Occurs when the <see cref="IsActive" /> property has changed.
        /// </summary>
        public event EventHandler IsActiveChanged;

        #endregion ITriggerValue

        public enum EDisplayState
        {
            None,
            SmallPortrait,
            SmallLandscape,
            MediumPortrait,
            MediumLandscape,
            LargePortrait,
            LargeLandscape,
            WidePortrait,
            WideLandcape
        }

    }
}