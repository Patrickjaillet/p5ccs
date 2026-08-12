let config;

function preload() {
  config = loadJSON('data/config.json');
}

function setup() {
  createCanvas(400, 300);
}

function draw() {
  background(20);
  fill(0, 255, 150);
  textSize(24);
  text('label: ' + config.label, 20, 40);
  text('value: ' + config.value, 20, 80);
  console.log('VALIDATION:assets:' + (config.label === 'asset-loaded-ok' && config.value === 42));
  noLoop();
}
