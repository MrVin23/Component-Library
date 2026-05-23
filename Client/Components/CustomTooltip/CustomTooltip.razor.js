/**
 * Positions the tooltip bubble with position:fixed using the trigger wrapper rect.
 * Keeps the bubble out of document flow so flex/grid layouts are unaffected.
 */
export function positionTooltip(wrapper, bubble, position) {
    if (!wrapper || !bubble) return;

    const gap = 8;
    const pad = 8;

    bubble.style.position = 'fixed';
    bubble.style.zIndex = '2147483647';
    bubble.style.margin = '0';
    bubble.style.right = 'auto';
    bubble.style.bottom = 'auto';
    bubble.style.visibility = 'hidden';

    requestAnimationFrame(() => {
        requestAnimationFrame(() => {
            const wr = wrapper.getBoundingClientRect();
            let br = bubble.getBoundingClientRect();

            if (br.width < 1 || br.height < 1) {
                bubble.style.visibility = 'visible';
                return;
            }

            let left = 0;
            let top = 0;

            switch (position) {
                case 'Right':
                    left = wr.right + gap;
                    top = wr.top + wr.height / 2 - br.height / 2;
                    break;
                case 'Left':
                    left = wr.left - gap - br.width;
                    top = wr.top + wr.height / 2 - br.height / 2;
                    break;
                case 'Bottom':
                    left = wr.left + wr.width / 2 - br.width / 2;
                    top = wr.bottom + gap;
                    break;
                case 'Top':
                    left = wr.left + wr.width / 2 - br.width / 2;
                    top = wr.top - gap - br.height;
                    break;
                default:
                    left = wr.left + wr.width / 2 - br.width / 2;
                    top = wr.top - gap - br.height;
            }

            left = Math.max(pad, Math.min(left, window.innerWidth - br.width - pad));
            top = Math.max(pad, Math.min(top, window.innerHeight - br.height - pad));

            bubble.style.left = `${Math.round(left)}px`;
            bubble.style.top = `${Math.round(top)}px`;
            bubble.style.visibility = 'visible';
        });
    });
}
