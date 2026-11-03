export function submitLogin() {
    const form = document.getElementById('branchflow-login');
    if (form instanceof HTMLFormElement) HTMLFormElement.prototype.submit.call(form);
}
