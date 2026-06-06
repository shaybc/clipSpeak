namespace ClipSpeak;

internal enum MenuIconKind
{
    Settings,
    Speak,
    Help,
    Info,
    Exit
}

internal static class MenuStyling
{
    private static readonly Color BackColor = Color.FromArgb(31, 31, 31);
    private static readonly Color ForeColor = Color.White;
    private static readonly Color SelectionColor = Color.FromArgb(64, 64, 64);
    private static readonly Color BorderColor = Color.FromArgb(83, 83, 83);
    private static readonly Color IconColor = Color.FromArgb(245, 245, 245);
    private static readonly Color AccentColor = Color.FromArgb(42, 166, 255);
    private static readonly Color ExitColor = Color.FromArgb(255, 116, 116);

    public static ContextMenuStrip CreateMenu()
    {
        var menu = new ContextMenuStrip
        {
            Renderer = new DarkMenuRenderer(),
            BackColor = BackColor,
            ForeColor = ForeColor,
            ShowImageMargin = true,
            ShowCheckMargin = false,
            Font = SystemFonts.MenuFont
        };

        menu.Disposed += (_, _) => DisposeMenuImages(menu);
        return menu;
    }

    public static ToolStripMenuItem CreateItem(string text, MenuIconKind iconKind, EventHandler onClick)
    {
        return new ToolStripMenuItem(text, CreateIcon(iconKind), onClick)
        {
            ForeColor = ForeColor,
            ImageScaling = ToolStripItemImageScaling.None,
            Padding = new Padding(4, 5, 18, 5)
        };
    }

    private static Bitmap CreateIcon(MenuIconKind kind)
    {
        var bitmap = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.Clear(Color.Transparent);

        using var iconPen = new Pen(kind == MenuIconKind.Exit ? ExitColor : IconColor, 1.8f)
        {
            StartCap = System.Drawing.Drawing2D.LineCap.Round,
            EndCap = System.Drawing.Drawing2D.LineCap.Round,
            LineJoin = System.Drawing.Drawing2D.LineJoin.Round
        };
        using var iconBrush = new SolidBrush(kind == MenuIconKind.Exit ? ExitColor : IconColor);
        using var accentBrush = new SolidBrush(AccentColor);

        switch (kind)
        {
            case MenuIconKind.Settings:
                graphics.DrawEllipse(iconPen, 4, 4, 8, 8);
                graphics.FillEllipse(iconBrush, 7, 7, 2, 2);
                graphics.DrawLine(iconPen, 8, 1.5f, 8, 3.5f);
                graphics.DrawLine(iconPen, 8, 12.5f, 8, 14.5f);
                graphics.DrawLine(iconPen, 1.5f, 8, 3.5f, 8);
                graphics.DrawLine(iconPen, 12.5f, 8, 14.5f, 8);
                graphics.DrawLine(iconPen, 3.4f, 3.4f, 4.8f, 4.8f);
                graphics.DrawLine(iconPen, 11.2f, 11.2f, 12.6f, 12.6f);
                graphics.DrawLine(iconPen, 12.6f, 3.4f, 11.2f, 4.8f);
                graphics.DrawLine(iconPen, 4.8f, 11.2f, 3.4f, 12.6f);
                break;

            case MenuIconKind.Speak:
                graphics.FillRectangle(accentBrush, 2, 6, 3, 4);
                graphics.FillPolygon(accentBrush, new[] { new Point(5, 5), new Point(9, 2), new Point(9, 14), new Point(5, 11) });
                graphics.DrawArc(iconPen, 9, 5, 4, 6, -45, 90);
                graphics.DrawArc(iconPen, 10, 3, 5, 10, -45, 90);
                break;

            case MenuIconKind.Help:
                graphics.DrawEllipse(iconPen, 2.5f, 2.5f, 11, 11);
                using (var font = new Font(FontFamily.GenericSansSerif, 9, FontStyle.Bold))
                {
                    graphics.DrawString("?", font, iconBrush, 4.3f, 1.1f);
                }
                break;

            case MenuIconKind.Info:
                graphics.DrawEllipse(iconPen, 2.5f, 2.5f, 11, 11);
                graphics.DrawLine(iconPen, 8, 7, 8, 11);
                graphics.FillEllipse(iconBrush, 7.1f, 4.1f, 1.8f, 1.8f);
                break;

            case MenuIconKind.Exit:
                graphics.DrawLine(iconPen, 4, 4, 12, 12);
                graphics.DrawLine(iconPen, 12, 4, 4, 12);
                break;
        }

        return bitmap;
    }

    private static void DisposeMenuImages(ContextMenuStrip menu)
    {
        foreach (ToolStripItem item in menu.Items)
        {
            item.Image?.Dispose();
        }
    }

    private sealed class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkColorTable())
        {
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using var pen = new Pen(BorderColor);
            e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1);
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            using var brush = new SolidBrush(BackColor);
            e.Graphics.FillRectangle(brush, e.AffectedBounds);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            using var pen = new Pen(BorderColor);
            var y = e.Item.Height / 2;
            e.Graphics.DrawLine(pen, 8, y, e.Item.Width - 8, y);
        }
    }

    private sealed class DarkColorTable : ProfessionalColorTable
    {
        public override Color ToolStripDropDownBackground => BackColor;
        public override Color ImageMarginGradientBegin => BackColor;
        public override Color ImageMarginGradientMiddle => BackColor;
        public override Color ImageMarginGradientEnd => BackColor;
        public override Color MenuBorder => BorderColor;
        public override Color MenuItemBorder => SelectionColor;
        public override Color MenuItemSelected => SelectionColor;
        public override Color MenuItemSelectedGradientBegin => SelectionColor;
        public override Color MenuItemSelectedGradientEnd => SelectionColor;
        public override Color MenuItemPressedGradientBegin => SelectionColor;
        public override Color MenuItemPressedGradientMiddle => SelectionColor;
        public override Color MenuItemPressedGradientEnd => SelectionColor;
    }
}
