using LunaLink.Examples;

namespace LunaLink.Examples.Master;

internal sealed class MainForm : Form
{
    private readonly Label _status = UiTheme.Label("●  STOPPED", 9, true, UiTheme.Muted);
    private readonly Label _node = UiTheme.Label("No node connected", 9, false, UiTheme.Muted);
    private readonly Label _tagCount = UiTheme.Label("0", 20, true);
    private readonly Button _start = UiTheme.Button("Start listening", true);
    private readonly Button _stop = UiTheme.Button("Stop listening");
    private readonly DataGridView _grid = new();
    private readonly RichTextBox _logs = UiTheme.LogBox();
    private readonly Dictionary<Guid, int> _rows = [];

    public MainForm(MasterEvents events, UiLogBuffer logs, MasterServerController server)
    {
        UiTheme.Configure(this, "LunaLink — Master Example", new Size(1080, 720));
        _stop.Enabled = false;
        ConfigureGrid();

        var root = new TableLayoutPanel {
            Dock = DockStyle.Fill, Padding = new Padding(28), ColumnCount = 1, RowCount = 4,
            BackColor = UiTheme.Window
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 62));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 38));
        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildToolbar(), 0, 1);
        root.Controls.Add(BuildDataPanel(), 0, 2);
        root.Controls.Add(BuildLogPanel(), 0, 3);
        Controls.Add(root);

        _start.Click += async (_, _) => await ChangeListeningAsync(server, true, logs);
        _stop.Click += async (_, _) => await ChangeListeningAsync(server, false, logs);
        events.NodeConnected += message => OnUi(() => {
            _node.Text = message;
            SetStatus("NODE CONNECTED", UiTheme.Success);
        });
        events.TagReceived += reading => OnUi(() => ShowReading(reading));
        logs.MessageAdded += message => OnUi(() => AppendLog(message));
    }

    private Control BuildHeader()
    {
        var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = UiTheme.Window };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var titles = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        titles.Controls.Add(UiTheme.Label("LUNALINK  /  EXAMPLES", 8.5f, true, UiTheme.Accent));
        titles.Controls.Add(UiTheme.Label("Master station", 22, true));
        header.Controls.Add(titles, 0, 0);
        var status = UiTheme.Card(new Padding(14, 10, 14, 10));
        status.AutoSize = true;
        status.Controls.Add(_status);
        header.Controls.Add(status, 1, 0);
        return header;
    }

    private Control BuildToolbar()
    {
        var card = UiTheme.Card(new Padding(18, 14, 18, 14));
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0, 0, 0, 12);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Left, AutoSize = true, WrapContents = false };
        buttons.Controls.Add(_start);
        buttons.Controls.Add(_stop);
        card.Controls.Add(buttons);
        var details = new FlowLayoutPanel { Dock = DockStyle.Right, AutoSize = true, WrapContents = false };
        _node.Margin = new Padding(0, 10, 20, 0);
        details.Controls.Add(_node);
        var endpoint = UiTheme.Label("TCP  •  0.0.0.0:7788", 9, true, UiTheme.Muted);
        endpoint.Margin = new Padding(0, 10, 0, 0);
        details.Controls.Add(endpoint);
        card.Controls.Add(details);
        return card;
    }

    private Control BuildDataPanel()
    {
        var card = UiTheme.Card(new Padding(1));
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0, 0, 0, 12);
        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = UiTheme.Border };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var header = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.Surface };
        var title = UiTheme.Label("LIVE TAGS", 8.5f, true, UiTheme.Muted);
        title.Location = new Point(14, 16);
        header.Controls.Add(title);
        _tagCount.Dock = DockStyle.Right;
        _tagCount.Padding = new Padding(0, 7, 16, 0);
        header.Controls.Add(_tagCount);
        shell.Controls.Add(header, 0, 0);
        shell.Controls.Add(_grid, 0, 1);
        card.Controls.Add(shell);
        return card;
    }

    private Control BuildLogPanel()
    {
        var card = UiTheme.Card(new Padding(1));
        card.Dock = DockStyle.Fill;
        card.Margin = new Padding(0, 0, 0, 0);
        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = UiTheme.Border };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var title = UiTheme.Label("ACTIVITY LOG", 8.5f, true, UiTheme.Muted);
        title.Dock = DockStyle.Fill;
        title.BackColor = UiTheme.Surface;
        title.Padding = new Padding(14, 12, 0, 0);
        shell.Controls.Add(title, 0, 0);
        shell.Controls.Add(_logs, 0, 1);
        card.Controls.Add(shell);
        return card;
    }

    private void ConfigureGrid()
    {
        _grid.Dock = DockStyle.Fill;
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AllowUserToResizeRows = false;
        _grid.RowHeadersVisible = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BackgroundColor = UiTheme.Surface;
        _grid.BorderStyle = BorderStyle.None;
        _grid.GridColor = UiTheme.Border;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersHeight = 40;
        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle {
            BackColor = UiTheme.SurfaceAlt, ForeColor = UiTheme.Muted, Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            SelectionBackColor = UiTheme.SurfaceAlt, Alignment = DataGridViewContentAlignment.MiddleLeft
        };
        _grid.DefaultCellStyle = new DataGridViewCellStyle {
            BackColor = UiTheme.Surface, ForeColor = UiTheme.Text, SelectionBackColor = Color.FromArgb(29, 78, 125),
            SelectionForeColor = UiTheme.Text, Padding = new Padding(5)
        };
        _grid.RowTemplate.Height = 38;
        _grid.Columns.Add("Name", "TAG");
        _grid.Columns.Add("Value", "VALUE");
        _grid.Columns.Add("Quality", "QUALITY");
        _grid.Columns.Add("DataType", "TYPE");
        _grid.Columns.Add("Timestamp", "TIMESTAMP (UTC)");
        _grid.Columns[0].FillWeight = 130;
        _grid.Columns[4].FillWeight = 145;
    }

    private async Task ChangeListeningAsync(MasterServerController server, bool start, UiLogBuffer logs)
    {
        _start.Enabled = false;
        _stop.Enabled = false;
        try
        {
            if (start) await server.StartAsync(); else await server.StopAsync();
            SetStatus(start ? "LISTENING  •  7788" : "STOPPED", start ? UiTheme.Accent : UiTheme.Muted);
            if (!start) _node.Text = "No node connected";
            _start.Enabled = !start;
            _stop.Enabled = start;
        }
        catch (Exception ex)
        {
            SetStatus("LISTENER FAILED", UiTheme.Danger);
            _start.Enabled = true;
            logs.Add($"{DateTime.Now:HH:mm:ss} [Error] UI: {ex.Message}");
        }
    }

    private void SetStatus(string text, Color color) { _status.Text = $"●  {text}"; _status.ForeColor = color; }

    private void AppendLog(string message)
    {
        _logs.AppendText(message + Environment.NewLine);
        _logs.SelectionStart = _logs.TextLength;
        _logs.ScrollToCaret();
    }

    private void ShowReading(TagReading reading)
    {
        object[] values = { reading.Name, reading.Value ?? string.Empty, reading.Quality, reading.DataType, reading.Timestamp.UtcDateTime.ToString("HH:mm:ss.fff") };
        if (_rows.TryGetValue(reading.Id, out var index))
            for (var i = 0; i < values.Length; i++) _grid.Rows[index].Cells[i].Value = values[i];
        else
            _rows[reading.Id] = _grid.Rows.Add(values);
        _tagCount.Text = _rows.Count.ToString();
    }

    private void OnUi(Action action)
    {
        if (IsDisposed) return;
        if (InvokeRequired) BeginInvoke(action); else action();
    }
}
