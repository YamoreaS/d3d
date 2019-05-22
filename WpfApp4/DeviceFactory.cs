using System;
using SharpDX;
using SharpDX.Direct3D;
using SharpDX.Direct3D11;

namespace WpfApp4
{
    public static class DeviceFactory
    {
        public static Device CreateDevice()
        {
            // Try to create a hardware device first and fall back to a
            // software (WARP doens't let us share resources)
            var device = TryCreateDevice(DriverType.Hardware) ?? TryCreateDevice(DriverType.Software);
            if (device == null)
                throw new SharpDXException("Unable to create a DirectX 11 device.");

            return device;
        }

        private static Device TryCreateDevice(DriverType type)
        {
            // We'll try to create the device that supports any of these feature levels
            var levels = new[] {
                FeatureLevel.Level_10_0,
                FeatureLevel.Level_9_3,
                FeatureLevel.Level_9_2,
                FeatureLevel.Level_9_1
            };

            foreach (var level in levels)
            {
                try
                {
                    return new Device(type, DeviceCreationFlags.BgraSupport, level);

                } // Try the next feature level
                catch (SharpDXException) { } // D3DERR_INVALIDCALL or E_FAIL
                catch (ArgumentException) { } // E_INVALIDARG
                catch (OutOfMemoryException) { } // E_OUTOFMEMORY
            }
            return null; // We failed to create a device at any required feature level
        }
    }
}
