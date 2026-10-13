# Export the same vector cat face used in CatArtwork.cs, without launching the app.
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName PresentationCore,WindowsBase
$root = Split-Path $PSScriptRoot -Parent
$assetDir = Join-Path $root 'Assets'
New-Item -ItemType Directory -Force -Path $assetDir | Out-Null
$iconStream = [IO.MemoryStream]::new()
$writer = [IO.BinaryWriter]::new($iconStream)
$sizes = @(16,24,32,48,64,128,256)
$images = @()
foreach ($size in $sizes) {
    $visual = [System.Windows.Media.DrawingVisual]::new()
    $drawing = $visual.RenderOpen()
    $drawing.PushTransform([System.Windows.Media.ScaleTransform]::new($size / 100.0, $size / 100.0))
    $brown = [System.Windows.Media.BrushConverter]::new().ConvertFromString('#5B2B19')
    $fur = [System.Windows.Media.BrushConverter]::new().ConvertFromString('#EFBA80')
    $pink = [System.Windows.Media.BrushConverter]::new().ConvertFromString('#EC908B')
    $ink = [System.Windows.Media.Pen]::new($brown,3)
    $ink.StartLineCap = $ink.EndLineCap = [System.Windows.Media.PenLineCap]::Round
    $ink.LineJoin = [System.Windows.Media.PenLineJoin]::Round
    $drawing.DrawGeometry($fur,$ink,[System.Windows.Media.Geometry]::Parse('M13,42 L15,6 36,24 64,24 85,6 88,43 C110,101 -10,101 13,42 Z'))
    $drawing.DrawGeometry($pink,$null,[System.Windows.Media.Geometry]::Parse('M20,17 L31,29 20,36 Z M80,17 L68,29 80,36 Z'))
    $drawing.DrawGeometry($null,$ink,[System.Windows.Media.Geometry]::Parse('M25,47 Q32,56 40,47 M61,47 Q68,56 76,47 M50,64 Q45,76 39,68 M50,64 Q55,76 62,68 M7,57 L28,61 M8,68 L27,65 M73,61 L96,57 M74,65 L95,69'))
    $drawing.DrawGeometry($brown,$null,[System.Windows.Media.Geometry]::Parse('M46,60 L54,60 50,65 Z'))
    $drawing.Pop()
    $drawing.Close()
    $bitmap = [System.Windows.Media.Imaging.RenderTargetBitmap]::new($size,$size,96,96,[System.Windows.Media.PixelFormats]::Pbgra32)
    $bitmap.Render($visual)
    $encoder = [System.Windows.Media.Imaging.PngBitmapEncoder]::new()
    $encoder.Frames.Add([System.Windows.Media.Imaging.BitmapFrame]::Create($bitmap))
    $pngStream = [IO.MemoryStream]::new()
    $encoder.Save($pngStream)
    $images += ,$pngStream.ToArray()
    $pngStream.Dispose()
}
$writer.Write([uint16]0)
$writer.Write([uint16]1)
$writer.Write([uint16]$sizes.Count)
$offset = 6 + 16 * $sizes.Count
for ($index = 0; $index -lt $sizes.Count; $index++) {
    $dimension = if ($sizes[$index] -eq 256) { 0 } else { $sizes[$index] }
    $writer.Write([byte]$dimension)
    $writer.Write([byte]$dimension)
    $writer.Write([byte]0)
    $writer.Write([byte]0)
    $writer.Write([uint16]1)
    $writer.Write([uint16]32)
    $writer.Write([uint32]$images[$index].Length)
    $writer.Write([uint32]$offset)
    $offset += $images[$index].Length
}
foreach ($png in $images) { $writer.Write([byte[]]$png) }
[IO.File]::WriteAllBytes((Join-Path $assetDir 'CatScreen.ico'),$iconStream.ToArray())
$writer.Dispose()
$iconStream.Dispose()
