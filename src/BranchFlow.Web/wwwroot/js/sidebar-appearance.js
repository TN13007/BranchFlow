const storageKey = 'branchflow.sidebar.companion.v1';
const allowed = new Set(['ginger', 'santa', 'reindeer', 'barista']);
const companions = new WeakMap();
const reduced = () => matchMedia('(prefers-reduced-motion: reduce)').matches;

function play(state, element, frames, duration, fill = 'both') {
    if (state.disposed || reduced() || !element?.isConnected) return Promise.resolve();
    const animation = element.animate(frames, { duration, easing: 'cubic-bezier(.35,.05,.22,1)', fill });
    state.animations.add(animation);
    return animation.finished.catch(() => {}).finally(() => {
        if (fill === 'none') { state.animations.delete(animation); animation.cancel(); }
    });
}

function cancelAnimations(state) {
    for (const animation of state.animations) animation.cancel();
    state.animations.clear();
}

function clearHandover(state) {
    state.controller?.abort();
    state.controller = null;
    releaseVideo(state);
    cancelAnimations(state);
    state.stage?.removeAttribute('data-acting');
    state.aside?.removeAttribute('data-handoff');
    state.layer?.replaceChildren();
    state.outgoing = null;
    state.incoming = null;
    state.mode = null;
    state.skipRequested = false;
}

function releaseVideo(state) {
    if (!state.video) return;
    state.video.pause();
    state.video.removeAttribute('src');
    state.video.load();
    state.video.remove();
    state.video = null;
}

export function initCompanion(panel, paths) {
    if (!panel || companions.has(panel)) return;
    const state = {
        panel, stage: panel.querySelector('.companion-display'),
        layer: panel.querySelector('.companion-handover'), aside: panel.closest('.employee-sidebar'),
        animations: new Set(), disposed: false, outgoing: null, incoming: null,
        video: null, controller: null, mode: null, skipRequested: false,
        media: matchMedia('(prefers-reduced-motion: reduce)')
    };
    state.motionChanged = () => { if (state.media.matches) skipHandover(panel); };
    state.visibilityChanged = () => { if (document.hidden) skipHandover(panel); };
    state.media.addEventListener('change', state.motionChanged);
    document.addEventListener('visibilitychange', state.visibilityChanged);
    companions.set(panel, state);
    for (const path of paths) {
        const image = new Image();
        image.src = path;
        void image.decode().catch(() => {});
    }
}

export async function openPicker(panel) {
    const state = companions.get(panel);
    const picker = panel?.querySelector('.companion-picker');
    if (!state || !picker) return;
    for (const animation of picker.getAnimations()) { animation.cancel(); state.animations.delete(animation); }
    const height = picker.scrollHeight;
    await play(state, picker, [{ height: '0px', opacity: 0 }, { height: `${height}px`, opacity: 1 }], 200, 'none');
    for (const animation of picker.getAnimations()) { animation.cancel(); state.animations.delete(animation); }
    if (!state.disposed && mayRestoreFocus(panel)) picker.querySelector('[aria-pressed="true"]')?.focus({ preventScroll: true });
}

export async function closePicker(panel) {
    const state = companions.get(panel);
    const picker = panel?.querySelector('.companion-picker');
    if (!state || !picker || picker.hidden) return;
    await play(state, picker, [{ height: `${picker.scrollHeight}px`, opacity: 1 }, { height: '0px', opacity: 0 }], 160);
    // Keep its collapsed size until Blazor applies hidden in the next render.
}

export function focusToggle(panel) {
    if (panel && mayRestoreFocus(panel)) panel.querySelector('.companion-toggle')?.focus({ preventScroll: true });
}

function mayRestoreFocus(panel) {
    const active = document.activeElement;
    return active === document.body || active === document.documentElement || panel.contains(active);
}

async function actor(path) {
    const image = new Image();
    image.alt = '';
    image.className = 'handover-actor';
    image.draggable = false;
    image.src = path;
    try { await image.decode(); return image; } catch { return null; }
}

