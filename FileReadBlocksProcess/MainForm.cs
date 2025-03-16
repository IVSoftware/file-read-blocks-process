
using System.Diagnostics;

namespace ImportValidateAnalyzeReporter
{
    public partial class MainForm : Form, IProgress<(int, int)>
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

            _progress = new Progress<(int, int)>();
            _progress.ProgressChanged += (sender, e) =>
            {
                progressCurrent = e.Item1;
                progressMax = e.Item2;
            };
            InitializeComponent();
            btnAction.Click += btnAction_Click;
        }
        int progressCurrent = 0, progressMax = 0;
        Progress<(int, int)>? _progress;
        public void Report((int, int) value) =>
            ((IProgress <(int, int)>?)_progress)?.Report(value);
        private async void btnAction_Click(object? sender, EventArgs e)
        {
            Task? task = null;
            var cts = new CancellationTokenSource();
            try
            {
                btnAction.Enabled = false;
                var stopwatch = Stopwatch.StartNew();
                labelElapsed.Visible = true;
                task = Task.Run(() =>
                {
                    while (!cts.IsCancellationRequested)
                    {
                        BeginInvoke(() =>
                        {
                            labelElapsed.Text = $@"{stopwatch.Elapsed:hh\:mm\:ss\.f}";
                            Text = $"Main Form {progressCurrent} of {progressMax}";
                        });
                        Thread.Sleep(100);
                    }
                }, cts.Token);
                await ImportValidateAnalyze(this, cts.Token);
            }
            catch (OperationCanceledException)
            { }
            finally
            {
                btnAction.Enabled = true;
                labelElapsed.Visible = false;
                cts.Cancel();
                if (task is not null) await task;
            }
        }
        private async Task ImportValidateAnalyze(IProgress<(int, int)> progress, CancellationToken token)
        {
            var stopwatch = Stopwatch.StartNew();
            var lastUpdate = 0;

            // "The ImportValidateAnalyze() method may import one million lines from a text file..."
            var count = 0;
            var max = 1000000;
            progress.Report((count, max));
            using (StreamReader reader = new StreamReader(VeryLargeFile))
            {
                while (count < max)
                {
                    if (await reader.ReadLineAsync(token) is string line)
                    {
                        count++;
                        progress.Report((count, max));
                        var currentUpdate = (int)(stopwatch.Elapsed.TotalSeconds * 10);
                        if (checkBoxBreakMe.Checked)
                        {
                            // Without throttling.
                        }
                        else
                        {
                            if (lastUpdate < currentUpdate)
                            {
                                // Periodically allow the UI message look catch up
                                await Task.Delay(TimeSpan.FromSeconds(0.1)); 
                                currentUpdate = (int)(stopwatch.Elapsed.TotalSeconds * 10);
                                lastUpdate = currentUpdate;
                            }
                        }
                    }
                    else break;
                }
            }
            progress.Report((max, max));
        }
    }
}

