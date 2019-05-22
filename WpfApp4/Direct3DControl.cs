using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;
using SharpDX.Direct2D1;

namespace WpfApp4
{
    public class Direct3DControl : UserControl
    {
        private readonly Direct3DWindowContext _context;

        private readonly bool _designMode;

        private bool _resizeEventSuppressed;

        public Direct3DControl()
        {
            SetStyle(ControlStyles.Opaque | ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.OptimizedDoubleBuffer, false);
            // чернь справа от opaque

            _designMode = DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

            if (_designMode)
                return;

            _context = new Direct3DWindowContext();

            _context.Render += (sender, e) => OnRender(EventArgs.Empty);
        }

        protected override void Dispose(bool disposing)
        {
            Trace.WriteLineIf(!disposing, "Possible memory leak");

            _context?.Dispose();

            base.Dispose(disposing);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            if (!_designMode)
                _context.Initialize(Handle);

            base.OnHandleCreated(e);

            if (_resizeEventSuppressed)
            {
                OnResize(EventArgs.Empty);
                _resizeEventSuppressed = false;
            }
        }
        protected override void OnHandleDestroyed(EventArgs e)
        {
            _context.Dispose();
            base.OnHandleDestroyed(e);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if (_designMode)
                return;

            //_context.PerformRender();
            Graphics.DrawMesh(this);

            ValidateState();

            base.OnPaint(e);
        }

        protected override void OnResize(EventArgs e)
        {
            if (!IsHandleCreated)
                _resizeEventSuppressed = true;
            else
                _context?.Update();

            base.OnResize(e);
        }

        protected override void OnParentChanged(EventArgs e)
        {
            if (_context != null && _context.IsInitialized)
                _context?.Update();

            base.OnParentChanged(e);
        }

        private void ValidateState()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(nameof(Direct3DControl));
            if (!IsHandleCreated)
                CreateControl();
            if (!_context.IsInitialized)
                RecreateHandle();
        }

        public event EventHandler Render;
        protected virtual void OnRender(EventArgs e) => Render?.Invoke(this, e);

        public DeviceContext Direct2DContext => _context.D2DContext;
    }
}