// Bounded, cancellable media waits: a failed load or suspended tab cannot trap the picker.
function untilVideo(state, predicate, timeout) {
    const video = state.video, signal = state.controller?.signal;
    if (!video || signal?.aborted || state.disposed) return Promise.resolve(false);
    if (video.error) return Promise.resolve(false);
    if (predicate(video)) return Promise.resolve(true);
    return new Promise(resolve => {
        const events = ['loadeddata', 'playing', 'timeupdate', 'ended'];
        const finish = value => {
            clearTimeout(timer);
            for (const event of events) video.removeEventListener(event, check);
            video.removeEventListener('error', fail);
            signal?.removeEventListener('abort', fail);
            resolve(value);
        };
        const fail = () => finish(false);
        const check = () => { if (predicate(video)) finish(true); };
        const timer = setTimeout(fail, timeout);
        for (const event of events) video.addEventListener(event, check);
        video.addEventListener('error', fail, { once: true });
        signal?.addEventListener('abort', fail, { once: true });
    });
}

async function beginVideo(state, path) {
    const video = document.createElement('video');
    if (!video.canPlayType('video/webm; codecs="vp9"')) return false;
    video.className = 'handover-video';
    video.muted = true;
    video.defaultMuted = true;
    video.playsInline = true;
    video.preload = 'auto';
    video.disablePictureInPicture = true;
    video.disableRemotePlayback = true;
    video.tabIndex = -1;
    video.setAttribute('aria-hidden', 'true');
    video.style.opacity = '0';
    state.video = video;
    state.layer.append(video);
    video.src = path;
    video.load();
    if (!await untilVideo(state, v => v.readyState >= 2, 3500)) {
        releaseVideo(state);
        return false;
    }
    void video.play().catch(() => video.dispatchEvent(new Event('error')));
    if (!await untilVideo(state, v => !v.paused && v.readyState >= 2, 2000)) {
        releaseVideo(state);
        return false;
    }
    state.mode = 'video';
    state.stage.dataset.acting = 'true';
    state.aside.dataset.handoff = 'video';
    await Promise.all([
        play(state, video, [{ opacity: 0 }, { opacity: 1 }], 180),
        play(state, state.stage.querySelector('.companion-portrait'), [{ opacity: 1 }, { opacity: 0 }], 180)
    ]);
    // Commit the new palette while the incoming character is entering, not at load time.
    await untilVideo(state, v => v.ended || v.currentTime >= v.duration * .44, 6500);
    return true;
}

export async function beginHandover(panel, { fromKey, fromPath, toKey, toPath, videoPath }) {
    const state = companions.get(panel);
    if (!state || state.disposed) return;
    const skipped = state.skipRequested;
    clearHandover(state);
    if (skipped || reduced() || document.hidden || !state.stage?.getBoundingClientRect().height) return;
    state.controller = new AbortController();
    if (videoPath && await beginVideo(state, videoPath)) return;
    if (state.disposed || state.controller?.signal.aborted || reduced()) return;
    const [outgoing, incoming] = await Promise.all([actor(fromPath), actor(toPath)]);
    if (state.disposed || state.controller?.signal.aborted || !outgoing || !incoming || reduced()) return;
    state.mode = 'cutout';
    state.outgoing = outgoing;
    state.incoming = incoming;
    // Align the reaching hands of the four differently proportioned cutout poses.
    const handHeights = { ginger: .4, santa: .34, reindeer: .38, barista: .31 };
    for (const [image, key] of [[outgoing, fromKey], [incoming, toKey]]) {
        image.style.top = `${(.4 - (handHeights[key] ?? .4) + (image === outgoing ? .04 : 0)) * state.stage.clientHeight}px`;
    }
    state.layer.replaceChildren(outgoing, incoming);
    state.stage.dataset.acting = 'true';
    state.aside.dataset.handoff = 'reach';
    await Promise.all([
        play(state, state.stage.querySelector('.companion-portrait'), [{ opacity: 1 }, { opacity: 0 }], 120),
        play(state, outgoing, [
            { transform: 'translateX(0) rotate(0deg)', opacity: 0 },
            { transform: 'translateX(-8%) rotate(-3deg) scale(.95)', opacity: 1, offset: .32 },
            { transform: 'translateX(-20%) rotate(-8deg) scale(.9)', opacity: 1 }
        ], 370),
        play(state, incoming, [
            { transform: 'translateX(100%) scaleX(-1) scale(.86)', opacity: 0 },
            { transform: 'translateX(74%) scaleX(-1) scale(.86)', opacity: 1, offset: .52 },
            { transform: 'translateX(44%) scaleX(-1) scale(.9)', opacity: 1 }
        ], 370)
    ]);
}

