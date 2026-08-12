(function () {
  var lastFrameTime = performance.now();
  var frameCount = 0;
  var fpsAccumMs = 0;

  var gridEnabled = false;
  var gridSpacing = 50;
  var hudGridCanvas = document.getElementById('hud-grid');
  var hudGridCtx = hudGridCanvas ? hudGridCanvas.getContext('2d') : null;
  var hudFpsEl = document.getElementById('hud-fps');

  function post(message) {
    if (window.chrome && window.chrome.webview) {
      window.chrome.webview.postMessage(message);
    }
  }

  function findSketchCanvas() {
    var canvases = document.getElementsByTagName('canvas');
    for (var i = 0; i < canvases.length; i++) {
      if (canvases[i] !== hudGridCanvas) {
        return canvases[i];
      }
    }
    return null;
  }

  function syncGridCanvas() {
    var target = findSketchCanvas();
    if (!target || !hudGridCanvas) {
      return;
    }

    var rect = target.getBoundingClientRect();
    var width = Math.round(rect.width);
    var height = Math.round(rect.height);

    if (hudGridCanvas.width !== width || hudGridCanvas.height !== height) {
      hudGridCanvas.width = width;
      hudGridCanvas.height = height;
    }

    hudGridCanvas.style.left = rect.left + 'px';
    hudGridCanvas.style.top = rect.top + 'px';
  }

  function drawGrid() {
    if (!hudGridCtx || !hudGridCanvas) {
      return;
    }

    syncGridCanvas();
    hudGridCtx.clearRect(0, 0, hudGridCanvas.width, hudGridCanvas.height);

    if (!gridEnabled) {
      return;
    }

    hudGridCtx.strokeStyle = 'rgba(255, 255, 255, 0.25)';
    hudGridCtx.lineWidth = 1;

    for (var x = 0; x <= hudGridCanvas.width; x += gridSpacing) {
      hudGridCtx.beginPath();
      hudGridCtx.moveTo(x + 0.5, 0);
      hudGridCtx.lineTo(x + 0.5, hudGridCanvas.height);
      hudGridCtx.stroke();
    }

    for (var y = 0; y <= hudGridCanvas.height; y += gridSpacing) {
      hudGridCtx.beginPath();
      hudGridCtx.moveTo(0, y + 0.5);
      hudGridCtx.lineTo(hudGridCanvas.width, y + 0.5);
      hudGridCtx.stroke();
    }
  }

  function reportFrame() {
    var now = performance.now();
    var deltaMs = now - lastFrameTime;
    lastFrameTime = now;

    frameCount++;
    fpsAccumMs += deltaMs;
    if (fpsAccumMs >= 250) {
      var fps = frameCount / (fpsAccumMs / 1000);
      post({ type: 'fps', value: fps });
      if (hudFpsEl) {
        hudFpsEl.textContent = Math.round(fps) + ' FPS';
      }
      frameCount = 0;
      fpsAccumMs = 0;
    }

    if (gridEnabled) {
      drawGrid();
    }

    if (typeof mouseX !== 'undefined' && typeof mouseY !== 'undefined') {
      post({ type: 'mouse', x: mouseX, y: mouseY });
    }
  }

  window.addEventListener('resize', drawGrid);

  window.addEventListener('error', function (event) {
    post({ type: 'error', message: event.message + ' at ' + event.filename + ':' + event.lineno });
  });

  var originalLog = console.log;
  console.log = function () {
    post({ type: 'console', message: Array.prototype.slice.call(arguments).join(' ') });
    originalLog.apply(console, arguments);
  };

  var originalError = console.error;
  console.error = function () {
    post({ type: 'console-error', message: Array.prototype.slice.call(arguments).join(' ') });
    originalError.apply(console, arguments);
  };

  var readyCheck = setInterval(function () {
    if (typeof p5 !== 'undefined' && p5.prototype) {
      clearInterval(readyCheck);

      var originalRedraw = p5.prototype.redraw;
      p5.prototype.redraw = function () {
        originalRedraw.apply(this, arguments);
        reportFrame();
      };

      post({ type: 'ready' });
      setTimeout(drawGrid, 50);
    }
  }, 10);

  if (window.chrome && window.chrome.webview) {
    window.chrome.webview.addEventListener('message', function (event) {
      var data = event.data;
      if (!data || !data.command) {
        return;
      }

      switch (data.command) {
        case 'loop':
          if (typeof loop === 'function') loop();
          break;
        case 'noLoop':
          if (typeof noLoop === 'function') noLoop();
          break;
        case 'redraw':
          if (typeof redraw === 'function') redraw();
          break;
        case 'setFrameRate':
          if (typeof frameRate === 'function') frameRate(data.value);
          break;
        case 'setVariable':
          if (typeof data.name === 'string') window[data.name] = data.value;
          break;
        case 'showGrid':
          gridEnabled = !!data.value;
          drawGrid();
          break;
        default:
          break;
      }
    });
  }
})();
