const taverns = new WeakMap();
const reducedMotion = () => matchMedia('(prefers-reduced-motion: reduce)').matches;
const pose = (y = 0, angle = 0) => `translateY(${y}px) rotate(${angle}deg)`;

function animate(state, element, frames, duration) {
    if (reducedMotion() || !element?.isConnected) return Promise.resolve();
    const animation = element.animate(frames, { duration, easing: 'cubic-bezier(.22,.8,.25,1)' });
    state.animations.add(animation);
    return animation.finished.catch(() => {}).finally(() => state.animations.delete(animation));
}
function setPull(card, y, angle = 0) {
    card.style.transform = pose(y, angle);
    card.closest('.hanging-menu-item')?.style.setProperty('--pull-y', `${y}px`);
}

function settle(state, card, from = 0, angle = 0) {
    if (!card?.isConnected) return;
    card.classList.remove('is-lifted', 'is-dragging');
    setPull(card, 0);
    void animate(state, card, [
        { transform: pose(from, angle) },
        { transform: pose(-3, -angle * .35), offset: .55 },
        { transform: pose(1, angle * .12), offset: .8 },
        { transform: pose() }
    ], 480);
}

function ghostCard(state, card, container) {
    const ghost = card.cloneNode(true);
    ghost.classList.remove('is-lifted', 'is-dragging');
    ghost.classList.add('tavern-flight');
    ghost.removeAttribute('data-product-id');
    ghost.removeAttribute('aria-describedby');
    ghost.setAttribute('aria-hidden', 'true');
    ghost.setAttribute('popover', 'manual');
    ghost.tabIndex = -1;
    ghost.disabled = true;
    ghost.querySelector('.card-pull-handle')?.remove();
    const rect = card.getBoundingClientRect();
    Object.assign(ghost.style, {
        position: 'fixed', left: '0', top: '0', right: 'auto', bottom: 'auto', margin: '0',
        width: `${rect.width}px`, height: `${rect.height}px`,
        transformOrigin: 'top left', pointerEvents: 'none', opacity: '0', zIndex: '10000'
    });
    container.append(ghost);
    ghost.showPopover();
    state.ghosts.add(ghost);
    return { ghost, rect };
}

function removeGhost(state, ghost) {
    ghost.remove();
    state.ghosts.delete(ghost);
}

export function initTavern(root) {
    if (!root || taverns.has(root)) return;
    const state = {
        root, abort: new AbortController(), animations: new Set(), ghosts: new Set(),
        selected: null, invoker: null, dialog: null, drag: null, ignoreClickUntil: 0
    };
    taverns.set(root, state);
    const options = { signal: state.abort.signal };

    root.addEventListener('click', event => {
        const card = event.target.closest('.product-card:not(.tavern-flight)');
        if (!card || card.disabled) return;
        if (performance.now() < state.ignoreClickUntil) {
            event.preventDefault();
            event.stopImmediatePropagation();
            return;
        }
        state.selected = card;
        state.invoker = card;
        card.focus({ preventScroll: true });
    }, { ...options, capture: true });

    // Only the pull ring captures touch; swiping the image still scrolls the page.
    root.addEventListener('pointerdown', event => {
        const card = event.target.closest('.product-card:not(.tavern-flight)');
        if (!card || card.disabled || event.button !== 0 || state.dialog?.open || state.drag) return;
        if (event.pointerType !== 'mouse' && !event.target.closest('.card-pull-handle')) return;
        for (const animation of card.getAnimations()) animation.cancel();
        state.drag = { card, id: event.pointerId, x: event.clientX, y: event.clientY, dy: 0, angle: 0, moved: false };
        card.setPointerCapture(event.pointerId);
    }, options);

    root.addEventListener('pointermove', event => {
        const drag = state.drag;
        if (!drag || drag.id !== event.pointerId) return;
        const dx = event.clientX - drag.x;
        const dy = event.clientY - drag.y;
        if (Math.abs(dx) + Math.abs(dy) < 5) return;
        drag.moved = true;
        drag.dy = Math.max(0, Math.min(100, dy * .72));
        drag.angle = reducedMotion() ? 0 : Math.max(-4, Math.min(4, dx * .065));
        drag.card.classList.add('is-dragging');
        setPull(drag.card, reducedMotion() ? Math.min(12, drag.dy) : drag.dy, drag.angle);
    }, options);

    const finishDrag = (event, canceled = false) => {
        const drag = state.drag;
        if (!drag || drag.id !== event.pointerId) return;
        state.drag = null;
        if (drag.card.hasPointerCapture(event.pointerId)) drag.card.releasePointerCapture(event.pointerId);
        drag.card.classList.remove('is-dragging');
        if (!drag.moved) return;
        state.ignoreClickUntil = performance.now() + 350;
        if (!canceled && drag.dy >= 42) {
            // Same Blazor action for click, keyboard and pull; no duplicate business logic.
            state.ignoreClickUntil = 0;
            drag.card.click();
            state.ignoreClickUntil = performance.now() + 350;
        } else {
            settle(state, drag.card, drag.dy, drag.angle);
        }
    };
    root.addEventListener('pointerup', event => finishDrag(event), options);
    root.addEventListener('pointercancel', event => finishDrag(event, true), options);
    root.addEventListener('lostpointercapture', event => finishDrag(event, true), options);

    root.addEventListener('pointerover', event => {
        const card = event.target.closest('.product-card:not(.tavern-flight)');
        if (!card || card.disabled || card.contains(event.relatedTarget) || state.drag || state.dialog?.open) return;
        if (event.pointerType !== 'mouse' || reducedMotion()) return;
        for (const animation of card.getAnimations()) animation.cancel();
        void animate(state, card, [
            { transform: pose() }, { transform: pose(2, 1.2) },
            { transform: pose(0, -.65) }, { transform: pose(0, .25) }, { transform: pose() }
        ], 650);
    }, options);

    root.addEventListener('pointerover', event => {
        const bar = event.target.closest('.menu-toolbar');
        if (!bar || bar.contains(event.relatedTarget) || event.pointerType !== 'mouse' || state.drag) return;
        for (const animation of bar.getAnimations()) animation.cancel();
        void animate(state, bar, [
            { transform: 'rotate(0deg)' }, { transform: 'rotate(.22deg)' },
            { transform: 'rotate(-.12deg)' }, { transform: 'rotate(0deg)' }
        ], 650);
    }, options);

    state.observer = new MutationObserver(() => {
        if (state.dialog && !state.dialog.isConnected) {
            state.dialog = null;
            settle(state, state.selected, 28);
            if (state.invoker?.isConnected) state.invoker.focus({ preventScroll: true });
            state.selected = null;
        }
    });
    state.observer.observe(root, { childList: true, subtree: true });
}

