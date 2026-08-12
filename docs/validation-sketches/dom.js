function setup() {
  createCanvas(200, 100);
  var div = createDiv('hello');
  var btn = createButton('click');
  var ok = (typeof div.elt !== 'undefined') && (typeof btn.elt !== 'undefined') && (typeof select === 'function');
  console.log('VALIDATION:dom:' + ok);
}

function draw() {
  background(20);
}
