using System.Drawing.Drawing2D;
using AudioFixer.Notifications;
using AudioFixer.Services;

namespace AudioFixer;

internal sealed class AudioFixerContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly System.Windows.Forms.Timer _pollTimer;
    private readonly DeviceService _deviceService = new();
    private readonly ToastService _toastService = new();
    private readonly PowerEventWindow _powerWindow;
    private readonly ToolStripMenuItem _fixMenuItem;
    private readonly ToolStripMenuItem _checkMenuItem;

    private bool _problemDetected;
    private bool _isFixing;

    private static readonly Icon OkIcon = CreateSpeakerIcon(Color.FromArgb(0, 150, 0), false);
    private static readonly Icon ErrorIcon = CreateSpeakerIcon(Color.FromArgb(200, 0, 0), true);

    public AudioFixerContext()
    {
        _checkMenuItem = new ToolStripMenuItem("Check Now", null, OnCheckNow);
        _fixMenuItem = new ToolStripMenuItem("Fix Now", null, OnFixNow) { Enabled = false };

        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add(_checkMenuItem);
        contextMenu.Items.Add(_fixMenuItem);
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("Exit", null, OnExit);

        _trayIcon = new NotifyIcon
        {
            Icon = OkIcon,
            Text = "Audio Fixer - Checking...",
            Visible = true,
            ContextMenuStrip = contextMenu
        };
        _trayIcon.DoubleClick += OnTrayDoubleClick;

        _powerWindow = new PowerEventWindow();
        _powerWindow.ResumeDetected += OnResumeDetected;

        // Initial check
        _ = CheckDevicesAsync();

        // Poll every 30 seconds
        _pollTimer = new System.Windows.Forms.Timer { Interval = 30_000 };
        _pollTimer.Tick += async (_, _) => await CheckDevicesAsync();
        _pollTimer.Start();
    }

    /// <summary>
    /// Called from Program.cs when the "Fix Now" toast button is clicked.
    /// </summary>
    public async Task HandleToastFixAction()
    {
        if (_trayIcon.ContextMenuStrip != null)
        {
            if (_trayIcon.ContextMenuStrip.InvokeRequired)
            {
                await Task.Factory.StartNew(
                    () => PerformFixAsync(),
                    CancellationToken.None,
                    TaskCreationOptions.None,
                    TaskScheduler.FromCurrentSynchronizationContext()).Unwrap();
            }
            else
            {
                await PerformFixAsync();
            }
        }
    }

    private void OnTrayDoubleClick(object? sender, EventArgs e)
    {
        if (_problemDetected && !_isFixing)
            _ = PerformFixAsync();
        else if (!_isFixing)
            _ = CheckDevicesAsync();
    }

    private void OnCheckNow(object? sender, EventArgs e) => _ = CheckDevicesAsync();

    private void OnFixNow(object? sender, EventArgs e) => _ = PerformFixAsync();

    private void OnExit(object? sender, EventArgs e)
    {
        _pollTimer.Stop();
        _pollTimer.Dispose();
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        _powerWindow.DestroyHandle();
        Application.Exit();
    }

    private async void OnResumeDetected()
    {
        // Delay: devices need time to initialize after resume
        await Task.Delay(5000);
        await CheckDevicesAsync();

        // Re-check after 15 seconds for slow devices
        if (!_problemDetected)
        {
            await Task.Delay(10_000);
            await CheckDevicesAsync();
        }
    }

    private async Task CheckDevicesAsync()
    {
        if (_isFixing) return;

        try
        {
            var (hasProblem, affectedCount, totalCount) = await Task.Run(() => _deviceService.GetProblemSummary());
            bool wasOk = !_problemDetected;
            _problemDetected = hasProblem;

            UpdateTrayState(hasProblem, affectedCount, totalCount);

            if (hasProblem && wasOk)
            {
                _toastService.ShowProblemDetected(affectedCount, totalCount);
            }
        }
        catch (Exception ex)
        {
            _trayIcon.Text = $"Audio Fixer - Error: {Truncate(ex.Message, 80)}";
        }
    }

    private async Task PerformFixAsync()
    {
        if (_isFixing) return;
        _isFixing = true;
        _fixMenuItem.Enabled = false;
        _checkMenuItem.Enabled = false;
        _trayIcon.Text = "Audio Fixer - Fixing...";

        try
        {
            await _deviceService.FixAudioDevicesAsync();

            _problemDetected = false;
            UpdateTrayState(false, 0, 0);
            _toastService.ShowFixSuccess();
        }
        catch (Exception ex)
        {
            _trayIcon.Text = $"Audio Fixer - Fix failed: {Truncate(ex.Message, 60)}";
            _toastService.ShowFixFailed(ex.Message);
        }
        finally
        {
            _isFixing = false;
            _checkMenuItem.Enabled = true;
            // Re-check to update state
            await CheckDevicesAsync();
        }
    }

    private void UpdateTrayState(bool hasProblem, int affectedCount, int totalCount)
    {
        if (hasProblem)
        {
            _trayIcon.Icon = ErrorIcon;
            _trayIcon.Text = $"Audio Fixer - Code 43 on {affectedCount} of {totalCount} device(s)";
            _fixMenuItem.Enabled = true;
            _fixMenuItem.Font = new Font(_fixMenuItem.Font, FontStyle.Bold);
        }
        else
        {
            _trayIcon.Icon = OkIcon;
            _trayIcon.Text = "Audio Fixer - All devices OK";
            _fixMenuItem.Enabled = false;
            _fixMenuItem.Font = new Font(_fixMenuItem.Font, FontStyle.Regular);
        }
    }

    private static Icon CreateSpeakerIcon(Color color, bool showWarning)
    {
        var bitmap = new Bitmap(16, 16);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Color.Transparent);

        // Speaker body
        using var brush = new SolidBrush(color);
        g.FillRectangle(brush, 2, 5, 4, 6);

        // Speaker cone
        var cone = new Point[] { new(6, 5), new(10, 2), new(10, 13), new(6, 11) };
        g.FillPolygon(brush, cone);

        if (showWarning)
        {
            // Red X for error
            using var pen = new Pen(Color.Red, 2f);
            g.DrawLine(pen, 11, 4, 15, 8);
            g.DrawLine(pen, 15, 4, 11, 8);
        }
        else
        {
            // Sound waves for OK
            using var pen = new Pen(color, 1.5f);
            g.DrawArc(pen, 11, 4, 4, 8, -60, 120);
        }

        return Icon.FromHandle(bitmap.GetHicon());
    }

    private static string Truncate(string s, int maxLength)
        => s.Length <= maxLength ? s : s[..(maxLength - 3)] + "...";
}

internal sealed class PowerEventWindow : NativeWindow
{
    private const int WM_POWERBROADCAST = 0x0218;
    private const int PBT_APMRESUMEAUTOMATIC = 0x0012;

    public event Action? ResumeDetected;

    public PowerEventWindow()
    {
        CreateHandle(new CreateParams());
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == WM_POWERBROADCAST && m.WParam == (nint)PBT_APMRESUMEAUTOMATIC)
        {
            ResumeDetected?.Invoke();
        }
        base.WndProc(ref m);
    }
}
