namespace P5CCS.Core.Sliders;

public static class SliderAnimator
{
    public static double Evaluate(SliderAnimationMode mode, double min, double max, double elapsedSeconds, double periodSeconds)
    {
        if (periodSeconds <= 0)
        {
            periodSeconds = 1;
        }

        return mode switch
        {
            SliderAnimationMode.Oscillate => min + ((max - min) * (0.5 + (0.5 * Math.Sin(2 * Math.PI * elapsedSeconds / periodSeconds)))),
            SliderAnimationMode.Ramp => min + ((max - min) * ((elapsedSeconds % periodSeconds) / periodSeconds)),
            _ => min,
        };
    }
}
