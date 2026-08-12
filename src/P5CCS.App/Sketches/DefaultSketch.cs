namespace P5CCS.App.Sketches;

public static class DefaultSketch
{
    public const string Source = """
        let x = 400;
        let y = 225;
        let vx = 3;
        let vy = 2;

        function setup() {
          createCanvas(800, 450);
        }

        function draw() {
          background(30);
          x += vx;
          y += vy;
          if (x < 20 || x > width - 20) {
            vx *= -1;
          }
          if (y < 20 || y > height - 20) {
            vy *= -1;
          }
          fill(0, 255, 150);
          noStroke();
          circle(x, y, 40);
        }
        """;
}
