// Offline asset preparation. Node.js + FFmpeg with libvpx-vp9; no web dependency.
// node tools/media/prepare-companion-videos.cjs --input "C:/.../animation charactor" --ffmpeg "C:/.../ffmpeg.exe"
const fs = require('node:fs');
const path = require('node:path');
const crypto = require('node:crypto');
const { execFileSync, spawn } = require('node:child_process');
const { once } = require('node:events');

const option = name => process.argv[process.argv.indexOf(name) + 1];
const input = process.argv.includes('--input') && option('--input');
const ffmpeg = process.argv.includes('--ffmpeg') ? option('--ffmpeg') : 'ffmpeg';
if (!input) throw new Error('Pass --input with the original video directory. Originals are never modified.');
const output = path.resolve(__dirname, '../../src/BranchFlow.Web/wwwroot/images/christmas/characters/companions');
const sources = [
    ['ginger', 'santa', 'humancookie/kling_20261105_VIDEO_TASK__CREA_3169_0.mp4'],
    ['ginger', 'reindeer', 'humancookie/kling_20261105_VIDEO_TASK__CREA_3236_0.mp4'],
    ['ginger', 'barista', 'humancookie/kling_20261105_VIDEO_TASK__CREA_3240_0.mp4'],
    ['santa', 'reindeer', 'satan/gemini_generated_video_8fb1b225.mp4']
];
const width = 640, height = 360, pixels = width * height;
const outWidth = 360, outHeight = 480, fps = 24;

function isolate(rgb) {
    const mask = new Uint8Array(pixels);
    const seen = new Uint8Array(pixels);
    const queue = new Int32Array(pixels);
    // Pink varies in brightness between clips. Key its hue rather than one RGB value.
    const darkBackdrop = rgb[0] < 70 && rgb[1] < 20 && rgb[2] < 40;
    for (let i = 0; i < pixels; i++) {
        const r = rgb[i * 3], g = rgb[i * 3 + 1], b = rgb[i * 3 + 2];
        const rg = r - g, bg = b - g, ratio = bg / Math.max(1, rg);
        const pink = rg > 22 && bg > 10 && ratio > .29 && ratio < .92;
        mask[i] = pink ? 0 : 1;
    }
    // The barista clip fades from black to pink. Flood only edge-connected black,
    // preserving dark details inside clothing and hair.
    if (darkBackdrop) {
        let head = 0, tail = 0;
        const enqueue = i => {
            if (seen[i]) return;
            const r = rgb[i * 3], g = rgb[i * 3 + 1], b = rgb[i * 3 + 2];
            if (Math.max(r, g, b) > 54 || g > 25) return;
            seen[i] = 1; queue[tail++] = i; mask[i] = 0;
        };
        for (let x = 0; x < width; x++) { enqueue(x); enqueue((height - 1) * width + x); }
        for (let y = 0; y < height; y++) { enqueue(y * width); enqueue(y * width + width - 1); }
        while (head < tail) {
            const i = queue[head++], x = i % width;
            if (x) enqueue(i - 1); if (x < width - 1) enqueue(i + 1);
            if (i >= width) enqueue(i - width); if (i < pixels - width) enqueue(i + width);
        }
    }
    // Discard isolated background speckles; retain characters and their cup steam.
    seen.fill(0);
    for (let i = 0; i < pixels; i++) {
        if (!mask[i] || seen[i]) continue;
        let head = 0, tail = 1; queue[0] = i; seen[i] = 1;
        const enqueue = j => { if (mask[j] && !seen[j]) { seen[j] = 1; queue[tail++] = j; } };
        while (head < tail) {
            const j = queue[head++], x = j % width;
            if (x) enqueue(j - 1); if (x < width - 1) enqueue(j + 1);
            if (j >= width) enqueue(j - width); if (j < pixels - width) enqueue(j + width);
        }
        if (tail < 300) {
            let upper = height;
            for (let j = 0; j < tail; j++) upper = Math.min(upper, Math.floor(queue[j] / width));
            // Keep detached cup steam and Santa's illustrated calling strokes.
            if (tail < 40 || upper >= height * .8)
                for (let j = 0; j < tail; j++) mask[queue[j]] = 0;
        }
    }
    const rgba = Buffer.alloc(pixels * 4);
    let left = width, right = 0, top = height, bottom = 0;
    for (let y = 1; y < height - 1; y++) for (let x = 1; x < width - 1; x++) {
        const i = y * width + x;
        if (!mask[i]) continue;
        let neighbors = 0;
        for (let dy = -1; dy <= 1; dy++) for (let dx = -1; dx <= 1; dx++) neighbors += mask[i + dy * width + dx];
        // A one-pixel feather suppresses compression fringes at sidebar scale.
        const alpha = Math.round(255 * Math.min(1, Math.max(0, (neighbors - 3) / 6)));
        if (!alpha) continue;
        rgba[i * 4] = rgb[i * 3]; rgba[i * 4 + 1] = rgb[i * 3 + 1]; rgba[i * 4 + 2] = rgb[i * 3 + 2]; rgba[i * 4 + 3] = alpha;
        left = Math.min(left, x); right = Math.max(right, x); top = Math.min(top, y); bottom = Math.max(bottom, y);
    }
    const scale = Math.min(1.5, 330 / Math.max(1, right - left), 448 / Math.max(1, bottom - top));
    return { rgba, camera: { x: (left + right) / 2, bottom, scale } };
}

