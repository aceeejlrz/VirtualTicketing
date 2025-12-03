// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
    const debounce = (fn, delay) => {
        let t; return function () {
            clearTimeout(t);
            const args = arguments;
            t = setTimeout(() => fn.apply(this, args), delay);
        };
    };

    function initLiveSearch() {
        const input = document.getElementById('liveSearchInput');
        if (!input) return;
        const spinner = document.getElementById('liveSearchSpinner');
        const results = document.getElementById('liveSearchResults');

        const doSearch = debounce(function () {
            const term = input.value || '';
            if (!term.trim()) {
                results.innerHTML = '';
                spinner.style.display = 'none';
                return;
            }

            spinner.style.display = 'block';
            fetch('/Events/Search?term=' + encodeURIComponent(term))
                .then(r => r.text())
                .then(html => {
                    results.innerHTML = html;
                })
                .catch(() => {
                    results.innerHTML = '<div class="text-danger">Error loading results.</div>';
                })
                .finally(() => {
                    spinner.style.display = 'none';
                    attachCartHandlers();
                });
        }, 300);

        input.addEventListener('keyup', doSearch);
    }

    function updateCartBadge() {
        const badge = document.getElementById('cartBadge');
        if (!badge) return;
        fetch('/Purchases/CartSummary')
            .then(r => r.json())
            .then(data => {
                badge.textContent = data.itemCount || 0;
            })
            .catch(() => { /* ignore */ });
    }

    function attachCartHandlers() {
        document.querySelectorAll('.js-add-to-cart').forEach(btn => {
            if (btn.dataset.boundCart === '1') return;
            btn.dataset.boundCart = '1';
            btn.addEventListener('click', function () {
                const eventId = this.getAttribute('data-event-id');
                if (!eventId) return;
                this.disabled = true;
                fetch('/Purchases/AddToCart', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-Requested-With': 'XMLHttpRequest' },
                    body: 'eventId=' + encodeURIComponent(eventId) + '&quantity=1'
                })
                    .then(r => r.json())
                    .then(data => {
                        updateCartBadge();
                        if (data.lowStock) {
                            alert('Only ' + data.remaining + ' tickets left for this event!');
                        }
                    })
                    .catch(() => {
                        alert('Could not add to cart.');
                    })
                    .finally(() => {
                        this.disabled = false;
                    });
            });
        });

        // Quantity inputs on purchase page
        document.querySelectorAll('.js-cart-qty').forEach(input => {
            if (input.dataset.boundQty === '1') return;
            input.dataset.boundQty = '1';
            input.addEventListener('change', function () {
                const eventId = this.getAttribute('data-event-id');
                const qty = parseInt(this.value || '0', 10);
                if (!eventId) return;
                fetch('/Purchases/UpdateCartItem', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/x-www-form-urlencoded', 'X-Requested-With': 'XMLHttpRequest' },
                    body: 'eventId=' + encodeURIComponent(eventId) + '&quantity=' + encodeURIComponent(qty)
                })
                    .then(r => r.json())
                    .then(data => {
                        updateCartBadge();
                        const totalEl = document.getElementById('cartTotal');
                        if (totalEl && typeof data.total === 'number') {
                            totalEl.textContent = data.total.toFixed(2);
                        }
                    })
                    .catch(() => {/* ignore */});
            });
        });
    }

    function initPurchaseModalConfetti() {
        const modalEl = document.getElementById('purchaseSuccessModal');
        if (!modalEl) return;

        const showEvent = modalEl.getAttribute('data-show');
        if (showEvent !== 'true') return;

        const bootstrapModal = new bootstrap.Modal(modalEl);
        bootstrapModal.show();

        if (window.particlesJS) {
            particlesJS('confetti-canvas', {
                particles: {
                    number: { value: 80 },
                    size: { value: 4 },
                    color: { value: ['#16a34a', '#f59e0b', '#3b82f6', '#ec4899'] },
                    move: { speed: 2 }
                },
                interactivity: { events: { onhover: { enable: false } } }
            });
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        initLiveSearch();
        attachCartHandlers();
        updateCartBadge();
        initPurchaseModalConfetti();
    });
})();
