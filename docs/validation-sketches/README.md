# p5.js API validation sketches

Manual validation fixtures for Phase 7 (full p5.js API coverage). Each sketch
prints one or more `VALIDATION:<module>:true` lines to the console panel when
the corresponding p5.js module works correctly in this app's embedded
WebView2/p5.js runtime, with no console errors.

## How to run

1. `File > Open Sketch...` and pick one of the `.js` files below.
2. Switch to the **Console** tab in the right-hand panel.
3. Confirm every `VALIDATION:...` line reads `true` and no `error`/
   `console-error` lines appear.

| Sketch | Modules covered |
|---|---|
| `sound.js` | p5.sound: Oscillator, SoundFile, Amplitude, FFT, Envelope |
| `webgl.js` | WEBGL 3D renderer, lights, box geometry |
| `vector-table-typeddict.js` | p5.Vector, p5.Table, p5.TypedDict (createNumberDict) |
| `shader.js` | Custom GLSL shaders via createShader() on a WEBGL canvas |
| `dom.js` | p5.dom: createDiv, createButton, select |
| `assets.js` | Local asset loading via loadJSON() and the sketch's own `data/` folder — requires the sketch to be saved to disk first, since asset resolution is relative to the sketch's file path |

`assets.js` needs its sibling `data/config.json` to stay next to it on disk
when saved/opened, since local asset loading resolves relative to the
sketch's file location (see `SketchTabViewModel.FilePath` and
`LocalSketchServer.AssetDirectory`).
