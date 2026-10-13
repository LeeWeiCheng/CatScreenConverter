using System.Windows;
using System.Windows.Media;

namespace CatVideo;

// Decorations contain only WPF vector drawing; Viewbox scales these alongside
// the interactive controls. They do not participate in hit testing.
public sealed class CatArt : FrameworkElement
{
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind), typeof(string), typeof(CatArt), new FrameworkPropertyMetadata("Paw", FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
        nameof(Fill), typeof(Brush), typeof(CatArt), new FrameworkPropertyMetadata(CatDrawing.Pink, FrameworkPropertyMetadataOptions.AffectsRender));

    public string Kind { get => (string)GetValue(KindProperty); set => SetValue(KindProperty, value); }
    public Brush Fill { get => (Brush)GetValue(FillProperty); set => SetValue(FillProperty, value); }

    public CatArt() { IsHitTestVisible = false; Focusable = false; }
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        if (ActualWidth > 0 && ActualHeight > 0)
            CatDrawing.InRect(drawingContext, Kind, new Rect(0, 0, ActualWidth, ActualHeight), Fill);
    }
}

public sealed class TrailArt : FrameworkElement
{
    public TrailArt() { IsHitTestVisible = false; Focusable = false; }
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);
        if (ActualWidth <= 0 || ActualHeight <= 0) return;
        drawingContext.PushClip(new RectangleGeometry(new Rect(0, 0, ActualWidth, ActualHeight)));
        var index = 0;
        for (double x = 20; x < ActualWidth - 8; x += 36)
        {
            if (index++ % 3 == 1)
                CatDrawing.InRect(drawingContext, "Fish", new Rect(x, ActualHeight / 2 - 7, 20, 14), CatDrawing.Peach);
            else
                CatDrawing.InRect(drawingContext, "Paw", new Rect(x - 4, ActualHeight / 2 - 8, 14, 16), CatDrawing.Print);
        }
        drawingContext.Pop();
    }
}

internal static class CatDrawing
{
    public static readonly Brush Brown = Brush("#5B2B19");
    public static readonly Brush Pink = Brush("#EC908B");
    public static readonly Brush Peach = Brush("#FFC5A9");
    public static readonly Brush Fur = Brush("#EFBA80");
    public static readonly Brush Patch = Brush("#DB9C5F");
    public static readonly Brush Print = Brush("#B06854");
    private static readonly Brush LightFur = Brush("#FFEFD2");
    private static readonly Brush Hill = Brush("#F3DAB7");
    private static readonly Dictionary<string, Geometry> Shapes = [];

