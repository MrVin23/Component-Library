let clickAwayHandlers = new Map();

function isDomElement(el) {
    return el && typeof el.nodeType === "number" && el.nodeType === Node.ELEMENT_NODE;
}

/**
 * Close on document click when the target is outside both the trigger (input row)
 * and the floating panel. Bubble phase runs after in-panel click handlers so selection
 * is applied before we evaluate outside clicks (capture was racing with panel unmount).
 */
export function initializeClickAway(triggerRef, dotNetObjectReference) {
    const trigger = triggerRef;

    const state = { trigger, panel: null };

    const clickAwayHandler = (event) => {
        const t = event.target;
        const inTrigger = isDomElement(state.trigger) && state.trigger.contains(t);
        const inPanel = isDomElement(state.panel) && state.panel.contains(t);
        if (inTrigger || inPanel) {
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

/** Call after each render when IsOpen or panel DOM may have changed. */
export function syncClickAwayPanel(dotNetObjectReference, isOpen, panelRef) {
    const entry = clickAwayHandlers.get(dotNetObjectReference);
    if (!entry) {
        return;
    }
    entry.state.panel = isOpen && isDomElement(panelRef) ? panelRef : null;
}

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
