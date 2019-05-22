using System;
using System.Collections.Generic;
using System.ComponentModel;
using SharpDX;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SharpDX.Mathematics.Interop;
using FontStyle = SharpDX.DirectWrite.FontStyle;

namespace WpfApp4
{
    public class Direct2DRenderContext : IRenderContext, IDisposable
    {
        private readonly RenderTarget _renderTarget;

        private readonly SharpDX.DirectWrite.Factory _writeFactory = new SharpDX.DirectWrite.Factory(SharpDX.DirectWrite.FactoryType.Shared);
        private readonly List<IDisposable> _frameResources = new List<IDisposable>();

        private readonly double _texelWidth;
        private readonly double _texelHeight;

        private AntialiasMode _antialiasMode;

        public Direct2DRenderContext(RenderTarget renderTarget)
        {
            Requires.NotNull(renderTarget, nameof(renderTarget));
            Requires.Argument(!renderTarget.IsDisposed, nameof(renderTarget));

            _renderTarget = renderTarget;

            _texelWidth = 96.0 / _renderTarget.DotsPerInch.Width;
            _texelHeight = 96.0 / _renderTarget.DotsPerInch.Height;
        }

        public void BeginFrame()
        {
            _antialiasMode = _renderTarget.AntialiasMode;
            _renderTarget.BeginDraw();
        }

        public void EndFrame()
        {
            try
            {
                _renderTarget.EndDraw();
            }
            finally
            {
                foreach (var resource in _frameResources)
                    resource.Dispose();
                _frameResources.Clear();
            }
        }

        public void Dispose()
        {
            _writeFactory.Dispose();
        }

        public void Clear(System.Drawing.Color color)
        {
            _renderTarget.Clear(ToRawColor4(color));
        }

        public void ResetClip()
        {
            _renderTarget.PopAxisAlignedClip();
        }

        public void DrawLine(System.Drawing.PointF p1, System.Drawing.PointF p2, System.Drawing.Color color, float width, bool aliased)
        {
            SetAntialiasMode(aliased);

            var v1 = ToRawVector2(p1);
            var v2 = ToRawVector2(p2);

            if (aliased)
            {
                v1 = SnapToGrid(v1);
                v2 = SnapToGrid(v2);
            }

            _renderTarget.DrawLine(v1, v2, CreateSolidColorBrush(color), width);
        }

        public void DrawLine(IEnumerable<System.Drawing.PointF> points, System.Drawing.Color color, float width, bool aliased)
        {
            SetAntialiasMode(aliased);

            var brush = CreateSolidColorBrush(color);

            using (var e = points.GetEnumerator())
            {
                if (!e.MoveNext())
                    return;

                var v1 = ToRawVector2(e.Current);
                if (aliased)
                    v1 = SnapToGrid(v1);

                while (e.MoveNext())
                {
                    var v2 = ToRawVector2(e.Current);
                    if (aliased)
                        v2 = SnapToGrid(v2);

                    _renderTarget.DrawLine(v1, v2, brush, width);

                    v1 = v2;
                }
            }
        }

        public void DrawLines(System.Drawing.PointF[] points, int index, int count, System.Drawing.Color color, float width, bool aliased)
        {
            Requires.Argument((count & 1) == 0, nameof(count));

            SetAntialiasMode(aliased);

            var brush = CreateSolidColorBrush(color);

            for (int i = 0; i < count; i += 2)
            {
                var v1 = ToRawVector2(points[index + i]);
                var v2 = ToRawVector2(points[index + i + 1]);

                if (aliased)
                {
                    v1 = SnapToGrid(v1);
                    v2 = SnapToGrid(v2);
                }

                _renderTarget.DrawLine(v1, v2, brush, width);
            }
        }

        public System.Drawing.SizeF MeasureString(string text, System.Drawing.Font font, System.Drawing.ContentAlignment alignment)
        {
            var format = CreateTextFormat(font);
            format.TextAlignment = ToTextAlignment(alignment);
            format.ParagraphAlignment = ToParagraphAlignment(alignment);

            var metricsLayout = new TextLayout(_writeFactory, text, format, float.MaxValue, float.MaxValue);

            var height = metricsLayout.Metrics.Height;
            var width = metricsLayout.Metrics.Width;

            _frameResources.Add(metricsLayout);

            return new System.Drawing.SizeF(width, height);
        }

