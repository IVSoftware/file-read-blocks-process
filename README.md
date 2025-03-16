**New! Improved!**

Agreed, my original answer didn't quite nail it. Today I woke up fresh with a **_new plausible and provable theory of failure_** about what could give the appearance of a thread being blocked during the execution of a process that the OP states _"may import one million lines from a text file"_. 

My new theory of failure is based on the idea that when when long tasks are offloaded to another thread, if they're intense enough, they can still hog all the CPU power. This doesn't block the UI thread directly, but it does choke off the resources the UI needs to update smoothly or at all. You mention the `ImportValidateAnalyze(...)` method specifically. If we read those one million lines with a `ReadLineAsync()` as shown in the snippet, one might think that would allow the loop to sufficiently breathe. But I have two MREs that show this isn't necessarily so.

- The first MRE uses a properly provisioned `Progress` class.
- The second MRE uses an externasl timer update like the original OP.
- Both examples show how one can make it work. You're right. It _"doesn't seem like too much to ask"!_
- Both examples can be shown to "freeze the UI" if the loop that reads the one million lines is run too tightly.
- We can even put some logging or a `DebugWrite` to see that the update requests are in fact still being received (i.e. thread isn't blocked per se).

The latest comment clarified:

> The "real problem" is that an application that runs a 10 minute method appears dead! I just want to post smething in the ui to let the user know it is still alive, and to wait for completion. 

So, here are two minimal examples showing different means of achieving that, that also demonstrate deterministic failures when the background loop runs too tightly.

___

**Minimal Reproducible Example #1 (using `Progress` class)**

As stated in the comment by Panagiotis Kanavos, on one hand this seems like a straightforward application for the `Progress` class. So let's "do everything right" in that respect and set up a minimal reproducible example that does a `ReadLineAsync()` one million times and attempts to update progress. The snippet below shows one way to set this up properly, but I've also provided an option to deliberately "break" the update mechanism.


```
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
        _progress = new Progress<(TimeSpan, int, int)>();
        Debug.Assert(SynchronizationContext.Current != null);
        _progress.ProgressChanged += (sender, e) =>
        {
            Debug.Assert(!InvokeRequired);
            labelElapsed.Text = $@"{e.Item1:hh\:mm\:ss\.f}";
            Text = $"Main Form {e.Item2} of {e.Item3}";

            // Uncomment to show that messages 'are' received but do not update the label.
            // Debug.WriteLine($"{e.Item2} of {e.Item3}");
        };

        InitializeComponent();
        btnAction.Click += btnAction_Click;
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
```

___


**Minimal Reproducible Example #2 (using external update timer like OP)**

```
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
```