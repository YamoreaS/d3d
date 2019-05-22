using System;
using System.Diagnostics;
using SharpDX;
using SharpDX.Direct3D11;

namespace WpfApp4
{
    public abstract class Direct3DContext : DisposableBase
    {
        private Device1 _device;

        protected override void Dispose(bool disposing)
        {
            Trace.WriteLineIf(!disposing, "Possible memory leak");

            Cleanup();
        }

        protected virtual void Cleanup()
        {
            Utilities.Dispose(ref _device);
        }

        protected void Initialize()
        {
            Requires.State(!IsInitialized);

            var defaultDevice = DeviceFactory.CreateDevice();

            _device = defaultDevice.QueryInterface<Device1>();
        }

        public void PerformRender()
        {
            Requires.State(IsInitialized);

            OnBeforeRender(EventArgs.Empty);
            //OnRender(EventArgs.Empty);
            OnAfterRender(EventArgs.Empty);
        }

        public bool IsInitialized {
            [DebuggerStepThrough]
            get {
                EnsureNotDisposed();

                return _device != null;
            }
        }

        protected Device1 Device => _device;

        // Render

        public event EventHandler BeforeRender;
        protected virtual void OnBeforeRender(EventArgs e) => BeforeRender?.Invoke(this, e);

        public event EventHandler Render;
        protected virtual void OnRender(EventArgs e) => Render?.Invoke(this, e);

        public event EventHandler AfterRender;
        protected virtual void OnAfterRender(EventArgs e) => AfterRender?.Invoke(this, e);
    }
}
