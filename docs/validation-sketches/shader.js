let shaderOk = false;
let myShader;

const vert = `
precision highp float;
attribute vec3 aPosition;
uniform mat4 uModelViewMatrix;
uniform mat4 uProjectionMatrix;
void main() {
  vec4 positionVec4 = vec4(aPosition, 1.0);
  gl_Position = uProjectionMatrix * uModelViewMatrix * positionVec4;
}
`;

const frag = `
precision highp float;
void main() {
  gl_FragColor = vec4(0.0, 1.0, 0.6, 1.0);
}
`;

function setup() {
  createCanvas(200, 200, WEBGL);
  try {
    myShader = createShader(vert, frag);
    shaderOk = true;
  } catch (e) {
    shaderOk = false;
    console.log('VALIDATION:shader-error:' + e.message);
  }
  console.log('VALIDATION:shader:' + shaderOk);
}

function draw() {
  background(20);
  if (shaderOk) {
    shader(myShader);
    noStroke();
    sphere(60);
  }
}
