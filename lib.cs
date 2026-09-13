using System;
using System.Diagnostics;
using OpenTabletDriver.Plugin.Attributes;
using OpenTabletDriver.Plugin.Output;
using OpenTabletDriver.Plugin.Tablet;

namespace OneEuroFilter;

[PluginName("One Euro Filter")]
public class Plugin : IPositionedPipelineElement<IDeviceReport>
{
    public PipelinePosition Position => PipelinePosition.PreTransform;

    [Property("Min Cutoff"), DefaultPropertyValue(Default.MIN_CUTOFF)]
    public double MinFreqCutoff { get; set; } = Default.MIN_CUTOFF;

    [Property("Beta"), DefaultPropertyValue(Default.BETA)]
    public double Beta { get; set; } = Default.BETA;

    [Property("Derivative Cutoff"), DefaultPropertyValue(Default.DERIVATIVE_CUTOFF)]
    public double DerivativeCutoff { get; set; } = Default.DERIVATIVE_CUTOFF;

    [Property("Tablet Internal Report Rate (hz)"), DefaultPropertyValue(Default.REPORT_RATE)]
    public double ReportRate { get; set; } = Default.REPORT_RATE;

    private readonly Stopwatch timer = Stopwatch.StartNew();
    private long? lastReport;
    private OneEuroFilterTuple? filter;

    public event Action<IDeviceReport>? Emit;

    public void Consume(IDeviceReport value)
    {
        filter ??= new(ReportRate, MinFreqCutoff, Beta, DerivativeCutoff);
        if (value is not ITabletReport tablet)
        {
            Emit?.Invoke(value);
            return;
        }

        long now = timer.ElapsedTicks;
        double deltaLastReport = (double)(now - lastReport ?? now) / Stopwatch.Frequency;
        lastReport = now;
        var pos = tablet.Position;
        var (nextX, nextY) = filter.Filter((pos.X, pos.Y), deltaLastReport);

        tablet.Position = new((float)nextX, (float)nextY);
        Emit?.Invoke(tablet);
    }
}
