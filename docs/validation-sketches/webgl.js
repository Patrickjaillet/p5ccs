let rot = 0;

function setup() {
  createCanvas(400, 300, WEBGL);
  console.log('VALIDATION:webgl:' + !!this._renderer.isP3D);
}

function draw() {
  background(20);
  rotateX(rot);
  rotateY(rot * 0.7);
  ambientLight(80);
  directionalLight(255, 255, 255, 0, 0, -1);
  fill(0, 255, 150);
  box(100);
  rot += 0.02;
}
