using P5CCS.Core.Sliders;

namespace P5CCS.Core.Tests.Sliders;

public class SliderAnimatorTests
{
    [Fact]
    public void Evaluate_NoneMode_ReturnsMin()
    {
        var value = SliderAnimator.Evaluate(SliderAnimationMode.None, 0, 100, 5, 2);

        Assert.Equal(0, value);
    }

    [Fact]
    public void Evaluate_Oscillate_AtQuarterPeriod_ReturnsMax()
    {
        var value = SliderAnimator.Evaluate(SliderAnimationMode.Oscillate, 0, 100, 0.5, 2);

        Assert.Equal(100, value, precision: 6);
    }

    [Fact]
    public void Evaluate_Oscillate_StaysWithinBounds()
    {
        for (var t = 0.0; t < 4; t += 0.1)
        {
            var value = SliderAnimator.Evaluate(SliderAnimationMode.Oscillate, 10, 20, t, 3);
            Assert.InRange(value, 10, 20);
        }
    }

    [Fact]
    public void Evaluate_Ramp_AtStartOfPeriod_ReturnsMin()
    {
        var value = SliderAnimator.Evaluate(SliderAnimationMode.Ramp, 0, 10, 0, 5);

        Assert.Equal(0, value, precision: 6);
    }

    [Fact]
    public void Evaluate_Ramp_AtHalfPeriod_ReturnsMidpoint()
    {
        var value = SliderAnimator.Evaluate(SliderAnimationMode.Ramp, 0, 10, 2.5, 5);

        Assert.Equal(5, value, precision: 6);
    }

    [Fact]
    public void Evaluate_Ramp_WrapsAfterFullPeriod()
    {
        var value = SliderAnimator.Evaluate(SliderAnimationMode.Ramp, 0, 10, 5.5, 5);

        Assert.Equal(1, value, precision: 6);
    }
}
