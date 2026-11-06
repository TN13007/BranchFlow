const prefix = 'branchflow.order-draft.v1.';
export function read(userId) {
    try {
        const key = prefix + userId;
        const entry = JSON.parse(sessionStorage.getItem(key) || 'null');
        if (!entry || !Number.isFinite(entry.savedAt) || Date.now() - entry.savedAt > 86400000 ||
            !entry.draft || !Array.isArray(entry.draft.items) || entry.draft.items.length > 100) {
            sessionStorage.removeItem(key);
            return null;
        }
        return entry.draft;
    } catch { return null; }
}
export function save(userId, draft) {
    try {
        const key = prefix + userId;
        if (!draft.items.length && !draft.outcomeUnknown) sessionStorage.removeItem(key);
        else sessionStorage.setItem(key, JSON.stringify({ savedAt: Date.now(), draft }));
        return true;
    } catch { return false; }
}