        public void DrawString(string text, System.Drawing.PointF origin, System.Drawing.Font font, System.Drawing.Color color, System.Drawing.ContentAlignment alignment, float angle)
        {
            var size = MeasureString(text, font, alignment);

            var format = CreateTextFormat(font);
            format.TextAlignment = ToTextAlignment(alignment);
            format.ParagraphAlignment = ToParagraphAlignment(alignment);

            var transform = Math.Abs(angle) > Math.PI / 180 ? Matrix3x2.Rotation(angle, ToRawVector2(origin)) : (Matrix3x2?)null;

            var layout = new TextLayout(_writeFactory, text, format, size.Width * 2, size.Height * 2, 1, transform, true);
            layout.ParagraphAlignment = ToParagraphAlignment(alignment);
            layout.TextAlignment = ToTextAlignment(alignment);

            var brush = CreateSolidColorBrush(color);

            var alignedOrigin = AlignTextOrigin(origin, size, alignment);

            if (transform.HasValue)
                _renderTarget.Transform = transform.Value;
            _renderTarget.DrawTextLayout(SnapToGrid(ToRawVector2(alignedOrigin)), layout, brush, DrawTextOptions.None);
            _renderTarget.Transform = Matrix3x2.Identity;

            // 2cache?
            _frameResources.Add(layout);
        }

        private static System.Drawing.PointF AlignTextOrigin(System.Drawing.PointF origin, System.Drawing.SizeF size, System.Drawing.ContentAlignment alignment)
        {
            float x;
            switch (alignment)
            {
                case System.Drawing.ContentAlignment.TopLeft:
                case System.Drawing.ContentAlignment.MiddleLeft:
                case System.Drawing.ContentAlignment.BottomLeft:
                    x = origin.X;
                    break;
                case System.Drawing.ContentAlignment.TopCenter:
                case System.Drawing.ContentAlignment.MiddleCenter:
                case System.Drawing.ContentAlignment.BottomCenter:
                    x = origin.X - size.Width;
                    break;
                case System.Drawing.ContentAlignment.TopRight:
                case System.Drawing.ContentAlignment.MiddleRight:
                case System.Drawing.ContentAlignment.BottomRight:
                    x = origin.X - size.Width * 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
            }

            float y;
            switch (alignment)
            {
                case System.Drawing.ContentAlignment.TopLeft:
                case System.Drawing.ContentAlignment.TopCenter:
                case System.Drawing.ContentAlignment.TopRight:
                    y = origin.Y;
                    break;
                case System.Drawing.ContentAlignment.MiddleLeft:
                case System.Drawing.ContentAlignment.MiddleCenter:
                case System.Drawing.ContentAlignment.MiddleRight:
                    y = origin.Y - size.Height;
                    break;
                case System.Drawing.ContentAlignment.BottomLeft:
                case System.Drawing.ContentAlignment.BottomCenter:
                case System.Drawing.ContentAlignment.BottomRight:
                    y = origin.Y - size.Height * 2;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(alignment), alignment, null);
            }
            return new System.Drawing.PointF(x, y);
        }

        public void FillThickLine(System.Drawing.Color color, float x1, float y1, float x2, float y2, float thickness)
        {
            var angle = Math.Atan2(y2 - y1, x2 - x1);
            var dx = (float)(thickness * Math.Cos(angle));
            var dy = (float)(thickness * Math.Sin(angle));

            var p = new PathGeometry(_renderTarget.Factory);
            _frameResources.Add(p);
            using (var sink = p.Open())
            {
                sink.BeginFigure(new RawVector2(x1 + dy, y1 - dx), FigureBegin.Filled);
                sink.AddArc(new ArcSegment
                {
                    Point = new RawVector2(x1 - dy, y1 + dx),
                    ArcSize = ArcSize.Large,
                    RotationAngle = 1,
                    Size = new Size2F(thickness, thickness),
                    SweepDirection = SweepDirection.CounterClockwise
                });
                sink.AddLine(new RawVector2(x2 - dy, y2 + dx));
                sink.AddArc(new ArcSegment
                {
                    Point = new RawVector2(x2 + dy, y2 - dx),
                    ArcSize = ArcSize.Large,
                    RotationAngle = 1,
                    Size = new Size2F(thickness, thickness),
                    SweepDirection = SweepDirection.CounterClockwise
                });
                sink.EndFigure(FigureEnd.Closed);
                sink.Close();

                _renderTarget.FillGeometry(p, CreateSolidColorBrush(color));
            }
        }

