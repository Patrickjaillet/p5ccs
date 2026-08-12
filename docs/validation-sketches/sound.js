function setup() {
  createCanvas(200, 100);
  var ok = (typeof p5.Oscillator === 'function') &&
    (typeof p5.SoundFile === 'function') &&
    (typeof p5.Amplitude === 'function') &&
    (typeof p5.FFT === 'function') &&
    (typeof p5.Envelope === 'function');
  console.log('VALIDATION:sound:' + ok);
}

function draw() {
  background(20);
}
