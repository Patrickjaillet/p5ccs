function setup() {
  createCanvas(200, 100);

  var v1 = createVector(1, 2, 3);
  var v2 = createVector(4, 5, 6);
  var vSum = p5.Vector.add(v1, v2);
  var vectorOk = (vSum.x === 5 && vSum.y === 7 && vSum.z === 9);

  var table = new p5.Table();
  table.addColumn('name');
  table.addColumn('score');
  var row = table.addRow();
  row.setString('name', 'alice');
  row.setNum('score', 42);
  var tableOk = (table.getRowCount() === 1 && table.getString(0, 'name') === 'alice' && table.getNum(0, 'score') === 42);

  var dict = createNumberDict('a', 1);
  dict.create('b', 2);
  var dictOk = (dict.size() === 2 && dict.get('a') === 1 && dict.get('b') === 2);

  console.log('VALIDATION:vector:' + vectorOk);
  console.log('VALIDATION:table:' + tableOk);
  console.log('VALIDATION:typeddict:' + dictOk);
}

function draw() {
  background(20);
}
