let clickAwayHandlers = new Map();

function isDomElement(el) {
    return el && typeof el.nodeType === "number" && el.nodeType === Node.ELEMENT_NODE;
}

/**
 * Multi-select click-away uses the outer wrapper (input + floating panel).
 * The panel is NOT inside the input row ref used by single-select — only the wrapper contains both.
 */
export function initializeClickAway(rootRef, dotNetObjectReference) {
    const state = { root: rootRef };

    const clickAwayHandler = (event) => {
        const t = event.target;
        if (isDomElement(state.root) && state.root.contains(t)) {
            return;
        }

        try {
            dotNetObjectReference.invokeMethodAsync("HandleClickAway").catch((err) => {
                if (err.message && !err.message.includes("disposed")) {
                    console.warn("Error invoking HandleClickAway:", err);
                }
            });
        } catch (err) {
            if (err.message && !err.message.includes("disposed")) {
                console.warn("Error invoking HandleClickAway:", err);
            }
        }
    };

    clickAwayHandlers.set(dotNetObjectReference, {
        handler: clickAwayHandler,
        dotNetRef: dotNetObjectReference,
        state,
    });

    setTimeout(() => {
        document.addEventListener("click", clickAwayHandler, false);
    }, 0);
}

/** Kept for API compatibility; multi-select only needs the stable wrapper ref. */
export function syncClickAwayPanel() {}

export function dispose(dotNetObjectReference) {
    const handlerData = clickAwayHandlers.get(dotNetObjectReference);
    if (handlerData) {
        document.removeEventListener("click", handlerData.handler, false);

        try {
            handlerData.dotNetRef.dispose();
        } catch (err) {
            // Already disposed, ignore
        }

        clickAwayHandlers.delete(dotNetObjectReference);
    }
}
