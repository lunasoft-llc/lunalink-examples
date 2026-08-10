using LunaLink;
using LunaLink.Examples;

namespace LunaLink.Examples.Node;

internal sealed class MainForm : Form
{
    private readonly NodeClientController _client;
    private readonly NodeState _state;
    private readonly UiLogBuffer _logBuffer;
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1000 };
    private readonly Label _connection = UiTheme.Label("DISCONNECTED", 9, true, UiTheme.Muted);
    private readonly Label _temperature = UiTheme.Label("22.0", 30, true);
    private readonly Label _pressure = UiTheme.Label("2.40", 30, true);
    private readonly Button _connect = UiTheme.Button("Connect", true);
    private readonly Button _disconnect = UiTheme.Button("Disconnect");
    private readonly Button _toggle = UiTheme.Button("Start publishing");
    private readonly RichTextBox _logs = UiTheme.LogBox();
    private readonly Random _random = new();
    private bool _publishing;

    public MainForm(NodeClientController client, NodeState state, UiLogBuffer logs)
    {
        _client = client;
        _state = state;
        _logBuffer = logs;
        UiTheme.Configure(this, "LunaLink — Node Example", new Size(940, 680));

        _disconnect.Enabled = false;
        _toggle.Enabled = false;

        var root = new TableLayoutPanel {
            Dock = DockStyle.Fill, Padding = new Padding(28), ColumnCount = 1, RowCount = 4,
            BackColor = UiTheme.Window
        };
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 176));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 86));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(BuildHeader(), 0, 0);
        root.Controls.Add(BuildMetrics(), 0, 1);
        root.Controls.Add(BuildActions(), 0, 2);
        root.Controls.Add(BuildLogPanel(), 0, 3);
        Controls.Add(root);

        _connect.Click += async (_, _) => await ChangeConnectionAsync(true);
        _disconnect.Click += async (_, _) => await ChangeConnectionAsync(false);
        _toggle.Click += (_, _) => TogglePublishing();
        _timer.Tick += async (_, _) => await PublishAsync();
        logs.MessageAdded += message => OnUi(() => AppendLog(message));
        UpdateValues(22.0, 2.4);
    }

    private Control BuildHeader()
    {
        var header = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, BackColor = UiTheme.Window };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        var titles = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        titles.Controls.Add(UiTheme.Label("LUNALINK  /  EXAMPLES", 8.5f, true, UiTheme.Accent));
        titles.Controls.Add(UiTheme.Label("Telemetry node", 22, true));
        header.Controls.Add(titles, 0, 0);
        var status = UiTheme.Card(new Padding(14, 10, 14, 10));
        status.AutoSize = true;
        status.Controls.Add(_connection);
        header.Controls.Add(status, 1, 0);
        return header;
    }

    private Control BuildMetrics()
    {
        var metrics = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(0, 8, 0, 12) };
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        metrics.Controls.Add(MetricCard("TEMPERATURE", _temperature, "°C", "demo.temperature"), 0, 0);
        metrics.Controls.Add(MetricCard("PRESSURE", _pressure, "bar", "demo.pressure"), 1, 0);
        return metrics;
    }

    private static Control MetricCard(string title, Label value, string unit, string tag)
    {
        var card = UiTheme.Card(new Padding(22));
        card.Dock = DockStyle.Fill;
        card.Margin = title == "TEMPERATURE" ? new Padding(0, 0, 8, 0) : new Padding(8, 0, 0, 0);
        var stack = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        stack.Controls.Add(UiTheme.Label(title, 8.5f, true, UiTheme.Muted));
        var reading = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 8, 0, 6) };
        reading.Controls.Add(value);
        var unitLabel = UiTheme.Label(unit, 13, false, UiTheme.Muted);
        unitLabel.Margin = new Padding(8, 17, 0, 0);
        reading.Controls.Add(unitLabel);
        stack.Controls.Add(reading);
        stack.Controls.Add(UiTheme.Label(tag, 9, false, UiTheme.Muted));
        card.Controls.Add(stack);
        return card;
    }

    private Control BuildActions()
    {
        var panel = UiTheme.Card(new Padding(18, 14, 18, 14));
        panel.Dock = DockStyle.Fill;
        panel.Margin = new Padding(0, 0, 0, 14);
        var buttons = new FlowLayoutPanel { Dock = DockStyle.Left, AutoSize = true, WrapContents = false };
        buttons.Controls.Add(_connect);
        buttons.Controls.Add(_disconnect);
        buttons.Controls.Add(_toggle);
        panel.Controls.Add(buttons);
        var endpoint = UiTheme.Label("127.0.0.1:7788  •  1 second interval", 9, false, UiTheme.Muted);
        endpoint.Dock = DockStyle.Right;
        endpoint.Padding = new Padding(0, 10, 0, 0);
        panel.Controls.Add(endpoint);
        return panel;
    }

    private Control BuildLogPanel()
    {
        var panel = UiTheme.Card(new Padding(1));
        panel.Dock = DockStyle.Fill;
        var shell = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, BackColor = UiTheme.Border };
        shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 40));
        shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var title = UiTheme.Label("ACTIVITY LOG", 8.5f, true, UiTheme.Muted);
        title.Dock = DockStyle.Fill;
        title.BackColor = UiTheme.Surface;
        title.Padding = new Padding(14, 12, 0, 0);
        shell.Controls.Add(title, 0, 0);
        shell.Controls.Add(_logs, 0, 1);
        panel.Controls.Add(shell);
        return panel;
    }

    private async Task ChangeConnectionAsync(bool connect)
    {
        _connect.Enabled = false;
        _disconnect.Enabled = false;
        try
        {
            if (connect) await _client.ConnectAsync(); else await _client.DisconnectAsync();
            if (!connect && _publishing) TogglePublishing();
            SetStatus(connect ? "CONNECTING" : "DISCONNECTED", connect ? UiTheme.Accent : UiTheme.Muted);
            _connect.Enabled = !connect;
            _disconnect.Enabled = connect;
            _toggle.Enabled = connect;
            _logBuffer.Add($"{DateTime.Now:HH:mm:ss} [Information] UI: Client {(connect ? "started" : "disconnected")}");
        }
        catch (Exception ex)
        {
            SetStatus("CONNECTION FAILED", UiTheme.Danger);
            _connect.Enabled = true;
            _toggle.Enabled = false;
            _logBuffer.Add($"{DateTime.Now:HH:mm:ss} [Error] UI: {ex.Message}");
        }
    }

    private void TogglePublishing()
    {
        _publishing = !_publishing;
        _toggle.Text = _publishing ? "Stop publishing" : "Start publishing";
        _toggle.BackColor = _publishing ? UiTheme.Danger : UiTheme.SurfaceAlt;
        _logBuffer.Add($"{DateTime.Now:HH:mm:ss} [Information] UI: Publishing {(_publishing ? "started" : "stopped")}");
        if (_publishing) _timer.Start(); else _timer.Stop();
    }

    private async Task PublishAsync()
    {
        _timer.Stop();
        try
        {
            var temperature = 20 + _random.NextDouble() * 8;
            var pressure = 2 + _random.NextDouble();
            UpdateValues(temperature, pressure);
            var now = DateTimeOffset.UtcNow;
            await _client.SendAsync([
                (NodeState.TemperatureId, "demo.temperature", temperature, LunaLinkQuality.Good, now, LunaLinkDataType.Float64),
                (NodeState.PressureId, "demo.pressure", pressure, LunaLinkQuality.Good, now, LunaLinkDataType.Float64)
            ]);
            SetStatus(_client.IsConnected ? "CONNECTED" : "QUEUED OFFLINE", _client.IsConnected ? UiTheme.Success : UiTheme.Muted);
            _logBuffer.Add($"{DateTime.Now:HH:mm:ss} [Information] Telemetry: temperature={temperature:F1}, pressure={pressure:F2}, connected={_client.IsConnected}");
        }
        catch (Exception ex)
        {
            SetStatus("PUBLISH FAILED", UiTheme.Danger);
            _logBuffer.Add($"{DateTime.Now:HH:mm:ss} [Error] Telemetry: {ex.Message}");
        }
        finally
        {
            if (_publishing) _timer.Start();
        }
    }

    private void SetStatus(string text, Color color) { _connection.Text = $"●  {text}"; _connection.ForeColor = color; }

    private void AppendLog(string message)
    {
        _logs.AppendText(message + Environment.NewLine);
        _logs.SelectionStart = _logs.TextLength;
        _logs.ScrollToCaret();
    }

    private void OnUi(Action action)
    {
        if (IsDisposed) return;
        if (InvokeRequired) BeginInvoke(action); else action();
    }

    private void UpdateValues(double temperature, double pressure)
    {
        var now = DateTimeOffset.UtcNow;
        _temperature.Text = temperature.ToString("F1");
        _pressure.Text = pressure.ToString("F2");
        _state.Set(new LunaLinkTagSnapshot { TagId = NodeState.TemperatureId, TagName = "demo.temperature", Value = temperature, Quality = LunaLinkQuality.Good, Timestamp = now, DataType = LunaLinkDataType.Float64 });
        _state.Set(new LunaLinkTagSnapshot { TagId = NodeState.PressureId, TagName = "demo.pressure", Value = pressure, Quality = LunaLinkQuality.Good, Timestamp = now, DataType = LunaLinkDataType.Float64 });
    }
}
