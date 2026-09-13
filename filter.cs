using System;

namespace OneEuroFilter;

// roughly a port of the c++ implementation located at: https://github.com/casiez/OneEuroFilter/blob/main/CppUsingTemplates/1efilter.cc

class LowPassFilter
{
    public double? PrevFilteredVal { get; private set; }

    public double Filter(double x, double alpha)
    {
        double v = PrevFilteredVal != null ?
            alpha * x + (1.0 - alpha) * PrevFilteredVal.Value :
            x;
        PrevFilteredVal = v;
        return v;
    }
}

public class OneEuroFilter(double frequency, double minimumCutoff, double beta, double derivativeCutoff)
{
    private double frequency = frequency;
    private readonly double minimumCutoff = minimumCutoff;
    private readonly double beta = beta;
    private readonly double derivativeCutoff = derivativeCutoff;

    private bool isFirstRun = true;

    private readonly LowPassFilter signalFilter = new();
    private readonly LowPassFilter dxFilter = new();

    public double Filter(double x, double delta)
    {
        if (!isFirstRun && delta > 0)
            frequency = 1.0 / delta;
        isFirstRun = false;

        double dx = signalFilter.PrevFilteredVal != null ?
            (x - signalFilter.PrevFilteredVal.Value) * frequency :
            0;
        double dxFiltered = dxFilter.Filter(dx, Alpha(derivativeCutoff));
        double cutoff = minimumCutoff + beta * Math.Abs(dxFiltered);
        return signalFilter.Filter(x, Alpha(cutoff));
    }

    private double Alpha(double cutoff)
    {
        double tauInv = 2.0 * Math.PI * cutoff;
        return tauInv / (tauInv + frequency);
    }
}

public class OneEuroFilterTuple(double frequency, double minimumCutoff, double beta, double derivativeCutoff)
{
    private readonly OneEuroFilter xFilter = new(frequency, minimumCutoff, beta, derivativeCutoff);
    private readonly OneEuroFilter yFilter = new(frequency, minimumCutoff, beta, derivativeCutoff);

    public (double, double) Filter((double, double) pos, double delta)
    {
        var (x, y) = pos;
        double xOut = xFilter.Filter(x, delta);
        double yOut = yFilter.Filter(y, delta);
        return (xOut, yOut);
    }
}