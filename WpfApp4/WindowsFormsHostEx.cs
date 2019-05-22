using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using System.Windows.Media;
using static HwndExtensions.Utils.Win32;

namespace WpfApp4
{
    public class WindowsFormsHostEx : WindowsFormsHost
    {
        static WindowsFormsHostEx()
        {
            // чтобы не копировался экран
            var argb = System.Drawing.Color.FromKnownColor(System.Drawing.KnownColor.Control).ToArgb();
            var color = Color.FromArgb((byte)(argb >> 24), (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb);
            BackgroundProperty.OverrideMetadata(typeof(WindowsFormsHostEx), new PropertyMetadata(new SolidColorBrush(color)));
        }

        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            return new PointHitTestResult(this, hitTestParameters.HitPoint);
        }

        protected override void OnPreviewMouseDown(MouseButtonEventArgs e)
        {
            Focus(); // форсируем фокус
            base.OnPreviewMouseDown(e);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            // отключаем пользовательский ввод и хиттесты по host'у
            // без этого никакого чуда не произойдёт
            SetWindowLong(Handle, GWL_STYLE, GetWindowLong(Handle, GWL_STYLE) | WS_DISABLED);

            return base.ArrangeOverride(finalSize);
        }
    }
}