export async function openOptions(root) {
    const state = taverns.get(root);
    const dialog = root?.querySelector('#product-options');
    if (!state || !(dialog instanceof HTMLDialogElement) || dialog.open) return;
    state.dialog = dialog;
    if (!state.selected?.isConnected) {
        state.selected = null;
        state.invoker = document.activeElement;
    }
    const source = state.selected;
    const sourceRect = source?.getBoundingClientRect();
    dialog.showModal();
    dialog.addEventListener('cancel', event => {
        event.preventDefault();
        dialog.querySelector('.dialog-close')?.click();
    }, { signal: state.abort.signal });
    dialog.addEventListener('click', event => {
        const r = dialog.getBoundingClientRect();
        if (event.target === dialog && (event.clientX < r.left || event.clientX > r.right || event.clientY < r.top || event.clientY > r.bottom)) {
            dialog.querySelector('.dialog-close')?.click();
        }
    }, { signal: state.abort.signal });
    const tasks = [];
    if (source) {
        source.classList.add('is-lifted');
        setPull(source, reducedMotion() ? 0 : 28);
        if (!reducedMotion()) {
            const { ghost } = ghostCard(state, source, root);
            const target = dialog.querySelector('.configurator-image')?.getBoundingClientRect() ?? dialog.getBoundingClientRect();
            tasks.push(animate(state, ghost, [
                { transform: `translate(${sourceRect.left}px,${sourceRect.top}px)`, opacity: .95 },
                { transform: `translate(${target.left}px,${target.top}px) scale(${target.width / sourceRect.width},${target.height / sourceRect.height})`, opacity: 0 }
            ], 320).then(() => removeGhost(state, ghost)));
        }
    }
    tasks.push(animate(state, dialog, [
        { opacity: 0, transform: 'translateY(18px) scale(.97)' },
        { opacity: 1, transform: 'translateY(0) scale(1)' }
    ], 260));
    await Promise.all(tasks);
}

export async function closeOptions(root) {
    const state = taverns.get(root);
    const dialog = state?.dialog;
    if (!dialog?.open) return;
    for (const animation of [...state.animations]) animation.cancel();
    for (const ghost of [...state.ghosts]) removeGhost(state, ghost);
    const card = state.selected?.isConnected ? state.selected : null;
    const target = card?.getBoundingClientRect();
    const origin = dialog.querySelector('.configurator-image')?.getBoundingClientRect() ?? dialog.getBoundingClientRect();
    await animate(state, dialog, [{ opacity: 1, transform: 'scale(1)' }, { opacity: 0, transform: 'translateY(8px) scale(.98)' }], 130);
    dialog.close();
    state.dialog = null;
    if (card) {
        if (!reducedMotion()) {
            const { ghost, rect } = ghostCard(state, card, root);
            await animate(state, ghost, [
                { transform: `translate(${origin.left}px,${origin.top}px) scale(${origin.width / rect.width},${origin.height / rect.height})`, opacity: .2 },
                { transform: `translate(${target.left}px,${target.top}px) scale(1)`, opacity: 1 }
            ], 280);
            removeGhost(state, ghost);
        }
        settle(state, card, 28, .8);
    }
    if (state.invoker?.isConnected) state.invoker.focus({ preventScroll: true });
    state.selected = null;
}

export function disposeTavern(root) {
    const state = taverns.get(root);
    if (!state) return;
    state.abort.abort();
    state.observer.disconnect();
    for (const animation of state.animations) animation.cancel();
    for (const ghost of state.ghosts) ghost.remove();
    state.dialog?.close();
    taverns.delete(root);
}

export function focusTab(tabListId, index) {
    const list = document.getElementById(tabListId);
    if (!list) return;
    const tabs = list.querySelectorAll('[role="tab"]');
    if (index >= 0 && index < tabs.length) {
        tabs[index].focus();
    }
}
