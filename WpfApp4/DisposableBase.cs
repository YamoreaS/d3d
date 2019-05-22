using System;
using System.Diagnostics;

namespace WpfApp4
{
    public abstract class DisposableBase : IDisposable
    {
        private bool _isDisposed;

        ~DisposableBase()
        {
            DisposeInternal(false);
        }

        private void DisposeInternal(bool disposing)
        {
            if (_isDisposed)
                return;

            Dispose(disposing);

            _isDisposed = true;
        }

        protected abstract void Dispose(bool disposing);

        public void Dispose()
        {
            DisposeInternal(true);
            GC.SuppressFinalize(this);
        }

        public bool IsDisposed => _isDisposed;

        [DebuggerStepThrough]
        public void EnsureNotDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(GetType().Name);
        }
    }
}
