const dialogs = new WeakMap();

// Dialog gốc của trình duyệt giữ focus, phím Escape và trả focus về nút mở.
export function openDialog(root, id) {
    const dialog = root?.querySelector(`#${id}`);
    if (!(dialog instanceof HTMLDialogElement) || dialog.open) return;
    const invoker = document.activeElement;
    const abort = new AbortController();
    dialogs.set(dialog, { invoker, abort });
    dialog.addEventListener('cancel', event => {
        event.preventDefault();
        dialog.querySelector('.dialog-close:not(:disabled)')?.click();
    }, { signal: abort.signal });
    dialog.addEventListener('click', event => {
        const rect = dialog.getBoundingClientRect();
        if (event.target === dialog &&
            (event.clientX < rect.left || event.clientX > rect.right || event.clientY < rect.top || event.clientY > rect.bottom)) {
            dialog.querySelector('.dialog-close:not(:disabled)')?.click();
        }
    }, { signal: abort.signal });
    dialog.showModal();
}

export function closeDialog(root, id) {
    const dialog = root?.querySelector(`#${id}`);
    if (!dialog) return;
    const state = dialogs.get(dialog);
    state?.abort.abort();
    dialog.close();
    if (state?.invoker?.isConnected) state.invoker.focus({ preventScroll: true });
    dialogs.delete(dialog);
}

export function disposeDialogs(root) {
    for (const dialog of root?.querySelectorAll('dialog') ?? []) closeDialog(root, dialog.id);
}
