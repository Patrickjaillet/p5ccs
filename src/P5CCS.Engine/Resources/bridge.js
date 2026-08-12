(function () {
  var lastFrameTime = performance.now();
  var frameCount = 0;
  var fpsAccumMs = 0;

  function post(message) {
    if (window.chrome && window.chrome.webview) {
      window.chrome.webview.postMessage(message);
    }
  }

  function reportFrame() {
    var now = performance.now();
    var deltaMs = now - lastFrameTime;
    lastFrameTime = now;

    frameCount++;
    fpsAccumMs += deltaMs;
    if (fpsAccumMs >= 250) {
      post({ type: 'fps', value: frameCount / (fpsAccumMs / 1000) });
      frameCount = 0;
      fpsAccumMs = 0;
    }

    if (typeof mouseX !== 'undefined' && typeof mouseY !== 'undefined') {
      post({ type: 'mouse', x: mouseX, y: mouseY });
    }
  }

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
        default:
          break;
      }
    });
  }
})();
