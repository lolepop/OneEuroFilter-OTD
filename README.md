Responsive tablet input smoothing using the [1€ filter algorithm](https://gery.casiez.net/1euro/)

# Tuning
- `Min Cutoff`: jitter during slow/small movements (larger = less smoothing)
- `Beta`: responsiveness during large movements (larger = faster adaptation), reduces smoothed and actual position for faster motions
- `Derivative Cutoff`: smoothing parameter for derivative (larger = less smoothing); does not appear to have a large effect