namespace LunaLink.Examples;

internal static class UiTheme
{
    public static readonly Color Window = Color.FromArgb(12, 17, 24);
    public static readonly Color Surface = Color.FromArgb(20, 27, 37);
    public static readonly Color SurfaceAlt = Color.FromArgb(25, 34, 46);
    public static readonly Color Border = Color.FromArgb(47, 60, 76);
    public static readonly Color Text = Color.FromArgb(238, 243, 248);
    public static readonly Color Muted = Color.FromArgb(148, 163, 184);
    public static readonly Color Accent = Color.FromArgb(46, 144, 250);
    public static readonly Color AccentHover = Color.FromArgb(66, 164, 255);
    public static readonly Color Success = Color.FromArgb(50, 213, 131);
    public static readonly Color Danger = Color.FromArgb(248, 113, 113);

    public static Button Button(string text, bool primary = false)
    {
        var button = new Button {
            Text = text, AutoSize = true, Height = 38, Padding = new Padding(16, 0, 16, 0),
            FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
            BackColor = primary ? Accent : SurfaceAlt,
            ForeColor = Text, Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Margin = new Padding(0, 0, 10, 0)
        };
        button.FlatAppearance.BorderColor = primary ? Accent : Border;
        button.FlatAppearance.MouseOverBackColor = primary ? AccentHover : Color.FromArgb(35, 46, 60);
        button.FlatAppearance.MouseDownBackColor = Color.FromArgb(33, 116, 209);
        return button;
    }

    public static Label Label(string text, float size = 9.5f, bool bold = false, Color? color = null) => new() {
        Text = text, AutoSize = true, ForeColor = color ?? Text,
        Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
        BackColor = Color.Transparent
    };

    public static Panel Card(Padding? padding = null) => new() {
        BackColor = Surface, Padding = padding ?? new Padding(20), Margin = new Padding(0)
    };

    public static RichTextBox LogBox() => new() {
        Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.FromArgb(8, 12, 18),
        ForeColor = Color.FromArgb(190, 203, 218), Font = new Font("Cascadia Mono", 9),
        BorderStyle = BorderStyle.None, DetectUrls = false, WordWrap = false, Padding = new Padding(8)
    };

    public static void Configure(Form form, string title, Size size)
    {
        form.Text = title;
        form.ClientSize = size;
        form.MinimumSize = new Size(760, 560);
        form.StartPosition = FormStartPosition.CenterScreen;
        form.BackColor = Window;
        form.ForeColor = Text;
        form.Font = new Font("Segoe UI", 9.5f);
        form.AutoScaleMode = AutoScaleMode.Dpi;
    }
}
