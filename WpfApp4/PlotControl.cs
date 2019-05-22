using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace WpfApp4
{
    public class PlotControl : ContentControl, IDisposable
    {
        public static readonly DependencyProperty BackendProperty = DependencyProperty.Register(
            nameof(Backend), typeof(string), typeof(PlotControl), new PropertyMetadata(default(string), OnBackendChanged));

        public string Backend {
            get { return (string)GetValue(BackendProperty); }
            set { SetValue(BackendProperty, value); }
        }

        private static void OnBackendChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            ((PlotControl)sender).OnBackendChanged(sender, EventArgs.Empty);
        }

        private GraphicsBackend _backend;

        private static readonly List<PlotControl> Instances = new List<PlotControl>();

        static PlotControl()
        {
            Application.Current.Dispatcher.ShutdownStarted += (sender, e) => {
                Instances.ToList().ForEach(x => x.Dispose());
            };
        }

        public PlotControl()
        {
            Loaded += OnLoaded;

            Instances.Add(this);
        }

        public void Dispose()
        {
            _backend?.Dispose();

            Instances.Remove(this);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            OnLoaded(null, null);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_backend == null || _backend.IsDisposed)
                UpdateBackend();
        }

        private void OnBackendChanged(object sender, EventArgs e)
        {
            UpdateBackend();
        }

        private void UpdateBackend()
        {
            if (_backend != null)
            {
                _backend.Render -= OnBackendRender;
                _backend.Resize -= OnBackendResize;

                _backend.Dispose();
            }

            if (!IsLoaded)
            {
                Content = null;
                return;
            }

            _backend = GraphicsBackend.Create(Backend);

            _backend.Render += OnBackendRender;
            _backend.Resize += OnBackendResize;

            Content = _backend.GetControl();

            OnContentChanged(EventArgs.Empty);
        }

        private void OnBackendRender(object sender, EventArgs<IRenderContext> e)
        {
            e.Data.Clear(System.Drawing.Color.White);
            e.Data.DrawLine(new PointF(100, 100), new PointF(200, 200), Color.Black, 1, true );
        }

        private void OnBackendResize(object sender, EventArgs<System.Drawing.Size> e)
        {
            _backend?.Invalidate();
        }

        public event EventHandler ContentChanged;
        protected virtual void OnContentChanged(EventArgs e) => ContentChanged?.Invoke(this, e);
    }
}