        private static double Snap(double value, double delta)
        {
            return delta * Math.Round(value / delta, MidpointRounding.AwayFromZero);
        }
        private float SnapToGridX(float v)
        {
            return (float)(Snap(v, _texelWidth) + _texelWidth / 2);
        }
        private float SnapToGridY(float v)
        {
            return (float)(Snap(v, _texelHeight) + _texelHeight / 2);
        }
        private RawVector2 SnapToGrid(RawVector2 v)
        {
            return new RawVector2(SnapToGridX(v.X), SnapToGridY(v.Y));
        }
        private RawRectangleF SnapToGrid(RawRectangleF v)
        {
            return new RawRectangleF(SnapToGridX(v.Left), SnapToGridY(v.Top), SnapToGridX(v.Right), SnapToGridY(v.Bottom));
        }

        private static RawVector2 ToRawVector2(System.Drawing.PointF p)
        {
            return new RawVector2(p.X, p.Y);
        }

        private static RawColor4 ToRawColor4(System.Drawing.Color color)
        {
            var argb = color.ToArgb();

            return new RawColor4(
                (byte)(argb >> 16) / 255f, // R
                (byte)(argb >> 08) / 255f, // G
                (byte)(argb >> 00) / 255f, // B
                (byte)(argb >> 24) / 255f  // A
            );
        }

        private static TextAlignment ToTextAlignment(System.Drawing.ContentAlignment alignment)
        {
            switch (alignment)
            {
                case System.Drawing.ContentAlignment.TopLeft:
                case System.Drawing.ContentAlignment.MiddleLeft:
                case System.Drawing.ContentAlignment.BottomLeft:
                    return TextAlignment.Leading;
                case System.Drawing.ContentAlignment.TopCenter:
                case System.Drawing.ContentAlignment.MiddleCenter:
                case System.Drawing.ContentAlignment.BottomCenter:
                    return TextAlignment.Center;
                case System.Drawing.ContentAlignment.TopRight:
                case System.Drawing.ContentAlignment.MiddleRight:
                case System.Drawing.ContentAlignment.BottomRight:
                    return TextAlignment.Trailing;
                default:
                    throw new InvalidEnumArgumentException(nameof(alignment), (int)alignment, typeof(System.Drawing.ContentAlignment));
            }
        }

        private static ParagraphAlignment ToParagraphAlignment(System.Drawing.ContentAlignment alignment)
        {
            switch (alignment)
            {
                case System.Drawing.ContentAlignment.TopLeft:
                case System.Drawing.ContentAlignment.TopCenter:
                case System.Drawing.ContentAlignment.TopRight:
                    return ParagraphAlignment.Near;
                case System.Drawing.ContentAlignment.MiddleLeft:
                case System.Drawing.ContentAlignment.MiddleCenter:
                case System.Drawing.ContentAlignment.MiddleRight:
                    return ParagraphAlignment.Center;
                case System.Drawing.ContentAlignment.BottomLeft:
                case System.Drawing.ContentAlignment.BottomCenter:
                case System.Drawing.ContentAlignment.BottomRight:
                    return ParagraphAlignment.Far;
                default:
                    throw new InvalidEnumArgumentException(nameof(alignment), (int)alignment, typeof(System.Drawing.ContentAlignment));
            }
        }

        private SolidColorBrush CreateSolidColorBrush(System.Drawing.Color color)
        {
            return new SolidColorBrush(_renderTarget, ToRawColor4(color));
        }

        private TextFormat CreateTextFormat(System.Drawing.Font font)
        {
            var weight = font.Bold ? FontWeight.Bold : FontWeight.Normal;
            var style = font.Italic ? FontStyle.Italic : FontStyle.Normal;
            var format = new TextFormat(_writeFactory, font.FontFamily.Name, null, weight, style, FontStretch.Normal, font.SizeInPoints);
            format.ParagraphAlignment = ParagraphAlignment.Center;
            format.TextAlignment = TextAlignment.Justified;
            return format;
        }

        // opt
        private void SetAntialiasMode(bool aliased)
        {
            var antialiasMode = aliased ? AntialiasMode.Aliased : AntialiasMode.PerPrimitive;
            if (_antialiasMode != antialiasMode)
            {
                _antialiasMode = antialiasMode;
                _renderTarget.AntialiasMode = antialiasMode;
            }
        }
    }
}