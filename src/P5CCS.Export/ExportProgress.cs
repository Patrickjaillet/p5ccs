namespace P5CCS.Export;

public sealed record ExportProgress(int CompletedFrames, int TotalFrames, TimeSpan Elapsed)
{
    public double FractionComplete => TotalFrames == 0 ? 0 : (double)CompletedFrames / TotalFrames;

    public TimeSpan? EstimatedTimeRemaining
    {
        get
        {
            if (CompletedFrames == 0)
            {
                return null;
            }

            var perFrame = Elapsed / CompletedFrames;
            var remainingFrames = TotalFrames - CompletedFrames;
            return perFrame * remainingFrames;
        }
    }
}
