
using System.Diagnostics;

namespace ImportValidateAnalyzeReporter
{
    public partial class MainForm : Form, IProgress<(TimeSpan, int, int)>
    {
        public MainForm()
        {
            InitializeComponent();
            btnAction.Click += btnAction_Click;
        }
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _progress = new Progress<(TimeSpan, int, int)>();
            _progress.ProgressChanged += (sender, e) =>
            {
                Debug.Assert(!InvokeRequired);
                labelElapsed.Text = $@"{e.Item1:hh\:mm\:ss}";
                Text = $"Main Form {e.Item2} of {e.Item3}";
            };
        }
        Progress<(TimeSpan, int, int)>? _progress;
        public void Report((TimeSpan, int, int) value) =>
            ((IProgress <(TimeSpan, int, int)>?)_progress)?.Report(value);
        CancellationTokenSource? _cts = null;
        private async void btnAction_Click(object? sender, EventArgs e)
        {
            try
            {
                btnAction.Enabled = false;
                if (_cts is not null) _cts.Cancel();
                _cts = new CancellationTokenSource();
                labelElapsed.Visible = true;
                await ImportValidateAnalyze(this, _cts.Token);
            }
            catch(OperationCanceledException)
            { }
            finally
            {
                btnAction.Enabled = true;
                labelElapsed.Visible = false;
            }
        }
        private async Task ImportValidateAnalyze(IProgress<(TimeSpan, int, int)> progress, CancellationToken token)
        {
            var stopwatch = Stopwatch.StartNew();
            var lastUpdateSecond = 0;
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "loremipsum.txt");

            // "The ImportValidateAnalyze() method may import one million lines from a text file..."
            var count = 0;
            var max = 1000000;
            progress.Report((TimeSpan.Zero, count, max));
            while (count < max)
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    while (count < max)
                    {
                        if (await reader.ReadLineAsync(token) is string line)
                        {
                            count++;
                            var currentUpdateSecond = (int)stopwatch.Elapsed.TotalSeconds;
                            if (lastUpdateSecond < currentUpdateSecond)
                            {
                                progress.Report((stopwatch.Elapsed, count, max));
                                lastUpdateSecond = currentUpdateSecond;
                            }
                        }
                        else break;
                    }
                }
                // Throttle this loop so that it doesn't consume all the CPU.
                await Task.Delay(100);
            }
            progress.Report((stopwatch.Elapsed, max, max));
        }
    }
}

