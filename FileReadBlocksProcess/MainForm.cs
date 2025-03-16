
using System.Diagnostics;

namespace ImportValidateAnalyzeReporter
{
    public partial class MainForm : Form, IProgress<(TimeSpan, int, int)>
    {
        string VeryLargeFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "one-million-guids.txt");
        public MainForm()
        {
            // Make a file with 1 million lines.
            if (!File.Exists(VeryLargeFile)) File.WriteAllText(
                VeryLargeFile,
                string.Join(
                    Environment.NewLine,
                    Enumerable.Range(0, 1000000)
                    .Select(_ => string.Join(
                        " ", 
                        Enumerable.Range(0, 10).Select(_=>$"{Guid.NewGuid()}")))));

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
                labelElapsed.Text = $@"{e.Item1:hh\:mm\:ss\.f}";
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
            var lastUpdate = 0;

            // "The ImportValidateAnalyze() method may import one million lines from a text file..."
            var count = 0;
            var max = 1000000;
            progress.Report((TimeSpan.Zero, count, max));
            using (StreamReader reader = new StreamReader(VeryLargeFile))
            {
                while (count < max)
                {
                    if (await reader.ReadLineAsync(token) is string line)
                    {
                        count++;
                        var currentUpdate = (int)(stopwatch.Elapsed.TotalSeconds * 10);
                        if (checkBoxBreakMe.Checked)
                        {
                            // Without throttling.
                            progress.Report((stopwatch.Elapsed, count, max));
                        }
                        else
                        {
                            if (lastUpdate < currentUpdate)
                            {
                                // Throttle updates to 0.1 second intervals.
                                progress.Report((stopwatch.Elapsed, count, max));
                                lastUpdate = currentUpdate;
                            }
                        }
                    }
                    else break;
                }
            }
            progress.Report((stopwatch.Elapsed, max, max));
        }
    }
}

