using System;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DXGI;

namespace WpfApp4
{
    public class Direct3DWindowContext : Direct3DContext
    {
        private const int BufferCount = 2;

        private SwapChain1 _swapChain;

        // 3D
        //private Texture2D _target;
        //private RenderTargetView _targetView;
        //private SharpDX.Direct3D11.DeviceContext1 _d3DContext;
        // !3D

        private DeviceContext _d2DContext;

        private Surface _backBuffer;
        private Bitmap1 _d2DTarget;

        private bool _updatePending;

        public void Initialize(IntPtr hWnd)
        {
            Requires.State(!IsInitialized);

            Initialize();

            var swapChainDescription = new SwapChainDescription1
            {
                Width = 0,
                Height = 0,
                Format = Format.B8G8R8A8_UNorm,
                Stereo = default,
                SampleDescription = new SampleDescription(1, 0),
                Usage = Usage.RenderTargetOutput,
                BufferCount = BufferCount,
                Scaling = Scaling.Stretch,
                SwapEffect = SwapEffect.FlipSequential,
                AlphaMode = default,
                Flags = default,
            };

            using (var dxgiDevice2 = Device.QueryInterface<SharpDX.DXGI.Device2>())
            using (var dxgiFactory2 = dxgiDevice2.Adapter.GetParent<SharpDX.DXGI.Factory2>())
            {
                _swapChain = new SwapChain1(dxgiFactory2, Device, hWnd, ref swapChainDescription);

                using (var d2DDevice = new SharpDX.Direct2D1.Device(dxgiDevice2))
                    _d2DContext = new DeviceContext(d2DDevice, DeviceContextOptions.None);
            }
        }

        protected override void Cleanup()
        {
            Utilities.Dispose(ref _d2DTarget);
            Utilities.Dispose(ref _backBuffer);
            Utilities.Dispose(ref _d2DContext);
            Utilities.Dispose(ref _swapChain);

            base.Cleanup();
        }

        public void Update()
        {
            Requires.State(IsInitialized);

            _updatePending = true;
        }

        protected override void OnBeforeRender(EventArgs e)
        {
            //Graphics.DrawMesh();
            base.OnBeforeRender(e);
            return;
            if (_updatePending)
            {
                _updatePending = false;

                Device.ImmediateContext.ClearState();

                //Graphics.DrawMesh(Device, _swapChain);

                _d2DContext.Target = null;

                Utilities.Dispose(ref _d2DTarget);
                Utilities.Dispose(ref _backBuffer);

                //_swapChain.ResizeBuffers(BufferCount, 0, 0, Format.B8G8R8A8_UNorm, SwapChainFlags.None);

                // Specify the properties for the bitmap that we will use as the target of our Direct2D operations.
                // We want a 32-bit BGRA surface with premultiplied alpha.
                var properties = new BitmapProperties1(new PixelFormat(Format.B8G8R8A8_UNorm, SharpDX.Direct2D1.AlphaMode.Premultiplied), 96, 96, BitmapOptions.Target | BitmapOptions.CannotDraw);

                // Get the default surface as a backbuffer and create the Bitmap1 that will hold the Direct2D drawing target.
                _backBuffer = _swapChain.GetBackBuffer<Surface>(0);
                _d2DTarget = new Bitmap1(_d2DContext, _backBuffer, properties);

                // Set the Direct2D drawing target.
                _d2DContext.Target = _d2DTarget;
            }

            base.OnBeforeRender(e);
        }

        protected override void OnAfterRender(EventArgs e)
        {
            base.OnAfterRender(e);

            _swapChain.Present(0, PresentFlags.None, default);
        }

        public DeviceContext D2DContext => _d2DContext;
    }
}
