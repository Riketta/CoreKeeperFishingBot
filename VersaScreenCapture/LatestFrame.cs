using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Capture;
using static VersaScreenCapture.Interop;
using Device = SharpDX.Direct3D11.Device;
using MapFlags = SharpDX.Direct3D11.MapFlags;

namespace VersaScreenCapture
{
    /// <summary>
    /// https://github.com/TheBlackPlague/DynoSharp.
    /// </summary>
    public class LatestFrame
    {
        protected static Direct3D11CaptureFrame CurrentFrame;
        protected static readonly Guid ResourceGuid = new Guid("DC8E63F3-D12B-4952-B47B-5E45026A862D");
        protected static long Texture2DTime;
        protected static long ByteArrayTime;
        protected static long MatTime;

        protected static Direct3D11CaptureFrame GetLatestFrame()
        {
            return Interlocked.Exchange(ref CurrentFrame, null);
        }

        protected static Texture2D GetLatestFrameAsTexture2D()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Direct3D11CaptureFrame frame = GetLatestFrame();
            Device device = CaptureHandler.GraphicCaptureDevice();
            if (frame == null) return null;

            // Somehow at certain times, an ObjectDisposedException comes here.
            // It makes zero sense exactly why at the moment, so using the trusty "try-catch" to workaround it for now.
            // Really odd considering all the checks done previous to running this.
            IDirect3DDxgiInterfaceAccess direct3DSurfaceDxgiInterfaceAccess;
            try
            {
                direct3DSurfaceDxgiInterfaceAccess = (IDirect3DDxgiInterfaceAccess)frame.Surface;
            }
            catch (ObjectDisposedException)
            {
                return null;
            }

            IntPtr resourcePointer = direct3DSurfaceDxgiInterfaceAccess.GetInterface(ResourceGuid);
            using (Texture2D surfaceTexture = new Texture2D(resourcePointer))
            {
                frame.Dispose();
                Texture2DDescription description = new Texture2DDescription
                {
                    ArraySize = 1,
                    BindFlags = BindFlags.None,
                    CpuAccessFlags = CpuAccessFlags.Read,
                    Format = Format.B8G8R8A8_UNorm,
                    Height = surfaceTexture.Description.Height,
                    Width = surfaceTexture.Description.Width,
                    MipLevels = 1,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = ResourceUsage.Staging // GPU -> CPU.
                };
                Texture2D texture2DFrame = new Texture2D(device, description);
                device.ImmediateContext.CopyResource(surfaceTexture, texture2DFrame);
                watch.Stop();
                Texture2DTime = watch.ElapsedMilliseconds;

                return texture2DFrame;
            }
        }

        protected static void CopyMemory(
            bool parallel,
            int from,
            int to,
            IntPtr sourcePointer,
            IntPtr destinationPointer,
            int sourceStride,
            int destinationStride)
        {
            if (!parallel)
            {
                for (int i = from; i < to; i++)
                {
                    IntPtr sourceIteratedPointer = IntPtr.Add(sourcePointer, sourceStride * i);
                    IntPtr destinationIteratedPointer = IntPtr.Add(destinationPointer, destinationStride * i);

                    // Memcpy is apparently faster than Buffer.MemoryCopy. 
                    Utilities.CopyMemory(destinationIteratedPointer, sourceIteratedPointer, destinationStride);
                }
                return;
            }

            Parallel.For(from, to, i =>
            {
                IntPtr sourceIteratedPointer = IntPtr.Add(sourcePointer, sourceStride * i);
                IntPtr destinationIteratedPointer = IntPtr.Add(destinationPointer, destinationStride * i);

                // Memcpy is apparently faster than Buffer.MemoryCopy. 
                Utilities.CopyMemory(destinationIteratedPointer, sourceIteratedPointer, destinationStride);
            });
        }

        protected static (byte[] frameBytes, int width, int height, int stride) GetLatestFrameAsByteBgra()
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            Texture2D frame = GetLatestFrameAsTexture2D();
            if (frame == null) return (null, 0, 0, 0);

            Device device = CaptureHandler.GraphicCaptureDevice();

            DataBox mappedMemory =
                device.ImmediateContext.MapSubresource(frame, 0, MapMode.Read, MapFlags.None);

            int width = frame.Description.Width;
            int height = frame.Description.Height;

            IntPtr sourcePointer = mappedMemory.DataPointer;
            int sourceStride = mappedMemory.RowPitch;
            int destinationStride = width * 4;

            byte[] frameBytes = new byte[width * height * 4]; // 4 bytes / pixel (High Mem. Allocation).

            unsafe
            {
                fixed (byte* frameBytesPointer = frameBytes)
                {
                    IntPtr destinationPointer = (IntPtr)frameBytesPointer;

                    CopyMemory(
                        true, // Should run in parallel or not.
                        0,
                        height,
                        sourcePointer,
                        destinationPointer,
                        sourceStride,
                        destinationStride
                        );
                }
            }

            device.ImmediateContext.UnmapSubresource(frame, 0);
            frame.Dispose();

            watch.Stop();
            ByteArrayTime = watch.ElapsedMilliseconds;

            return (frameBytes, width, height, destinationStride);
        }

        public static (long textureTime, long byteTime, long matTime) GetTimerValues()
        {
            return (Texture2DTime, ByteArrayTime, MatTime);
        }

        public static void AddFrame(Direct3D11CaptureFrame frame)
        {
            CurrentFrame = frame;
        }

        public static void FreeRuntimeResources()
        {
            CurrentFrame?.Dispose();
        }
    }
}
