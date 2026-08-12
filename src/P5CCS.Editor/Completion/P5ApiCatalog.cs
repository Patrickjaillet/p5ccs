namespace P5CCS.Editor.Completion;

public static class P5ApiCatalog
{
    public static readonly IReadOnlyList<P5ApiEntry> Entries = new List<P5ApiEntry>
    {
        // Structure
        new("setup", "setup()", "Called once when the program starts. Used to define initial environment properties such as screen size.", "Structure"),
        new("draw", "draw()", "Called directly after setup(), then continuously executes the code within its block until the program is stopped or noLoop() is called.", "Structure"),
        new("preload", "preload()", "Called before setup(). Used to load external assets (images, fonts, sounds, JSON, tables) before the sketch starts.", "Structure"),

        // Environment
        new("createCanvas", "createCanvas(width, height, [renderer])", "Creates a canvas element in the document, sized width x height pixels. Pass WEBGL as renderer for 3D.", "Environment"),
        new("resizeCanvas", "resizeCanvas(width, height)", "Resizes the canvas to the given width and height.", "Environment"),
        new("frameRate", "frameRate([fps])", "Sets or returns the target frame rate of the sketch.", "Environment"),
        new("width", "width", "System variable containing the width of the canvas in pixels.", "Environment"),
        new("height", "height", "System variable containing the height of the canvas in pixels.", "Environment"),
        new("frameCount", "frameCount", "System variable containing the number of frames displayed since the program started.", "Environment"),
        new("windowWidth", "windowWidth", "System variable containing the width of the browser/viewport window.", "Environment"),
        new("windowHeight", "windowHeight", "System variable containing the height of the browser/viewport window.", "Environment"),
        new("pixelDensity", "pixelDensity([val])", "Sets or returns the pixel density of the canvas for high-DPI displays.", "Environment"),

        // Color
        new("background", "background(color)", "Sets the color used for the background of the canvas.", "Color"),
        new("clear", "clear()", "Clears the pixels on the canvas, making every pixel 100% transparent.", "Color"),
        new("colorMode", "colorMode(mode, [max1], [max2], [max3], [maxA])", "Changes the way p5.js interprets color data (RGB, HSB, or HSL).", "Color"),
        new("fill", "fill(color)", "Sets the color used to fill shapes.", "Color"),
        new("noFill", "noFill()", "Disables filling geometry.", "Color"),
        new("stroke", "stroke(color)", "Sets the color used to draw lines and borders around shapes.", "Color"),
        new("noStroke", "noStroke()", "Disables drawing the stroke (outline).", "Color"),
        new("strokeWeight", "strokeWeight(weight)", "Sets the width of the stroke used for lines, points, and the border around shapes, in pixels.", "Color"),
        new("color", "color(gray|r, [g], [b], [a])", "Creates a color from RGB, HSB, or HSL values.", "Color"),
        new("lerpColor", "lerpColor(c1, c2, amt)", "Blends two colors to find a third color between them.", "Color"),

        // Shape (2D)
        new("rect", "rect(x, y, w, [h])", "Draws a rectangle on the canvas.", "Shape"),
        new("ellipse", "ellipse(x, y, w, [h])", "Draws an ellipse (oval) to the canvas.", "Shape"),
        new("circle", "circle(x, y, d)", "Draws a circle to the canvas, given a center point and diameter.", "Shape"),
        new("square", "square(x, y, s)", "Draws a square to the canvas, given the top-left corner and side length.", "Shape"),
        new("line", "line(x1, y1, x2, y2)", "Draws a line (a direct path between two points) on the canvas.", "Shape"),
        new("point", "point(x, y)", "Draws a point, a coordinate in space at the dimension of one pixel.", "Shape"),
        new("triangle", "triangle(x1, y1, x2, y2, x3, y3)", "Draws a triangle to the canvas.", "Shape"),
        new("quad", "quad(x1, y1, x2, y2, x3, y3, x4, y4)", "Draws a quadrilateral (four-sided polygon).", "Shape"),
        new("arc", "arc(x, y, w, h, start, stop, [mode])", "Draws an arc to the canvas.", "Shape"),
        new("beginShape", "beginShape([kind])", "Begins recording vertices for a custom shape.", "Shape"),
        new("endShape", "endShape([mode])", "Stops recording vertices for a custom shape.", "Shape"),
        new("vertex", "vertex(x, y)", "Adds a vertex to a custom shape between beginShape() and endShape().", "Shape"),

        // Transform
        new("translate", "translate(x, y, [z])", "Moves the origin point of the coordinate system.", "Transform"),
        new("rotate", "rotate(angle)", "Rotates the coordinate system.", "Transform"),
        new("scale", "scale(s)", "Scales the coordinate system.", "Transform"),
        new("push", "push()", "Saves the current drawing style settings and transformations.", "Transform"),
        new("pop", "pop()", "Restores the drawing style settings and transformations previously saved by push().", "Transform"),

        // Typography
        new("text", "text(str, x, y)", "Draws text to the canvas.", "Typography"),
        new("textFont", "textFont(font)", "Sets the current font for text rendering.", "Typography"),
        new("textSize", "textSize(size)", "Sets the current font size.", "Typography"),
        new("textAlign", "textAlign(horizAlign, [vertAlign])", "Sets the alignment of text.", "Typography"),
        new("loadFont", "loadFont(path, [callback])", "Loads a font file (.otf/.ttf) and returns a p5.Font, for use with textFont().", "Typography"),

        // Math
        new("random", "random([min], [max])", "Returns a random floating point number.", "Math"),
        new("randomSeed", "randomSeed(seed)", "Sets the seed value for random().", "Math"),
        new("noise", "noise(x, [y], [z])", "Returns a Perlin noise value.", "Math"),
        new("noiseSeed", "noiseSeed(seed)", "Sets the seed value for noise().", "Math"),
        new("map", "map(value, start1, stop1, start2, stop2)", "Re-maps a number from one range to another.", "Math"),
        new("constrain", "constrain(n, low, high)", "Constrains a value between a minimum and maximum value.", "Math"),
        new("dist", "dist(x1, y1, x2, y2)", "Calculates the distance between two points.", "Math"),
        new("lerp", "lerp(start, stop, amt)", "Calculates a number between two numbers at a specific increment.", "Math"),
        new("radians", "radians(degrees)", "Converts a degree measurement to its corresponding value in radians.", "Math"),
        new("degrees", "degrees(radians)", "Converts a radian measurement to its corresponding value in degrees.", "Math"),
        new("min", "min(value1, value2)", "Returns the smallest value in a sequence of numbers.", "Math"),
        new("max", "max(value1, value2)", "Returns the largest value in a sequence of numbers.", "Math"),
        new("floor", "floor(n)", "Rounds a number down to the nearest integer value.", "Math"),
        new("ceil", "ceil(n)", "Rounds a number up to the nearest integer value.", "Math"),
        new("round", "round(n)", "Rounds a number to the nearest integer.", "Math"),
        new("sqrt", "sqrt(n)", "Calculates the square root of a number.", "Math"),
        new("sin", "sin(angle)", "Calculates the sine of an angle.", "Math"),
        new("cos", "cos(angle)", "Calculates the cosine of an angle.", "Math"),

        // Vector & Data
        new("createVector", "createVector([x], [y], [z])", "Creates a new p5.Vector.", "Vector & Data"),
        new("p5.Vector", "p5.Vector", "A class describing a two or three-dimensional vector, used for position, velocity, and acceleration. Static helpers: p5.Vector.add/sub/mult/div/dist/dot/cross/lerp.", "Vector & Data"),
        new("loadJSON", "loadJSON(path, [callback])", "Loads a JSON file from a path (relative to the sketch) and returns an object.", "Vector & Data"),
        new("loadStrings", "loadStrings(path, [callback])", "Loads a text file and returns an array, one entry per line.", "Vector & Data"),
        new("loadTable", "loadTable(path, [extension], [header], [callback])", "Loads a CSV/TSV table file and returns a p5.Table.", "Vector & Data"),
        new("p5.Table", "p5.Table", "Datatype for storing spreadsheet-like data (rows/columns), used with loadTable() or created directly with new p5.Table().", "Vector & Data"),
        new("createStringDict", "createStringDict(key, value)", "Creates a new p5.StringDict, a key/value store for strings.", "Vector & Data"),
        new("createNumberDict", "createNumberDict(key, value)", "Creates a new p5.NumberDict, a key/value store for numbers with arithmetic helpers.", "Vector & Data"),
        new("saveJSON", "saveJSON(json, filename)", "Writes an object to a JSON file, triggering a download.", "Vector & Data"),
        new("saveStrings", "saveStrings(list, filename)", "Writes an array of strings to a text file, triggering a download.", "Vector & Data"),
        new("saveTable", "saveTable(table, filename, [options])", "Writes a p5.Table to a file (CSV/TSV/HTML), triggering a download.", "Vector & Data"),

        // Image
        new("loadImage", "loadImage(path, [callback])", "Loads an image from a path (relative to the sketch) and returns a p5.Image.", "Image"),
        new("image", "image(img, x, y, [w], [h])", "Draws an image to the canvas.", "Image"),
        new("imageMode", "imageMode(mode)", "Sets how images are drawn relative to their position: CORNER, CORNERS, or CENTER.", "Image"),
        new("tint", "tint(color)", "Tints images so that they are drawn with a color blend.", "Image"),
        new("noTint", "noTint()", "Removes the current tint set by tint().", "Image"),

        // Sound (p5.sound addon)
        new("loadSound", "loadSound(path, [callback])", "Loads a sound file (relative to the sketch) and returns a p5.SoundFile. Call in preload() for best results.", "Sound"),
        new("p5.SoundFile", "p5.SoundFile", "An audio file, loaded via loadSound(). Provides play()/stop()/pause()/setVolume()/loop() and more.", "Sound"),
        new("p5.Oscillator", "p5.Oscillator([freq], [type])", "A generator of sine/triangle/square/sawtooth waveforms for synthesis.", "Sound"),
        new("p5.Amplitude", "p5.Amplitude([smoothing])", "Analyzer that tracks the overall volume/amplitude of the audio output.", "Sound"),
        new("p5.FFT", "p5.FFT([smoothing], [bins])", "Fast Fourier Transform analyzer for frequency-spectrum and waveform data.", "Sound"),
        new("p5.Envelope", "p5.Envelope()", "An ADSR envelope generator for shaping the volume of an oscillator or sound over time.", "Sound"),
        new("p5.Noise", "p5.Noise([type])", "A generator of white/pink/brown noise.", "Sound"),
        new("p5.Delay", "p5.Delay()", "An audio delay/echo effect with feedback and filtering.", "Sound"),
        new("p5.Reverb", "p5.Reverb()", "A convolution-based reverb effect.", "Sound"),
        new("p5.Filter", "p5.Filter([type])", "A biquad audio filter (lowpass/highpass/bandpass/notch).", "Sound"),
        new("getAudioContext", "getAudioContext()", "Returns the Web Audio API AudioContext used by p5.sound.", "Sound"),
        new("userStartAudio", "userStartAudio()", "Resumes the audio context after a user gesture, required by browser autoplay policies.", "Sound"),

        // WEBGL / 3D
        new("WEBGL", "WEBGL", "Constant used with createCanvas() to enable 3D WebGL rendering.", "WEBGL / 3D"),
        new("box", "box([width], [height], [depth])", "Draws a 3D box. Requires a WEBGL canvas.", "WEBGL / 3D"),
        new("sphere", "sphere([radius], [detailX], [detailY])", "Draws a 3D sphere. Requires a WEBGL canvas.", "WEBGL / 3D"),
        new("plane", "plane([width], [height])", "Draws a 3D plane. Requires a WEBGL canvas.", "WEBGL / 3D"),
        new("cylinder", "cylinder([radius], [height])", "Draws a 3D cylinder. Requires a WEBGL canvas.", "WEBGL / 3D"),
        new("cone", "cone([radius], [height])", "Draws a 3D cone. Requires a WEBGL canvas.", "WEBGL / 3D"),
        new("torus", "torus([radius], [tubeRadius])", "Draws a 3D torus. Requires a WEBGL canvas.", "WEBGL / 3D"),
        new("ambientLight", "ambientLight(color)", "Adds an ambient light that lights every part of a 3D scene equally.", "WEBGL / 3D"),
        new("directionalLight", "directionalLight(color, x, y, z)", "Adds a light that shines in one direction, like sunlight.", "WEBGL / 3D"),
        new("pointLight", "pointLight(color, x, y, z)", "Adds a light that radiates from a single point.", "WEBGL / 3D"),
        new("normalMaterial", "normalMaterial()", "Sets the current material to a normal-mapped debug material.", "WEBGL / 3D"),
        new("ambientMaterial", "ambientMaterial(color)", "Sets the ambient reflectance for a 3D material.", "WEBGL / 3D"),
        new("specularMaterial", "specularMaterial(color)", "Sets the specular reflectance for a 3D material, used for shiny highlights.", "WEBGL / 3D"),
        new("shininess", "shininess(shine)", "Sets the amount of gloss on a specular material.", "WEBGL / 3D"),
        new("texture", "texture(tex)", "Sets the texture used to fill 3D shapes, from a p5.Image, p5.Graphics, or video.", "WEBGL / 3D"),
        new("camera", "camera(x, y, z, centerX, centerY, centerZ, upX, upY, upZ)", "Sets the position and orientation of the 3D camera.", "WEBGL / 3D"),
        new("perspective", "perspective(fovy, aspect, near, far)", "Sets a perspective projection for the 3D scene.", "WEBGL / 3D"),
        new("ortho", "ortho(left, right, bottom, top, near, far)", "Sets an orthographic projection for the 3D scene.", "WEBGL / 3D"),
        new("orbitControl", "orbitControl()", "Lets the user rotate/pan/zoom the 3D camera with the mouse.", "WEBGL / 3D"),
        new("loadModel", "loadModel(path, [normalize], [callback])", "Loads a 3D model file (.obj/.stl) and returns a p5.Geometry.", "WEBGL / 3D"),
        new("model", "model(geometry)", "Renders a 3D model previously loaded with loadModel().", "WEBGL / 3D"),
        new("loadShader", "loadShader(vertPath, fragPath, [callback])", "Loads a GLSL vertex/fragment shader pair from files and returns a p5.Shader.", "WEBGL / 3D"),
        new("createShader", "createShader(vertSrc, fragSrc)", "Creates a p5.Shader from inline GLSL vertex/fragment source strings.", "WEBGL / 3D"),
        new("shader", "shader(shaderProgram)", "Sets the active shader used to render subsequent shapes.", "WEBGL / 3D"),
        new("resetShader", "resetShader()", "Restores the default material shader.", "WEBGL / 3D"),

        // DOM
        new("createDiv", "createDiv([html])", "Creates a <div> element and returns a p5.Element.", "DOM"),
        new("createButton", "createButton(label, [value])", "Creates a <button> element and returns a p5.Element.", "DOM"),
        new("createSlider", "createSlider(min, max, [value], [step])", "Creates an HTML <input type=\"range\"> slider element.", "DOM"),
        new("createInput", "createInput([default], [type])", "Creates a text <input> element.", "DOM"),
        new("createCapture", "createCapture(type, [callback])", "Creates a capture element for webcam/microphone access. Requires user permission and is not available in the sandboxed local viewport.", "DOM"),
        new("select", "select(selector, [container])", "Searches the DOM for the first matching element and returns a p5.Element.", "DOM"),
        new("selectAll", "selectAll(selector, [container])", "Searches the DOM for all matching elements and returns an array of p5.Element.", "DOM"),

        // Events
        new("mousePressed", "mousePressed()", "Called once after a mouse button is pressed.", "Events"),
        new("mouseReleased", "mouseReleased()", "Called once after a mouse button is released.", "Events"),
        new("mouseDragged", "mouseDragged()", "Called when the mouse moves while a button is pressed.", "Events"),
        new("mouseMoved", "mouseMoved()", "Called every time the mouse moves and a mouse button is not pressed.", "Events"),
        new("mouseX", "mouseX", "System variable containing the current horizontal position of the mouse relative to the canvas.", "Events"),
        new("mouseY", "mouseY", "System variable containing the current vertical position of the mouse relative to the canvas.", "Events"),
        new("keyPressed", "keyPressed()", "Called once every time a key is pressed.", "Events"),
        new("keyReleased", "keyReleased()", "Called once every time a key is released.", "Events"),

        // Control Flow
        new("noLoop", "noLoop()", "Stops p5.js from continuously executing draw().", "Control Flow"),
        new("loop", "loop()", "Resumes continuous execution of draw() after noLoop() was called.", "Control Flow"),
        new("redraw", "redraw([n])", "Executes draw() one or more times, useful with noLoop().", "Control Flow"),

        // Constants
        new("PI", "PI", "Mathematical constant PI (approximately 3.14159).", "Constants"),
        new("TWO_PI", "TWO_PI", "Mathematical constant equal to 2 * PI.", "Constants"),
        new("HALF_PI", "HALF_PI", "Mathematical constant equal to PI / 2.", "Constants"),
        new("CENTER", "CENTER", "Constant used for alignment and shape/image drawing modes.", "Constants"),
        new("RGB", "RGB", "Constant used with colorMode() to select the RGB color model.", "Constants"),
        new("HSB", "HSB", "Constant used with colorMode() to select the HSB color model.", "Constants"),
    };
}
