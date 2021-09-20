using System;
using System.Collections.Generic;
using System.Linq;

public class MovingAverage
{
    private Queue<Decimal> samples = new Queue<Decimal>();
    private int windowSize = 16;
    private Decimal sampleAccumulator;
    public Decimal Average { get; private set; }

    public MovingAverage(int windowSize)
    {
        this.windowSize = windowSize;
    }

    /// <summary>
    /// Computes a new windowed average each time a new sample arrives
    /// </summary>
    /// <param name="newSample"></param>
    public void ComputeAverage(Decimal newSample)
    {
        sampleAccumulator += newSample;
        samples.Enqueue(newSample);

        if (samples.Count > windowSize)
        {
            sampleAccumulator -= samples.Dequeue();
        }

        Average = sampleAccumulator / samples.Count;
    }

    public double StandardDeviation()
    {
        double result = 0;

        if (samples.Any())
        {
            double average = (double)samples.Average();
            double sum = samples.Sum(d => Math.Pow((double)d - average, 2));
            result = Math.Sqrt((sum) / (samples.Count() - 1));
        }
        return result;
    }
}