export async function finishHandover(panel) {
    const state = companions.get(panel);
    if (!state || state.disposed) return;
    if (reduced() || state.controller?.signal.aborted) { clearHandover(state); return; }
    if (state.mode === 'video') {
        const video = state.video;
        const remaining = Number.isFinite(video.duration) ? (video.duration - video.currentTime) * 1000 : 10000;
        await untilVideo(state, v => v.ended, Math.min(14000, remaining + 2200));
        if (state.disposed || state.controller?.signal.aborted || reduced()) { clearHandover(state); return; }
        video.pause();
        const portrait = state.stage.querySelector('.companion-portrait');
        await portrait.decode().catch(() => {});
        if (state.disposed || state.controller?.signal.aborted) { clearHandover(state); return; }
        state.aside.dataset.handoff = 'settle';
        state.stage.removeAttribute('data-acting');
        await Promise.all([
            play(state, video, [{ opacity: 1 }, { opacity: 0 }], 260),
            play(state, portrait, [{ opacity: 0 }, { opacity: 1 }], 260)
        ]);
        clearHandover(state);
        return;
    }
    if (!state.outgoing || !state.incoming) { clearHandover(state); return; }
    state.aside.dataset.handoff = 'pull';
    await Promise.all([
        play(state, state.outgoing, [
            { transform: 'translateX(-20%) rotate(-8deg) scale(.9)', opacity: 1 },
            { transform: 'translateX(-64%) rotate(-8deg) scale(.9)', opacity: 1, offset: .78 },
            { transform: 'translateX(-106%) rotate(-13deg) scale(.84)', opacity: 0 }
        ], 460),
        play(state, state.incoming, [
            { transform: 'translateX(44%) scaleX(-1) scale(.9)', opacity: 1 },
            { transform: 'translateX(0) scaleX(-1) scale(.9)', opacity: 1, offset: .78 },
            { transform: 'translateX(-2%) scaleX(-1) scale(.94)', opacity: 1, offset: .9 },
            { transform: 'translateX(0) scaleX(-1) scale(.94)', opacity: 1 }
        ], 460)
    ]);
    if (state.disposed) return;
    state.aside.dataset.handoff = 'settle';
    state.stage.removeAttribute('data-acting');
    const portrait = state.stage.querySelector('.companion-portrait');
    await Promise.all([
        play(state, state.incoming, [{ opacity: 1 }, { opacity: 0 }], 190),
        play(state, portrait, [{ opacity: 0, transform: 'translateY(3px) scale(.97)' }, { opacity: 1, transform: 'none' }], 190)
    ]);
    clearHandover(state);
}

export function skipHandover(panel) {
    const state = companions.get(panel);
    if (!state || state.disposed) return;
    state.skipRequested = panel.getAttribute('aria-busy') === 'true';
    state.controller?.abort();
    state.video?.pause();
    cancelAnimations(state);
}

export function disposeCompanion(panel) {
    const state = companions.get(panel);
    if (!state) return;
    state.disposed = true;
    state.media.removeEventListener('change', state.motionChanged);
    document.removeEventListener('visibilitychange', state.visibilityChanged);
    clearHandover(state);
    companions.delete(panel);
}

// Appearance only. Storage denial must never prevent navigation or ordering.
export function loadStyle() {
    try {
        const key = localStorage.getItem(storageKey);
        return allowed.has(key) ? key : 'ginger';
    } catch { return 'ginger'; }
}

export function saveStyle(key) {
    if (!allowed.has(key)) return;
    try { localStorage.setItem(storageKey, key); } catch { /* Session-only choice when storage is unavailable. */ }
}
