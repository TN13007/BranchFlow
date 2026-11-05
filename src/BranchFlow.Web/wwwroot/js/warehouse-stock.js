const warehouses = new WeakMap();
const clamp = (state, index) => Math.max(0, Math.min(Math.max(0, state.count - 1), index));
const mouseDragGain = .55;

function stopMotion(state) {
    cancelAnimationFrame(state.frame);
    state.animating = false;
    state.viewport.classList.remove('is-animating');
}

function announce(state, index) {
    if (state.disposed || state.index === index) return;
    state.index = index;
    void state.reference.invokeMethodAsync('OnWarehouseChanged', index).catch(() => {});
}

function go(state, index, smooth = true) {
    if (state.disposed || !state.viewport.clientWidth) return;
    stopMotion(state);
    state.target = clamp(state, index);
    announce(state, state.target);
    const from = state.viewport.scrollLeft, to = state.target * state.viewport.clientWidth;
    if (!smooth || state.motion.matches || Math.abs(to - from) < 1) {
        state.viewport.scrollTo({ left: to, behavior: 'instant' });
        return;
    }
    state.animating = true;
    state.viewport.classList.add('is-animating');
    const start = performance.now();
    const duration = 450 + 450 * Math.sqrt(Math.abs(to - from) / state.viewport.clientWidth);
    const tick = now => {
        if (state.disposed) return;
        const progress = Math.min(1, (now - start) / duration);
        // A gentle head turn: build speed gradually, then ease into the next room.
        state.viewport.scrollLeft = from + (to - from) * (.5 - .5 * Math.cos(Math.PI * progress));
        if (progress < 1) state.frame = requestAnimationFrame(tick);
        else stopMotion(state);
    };
    state.frame = requestAnimationFrame(tick);
}

export function init(root, reference) {
    if (!root || warehouses.has(root)) return;
    const viewport = root.querySelector('.warehouse-viewport');
    const state = { viewport, reference, count: viewport.querySelectorAll('.warehouse-slide').length, index: 0, target: 0, disposed: false, timer: 0, drag: null,
        animating: false, frame: 0, abort: new AbortController(), motion: matchMedia('(prefers-reduced-motion: reduce)') };
    warehouses.set(root, state);
    const options = { signal: state.abort.signal };
    const settle = () => {
        if (state.drag || state.animating || state.disposed) return;
        state.target = clamp(state, Math.round(viewport.scrollLeft / Math.max(1, viewport.clientWidth)));
        announce(state, state.target);
    };
    viewport.addEventListener('scroll', () => {
        clearTimeout(state.timer);
        state.timer = setTimeout(settle, 160);
    }, { ...options, passive: true });
    viewport.addEventListener('scrollend', settle, options);
    viewport.addEventListener('keydown', event => {
        if (event.target !== viewport || event.altKey || event.ctrlKey || event.metaKey) return;
        const next = { ArrowLeft: state.target - 1, ArrowRight: state.target + 1, Home: 0, End: state.count - 1 }[event.key];
        if (next === undefined) return;
        event.preventDefault();
        go(state, next);
    }, options);

    // Touch uses native scrolling/inertia and scroll snap; mouse/pen can drag too.
    viewport.addEventListener('pointerdown', event => {
        stopMotion(state);
        if (event.pointerType === 'touch' || event.button !== 0 || event.target.closest('button, a, input')) return;
        state.drag = { id: event.pointerId, x: event.clientX, y: event.clientY,
            left: viewport.scrollLeft, time: performance.now(), moving: false };
        viewport.setPointerCapture(event.pointerId);
    }, options);
    viewport.addEventListener('pointermove', event => {
        const drag = state.drag;
        if (!drag || drag.id !== event.pointerId) return;
        const dx = event.clientX - drag.x;
        if (!drag.moving && Math.abs(dx) > 8 && Math.abs(dx) > Math.abs(event.clientY - drag.y) * 1.2) {
            drag.moving = true;
            viewport.classList.add('is-dragging');
        }
        if (drag.moving) { event.preventDefault(); viewport.scrollLeft = drag.left - dx * mouseDragGain; }
    }, options);
    const release = event => {
        const drag = state.drag;
        if (!drag || drag.id !== event.pointerId) return;
        state.drag = null;
        if (viewport.hasPointerCapture(event.pointerId)) viewport.releasePointerCapture(event.pointerId);
        if (!drag.moving) return;
        const dx = event.clientX - drag.x, elapsed = Math.max(1, performance.now() - drag.time);
        const committed = event.type !== 'pointercancel' &&
            (Math.abs(dx) > viewport.clientWidth * .16 || (Math.abs(dx) > 60 && Math.abs(dx) / elapsed > .65));
        go(state, committed ? Math.round(drag.left / viewport.clientWidth) + (dx < 0 ? 1 : -1)
            : Math.round(viewport.scrollLeft / viewport.clientWidth));
        viewport.classList.remove('is-dragging');
    };
    viewport.addEventListener('pointerup', release, options);
    viewport.addEventListener('pointercancel', release, options);
    state.resize = new ResizeObserver(() => go(state, state.target, false));
    state.resize.observe(viewport);
    const heading = root.querySelector('.warehouse-heading');
    const help = root.querySelector('.warehouse-help');
    const measureOverlay = () => {
        const bottom = 16 + heading.offsetHeight;
        root.style.setProperty('--warehouse-header-bottom', `${bottom}px`);
        root.style.setProperty('--warehouse-scene-inset', `${bottom + 16}px`);
    };
    state.overlayResize = new ResizeObserver(measureOverlay);
    state.overlayResize.observe(heading);
    state.overlayResize.observe(help);
    measureOverlay();
    state.motion.addEventListener('change', () => { if (state.motion.matches) go(state, state.target, false); }, options);
}

export function step(root, direction) {
    const state = warehouses.get(root);
    if (state) go(state, state.target + Math.sign(direction));
}

export function refresh(root) {
    const state = warehouses.get(root);
    if (!state) return;
    state.count = state.viewport.querySelectorAll('.warehouse-slide').length;
    go(state, state.target, false);
}

export function dispose(root) {
    const state = warehouses.get(root);
    if (!state) return;
    state.disposed = true;
    stopMotion(state);
    clearTimeout(state.timer);
    state.abort.abort();
    state.resize.disconnect();
    state.overlayResize.disconnect();
    state.viewport.classList.remove('is-dragging');
    warehouses.delete(root);
}