    private static Brush Brush(string hex)
    {
        var brush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex));
        brush.Freeze();
        return brush;
    }

    private static Geometry Shape(string path)
    {
        if (Shapes.TryGetValue(path, out var cached)) return cached;
        var geometry = Geometry.Parse(path);
        geometry.Freeze();
        Shapes.Add(path, geometry);
        return geometry;
    }

    private static Pen Ink(double thickness = 3, Brush? brush = null)
    {
        var pen = new Pen(brush ?? Brown, thickness)
        {
            StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round,
            LineJoin = PenLineJoin.Round
        };
        pen.Freeze();
        return pen;
    }

    public static ImageSource CreateIcon()
    {
        var drawing = new DrawingGroup();
        using (var context = drawing.Open()) InRect(context, "Face", new Rect(0, 0, 100, 100), Fur);
        drawing.Freeze();
        var image = new DrawingImage(drawing);
        image.Freeze();
        return image;
    }

    public static void InRect(DrawingContext dc, string kind, Rect bounds, Brush fill)
    {
        var width = kind == "Stretch" ? 160 : kind == "Footer" ? 240 : 100;
        var height = kind == "Footer" ? 120 : 100;
        dc.PushTransform(new TranslateTransform(bounds.X, bounds.Y));
        dc.PushTransform(new ScaleTransform(bounds.Width / width, bounds.Height / height));
        switch (kind)
        {
            case "Badge": Badge(dc); break;
            case "Head": Head(dc, fill, false); break;
            case "Face": Head(dc, fill, true); break;
            case "Stretch": Stretch(dc); break;
            case "Footer": Footer(dc); break;
            case "Fish": Fish(dc, fill); break;
            default: Paw(dc, fill, false); break;
        }
        dc.Pop();
        dc.Pop();
    }

    private static void Paw(DrawingContext dc, Brush fill, bool outline)
    {
        var pen = outline ? Ink(4) : null;
        dc.DrawEllipse(fill, pen, new Point(15, 43.5), 9, 12.5);
        dc.DrawEllipse(fill, pen, new Point(36.5, 22.5), 9.5, 13.5);
        dc.DrawEllipse(fill, pen, new Point(62.5, 22.5), 9.5, 13.5);
        dc.DrawEllipse(fill, pen, new Point(85, 43.5), 9, 12.5);
        dc.DrawGeometry(fill, pen, Shape("M23,82 C10,61 31,51 42,42 C48,36 55,36 62,44 C75,55 91,66 77,83 C67,98 59,83 50,85 C40,86 33,95 23,82 Z"));
    }

    private static void Badge(DrawingContext dc)
    {
        dc.DrawGeometry(Peach, Ink(4), Shape("M18,61 C1,49 5,27 22,26 C20,4 38,1 49,15 C65,0 82,6 79,26 C98,27 100,48 84,61 C91,98 68,98 49,97 C25,100 11,85 18,61 Z"));
        dc.PushTransform(new TranslateTransform(20, 23));
        dc.PushTransform(new ScaleTransform(.62, .66));
        Paw(dc, Pink, true);
        dc.Pop(); dc.Pop();
    }

    private static void Head(DrawingContext dc, Brush fill, bool face)
    {
        var ink = Ink();
        dc.DrawGeometry(fill, ink, Shape("M13,42 L15,6 36,24 64,24 85,6 88,43 C110,101 -10,101 13,42 Z"));
        if (!face) return;
        dc.DrawGeometry(Pink, null, Shape("M20,17 L31,29 20,36 Z M80,17 L68,29 80,36 Z"));
        dc.DrawGeometry(null, ink, Shape("M25,47 Q32,56 40,47 M61,47 Q68,56 76,47 M50,64 Q45,76 39,68 M50,64 Q55,76 62,68 M7,57 L28,61 M8,68 L27,65 M73,61 L96,57 M74,65 L95,69"));
        dc.DrawGeometry(Brown, null, Shape("M46,60 L54,60 50,65 Z"));
    }

    private static void Fish(DrawingContext dc, Brush fill)
    {
        var pen = Ink(5, Print);
        dc.DrawGeometry(fill, pen, Shape("M0,0 L34,50 0,100 Z"));
        dc.DrawEllipse(fill, pen, new Point(59.5, 50), 37.5, 35);
        dc.DrawEllipse(Brown, null, new Point(79, 42), 4.5, 4.5);
    }

    private static void Stretch(DrawingContext dc)
    {
        var tail = Shape("M125,37 C145,9 105,-2 107,12");
        dc.DrawGeometry(null, Ink(12), tail);
        dc.DrawGeometry(null, Ink(8, Patch), tail);
        var body = Shape("M48,64 C78,67 74,20 110,28 C147,34 148,62 132,72 L136,91 111,94 116,80 C93,83 73,81 47,85 L17,94 8,89 36,72 Z");
        dc.DrawGeometry(LightFur, null, body);
        dc.PushClip(body);
        dc.DrawEllipse(Patch, null, new Point(104.5, 38), 24.5, 23);
        dc.Pop();
        dc.DrawGeometry(null, Ink(), body);
        InRect(dc, "Face", new Rect(12, 39, 62, 55), Fur);
        dc.DrawGeometry(null, Ink(), Shape("M22,91 L30,87 M116,92 L123,88"));
    }

    private static void Footer(DrawingContext dc)
    {
        dc.DrawEllipse(Hill, null, new Point(127.5, 132.5), 142.5, 42.5);
        dc.DrawEllipse(Brown, null, new Point(183.5, 71.5), 33.5, 28.5);
        InRect(dc, "Head", new Rect(118, 43, 47, 46), Brown);
        dc.DrawGeometry(null, Ink(10), Shape("M206,67 C251,9 208,9 205,20 M170,84 C156,99 127,109 113,108 M201,86 L219,107 M188,89 L181,109"));
        var yarn = new EllipseGeometry(new Point(46, 88.5), 32, 30.5);
        dc.DrawGeometry(Pink, null, yarn);
        dc.PushClip(yarn);
        for (var i = -4; i < 9; i++)
        {
            // Invariant formatting keeps geometry parsing stable on all locales.
            var diagonal = FormattableString.Invariant($"M3,{46 + i * 9} C38,{50 + i * 9} 61,{84 + i * 9} 87,{102 + i * 9}");
            var curved = FormattableString.Invariant($"M{8 + i * 9},52 C{43 + i * 6},66 {29 + i * 5},95 {43 + i * 9},123");
            dc.DrawGeometry(null, Ink(2.5), Shape(diagonal));
            dc.DrawGeometry(null, Ink(2.5), Shape(curved));
        }
        dc.Pop();
        dc.DrawGeometry(null, Ink(2.5), yarn);
        dc.DrawGeometry(null, Ink(2.5), Shape("M76,103 C94,94 87,118 108,111"));
        InRect(dc, "Paw", new Rect(92, 42, 19, 21), Brown);
        InRect(dc, "Paw", new Rect(202, 1, 20, 23), Brown);
    }
}
