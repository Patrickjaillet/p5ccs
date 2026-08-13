(function () {
  var originalRequestAnimationFrame = window.requestAnimationFrame.bind(window);
  var onFrameCallbacks = [];

  window.__p5ccsOnFrame = function (callback) {
    onFrameCallbacks.push(callback);
  };

  window.requestAnimationFrame = function (callback) {
    return originalRequestAnimationFrame(function (timestamp) {
      callback(timestamp);
      for (var i = 0; i < onFrameCallbacks.length; i++) {
        onFrameCallbacks[i](timestamp);
      }
    });
  };
})();
