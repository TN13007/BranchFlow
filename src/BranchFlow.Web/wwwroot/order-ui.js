export function openOptions() {
    const dialog = document.getElementById('product-options');
    if (dialog instanceof HTMLDialogElement && !dialog.open) dialog.showModal();
}
