using System;
using System.Collections.Generic;
using System.Drawing;

namespace WpfApp4
{
    public interface IRenderContext
    {
        void Clear(Color color);

        void DrawLine(PointF p1, PointF p2, Color color, float width, bool aliased);
        void DrawLine(IEnumerable<PointF> points, Color color, float width, bool aliased);

        void FillThickLine(Color color, float x1, float y1, float x2, float y2, float thickness);

        void DrawString(string text, PointF origin, Font font, Color brush, ContentAlignment alignment, float angle);
        void BeginFrame();
        void EndFrame();
        SizeF MeasureString(string text, Font font, ContentAlignment alignment);
    }
}