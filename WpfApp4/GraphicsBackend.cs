using System;
using System.Diagnostics;
using System.Windows;

namespace WpfApp4
{
    public abstract class GraphicsBackend : DisposableBase
    {
        private GraphicsBackend() { }

        public static GraphicsBackend Create(string backend)
        {
            return new Direct3DWindow();
        }

        protected override void Dispose(bool disposing)
        {
            Trace.WriteLineIf(!disposing, "Possible memory leak");
        }

        public abstract void Invalidate();

        public abstract FrameworkElement GetControl();

        public event EventHandler<EventArgs<IRenderContext>> Render;
        protected virtual void OnRender(EventArgs<IRenderContext> e) => Render?.Invoke(this, e);

        public event EventHandler<EventArgs<System.Drawing.Size>> Resize;
        protected virtual void OnResize(EventArgs<System.Drawing.Size> e) => Resize?.Invoke(this, e);

        private abstract class WindowBackend<T> : GraphicsBackend where T : System.Windows.Forms.Control
        {
            private readonly WindowsFormsHostEx _host = new WindowsFormsHostEx();

            protected T Control { get; }

            private bool _isInvalidated;

            public WindowBackend(T control)
            {
                Requires.NotNull(control, nameof(control));

                _host.Child = Control = control;

                // обходим баг в GetBitmapFromRenderTargetBitmap
                Control.MinimumSize = new System.Drawing.Size(1, 1);

                // синхронизируем фокус
                _host.GotFocus += (sender, e) => Control.Focus();

                Control.Resize += OnResize;

                _host.Child.Paint += (sender, e) => _isInvalidated = false;
            }

            public override FrameworkElement GetControl() => _host;

            private void OnResize(object sender, EventArgs e) => OnResize(new EventArgs<System.Drawing.Size>(Control.Size));

            public override void Invalidate()
            {
                if (_host.Child == null)
                    return;

                if (!_isInvalidated)
                {
                    _host.Child.Invalidate();
                    _isInvalidated = true;
                }
            }

            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);
                _host.Dispose();
            }
        }

        private class Direct3DWindow : WindowBackend<Direct3DControl>
        {
            private Direct2DRenderContext _renderContext;
            private SharpDX.Direct2D1.RenderTarget _d2dTarget;

            public Direct3DWindow()
                : base(new Direct3DControl())
            {

                Control.Render += OnControlRender;
            }

            private void OnControlRender(object sender, EventArgs e)
            {
                if (_renderContext == null || _d2dTarget != Control.Direct2DContext)
                {
                    _renderContext?.Dispose();
                    _renderContext = new Direct2DRenderContext(_d2dTarget = Control.Direct2DContext);
                }

                _renderContext.BeginFrame();

                OnRender(new EventArgs<IRenderContext>(_renderContext));

                _renderContext.EndFrame();
            }


            protected override void Dispose(bool disposing)
            {
                base.Dispose(disposing);

                Control.Render -= OnControlRender;
                _renderContext?.Dispose();
            }
        }
    }
}