function render(frame, camera) {
    const result = Buffer.alloc(outWidth * outHeight * 4);
    const src = frame.rgba;
    for (let y = 0; y < outHeight; y++) {
        const sy = (y - 466) / camera.scale + camera.bottom;
        if (sy < 0 || sy >= height - 1) continue;
        const y0 = Math.floor(sy), fy = sy - y0;
        for (let x = 0; x < outWidth; x++) {
            const sx = (x - outWidth / 2) / camera.scale + camera.x;
            if (sx < 0 || sx >= width - 1) continue;
            const x0 = Math.floor(sx), fx = sx - x0;
            const indices = [(y0 * width + x0) * 4, (y0 * width + x0 + 1) * 4, ((y0 + 1) * width + x0) * 4, ((y0 + 1) * width + x0 + 1) * 4];
            const weights = [(1 - fx) * (1 - fy), fx * (1 - fy), (1 - fx) * fy, fx * fy];
            let a = 0, r = 0, g = 0, b = 0;
            for (let k = 0; k < 4; k++) {
                const i = indices[k], w = weights[k] * src[i + 3];
                a += w; r += src[i] * w; g += src[i + 1] * w; b += src[i + 2] * w;
            }
            const j = (y * outWidth + x) * 4;
            if (a) { result[j] = r / a; result[j + 1] = g / a; result[j + 2] = b / a; result[j + 3] = a; }
        }
    }
    return result;
}

async function prepare(from, to, relative) {
    const source = path.join(input, relative);
    const raw = execFileSync(ffmpeg, ['-v', 'error', '-i', source, '-an', '-vf', `fps=${fps},scale=${width}:${height}`, '-pix_fmt', 'rgb24', '-f', 'rawvideo', 'pipe:1'], { maxBuffer: 256 * 1024 * 1024 });
    const frames = [];
    for (let offset = 0; offset + pixels * 3 <= raw.length; offset += pixels * 3) frames.push(isolate(raw.subarray(offset, offset + pixels * 3)));
    const dir = path.join(output, from, 'transitions'); fs.mkdirSync(dir, { recursive: true });
    const target = path.join(dir, `to-${to}.webm`);
    const encoder = spawn(ffmpeg, ['-v', 'error', '-y', '-f', 'rawvideo', '-pixel_format', 'rgba', '-video_size', `${outWidth}x${outHeight}`, '-framerate', String(fps), '-i', 'pipe:0', '-an', '-c:v', 'libvpx-vp9', '-pix_fmt', 'yuva420p', '-b:v', '0', '-crf', '33', '-deadline', 'good', '-cpu-used', '4', '-row-mt', '1', target], { stdio: ['pipe', 'ignore', 'pipe'] });
    let errors = ''; encoder.stderr.on('data', chunk => errors += chunk);
    const completion = once(encoder, 'close');
    for (let i = 0; i < frames.length; i++) {
        const camera = { x: 0, bottom: 0, scale: 0 }; let weight = 0;
        // Smooth the framing across both actors instead of cutting between crops.
        for (let j = Math.max(0, i - 8); j <= Math.min(frames.length - 1, i + 8); j++) {
            const w = 9 - Math.abs(i - j); weight += w;
            for (const key of Object.keys(camera)) camera[key] += frames[j].camera[key] * w;
        }
        for (const key of Object.keys(camera)) camera[key] /= weight;
        if (!encoder.stdin.write(render(frames[i], camera))) await once(encoder.stdin, 'drain');
    }
    encoder.stdin.end(); const [code] = await completion;
    if (code) throw new Error(errors || `FFmpeg exited ${code}`);
    console.log(`${from} -> ${to}: ${frames.length} frames, ${fs.statSync(target).size} bytes`);
    return { from, to, original: relative, sha256: crypto.createHash('sha256').update(fs.readFileSync(source)).digest('hex'), asset: `${from}/transitions/to-${to}.webm`, duration: frames.length / fps, width: outWidth, height: outHeight, fps, bytes: fs.statSync(target).size };
}

(async () => {
    const clips = [];
    for (const source of sources) clips.push(await prepare(...source));
    fs.writeFileSync(path.join(output, 'manifest.json'), JSON.stringify({ version: 1, source: 'User-supplied videos, 2026-11-05', preparation: 'Transparent VP9, no audio; pink/black backdrop removal, smoothed framing. Original footage is unchanged outside source.', clips }, null, 2) + '\n');
})().catch(error => { console.error(error); process.exitCode = 1; });
