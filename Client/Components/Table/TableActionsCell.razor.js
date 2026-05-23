/**
 * Positions the dropdown above or below the trigger based on available viewport space.
 * Ensures the dropdown is never clipped when the table is scrolled.
 * Uses requestAnimationFrame so layout is complete before measuring.
 */
export function positionDropdown(triggerElement, dropdownElement) {
    if (!triggerElement || !dropdownElement) return;

    dropdownElement.style.visibility = 'hidden';

    requestAnimationFrame(() => {
        const triggerRect = triggerElement.getBoundingClientRect();
        const dropdownRect = dropdownElement.getBoundingClientRect();
        const viewportHeight = window.innerHeight;
        const viewportWidth = window.innerWidth;

        const spaceBelow = viewportHeight - triggerRect.bottom;
        const spaceAbove = triggerRect.top;
        const dropdownHeight = dropdownRect.height;
        const dropdownWidth = dropdownRect.width;

        // Vertical: show above if more space above, otherwise below
        const showAbove = spaceAbove >= spaceBelow || spaceBelow < dropdownHeight;

        if (showAbove) {
            dropdownElement.style.top = 'auto';
            dropdownElement.style.bottom = `${viewportHeight - triggerRect.top + 4}px`;
        } else {
            dropdownElement.style.bottom = 'auto';
            dropdownElement.style.top = `${triggerRect.bottom + 4}px`;
        }

        // Horizontal: align right edge with trigger, clamp to viewport
        const preferredLeft = triggerRect.right - dropdownWidth;
        const left = Math.max(8, Math.min(preferredLeft, viewportWidth - dropdownWidth - 8));
        dropdownElement.style.left = `${left}px`;
        dropdownElement.style.right = 'auto';
        dropdownElement.style.visibility = 'visible';
    });
}
