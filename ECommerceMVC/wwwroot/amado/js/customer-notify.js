(function () {
    'use strict';

    function ensureContainer() {
        var container = document.getElementById('clickNotifyContainer');
        if (container) {
            return container;
        }

        container = document.createElement('div');
        container.id = 'clickNotifyContainer';
        container.style.position = 'fixed';
        container.style.bottom = '16px';
        container.style.right = '16px';
        container.style.zIndex = '2100';
        container.style.display = 'flex';
        container.style.flexDirection = 'column';
        container.style.gap = '8px';
        container.style.maxWidth = '320px';
        document.body.appendChild(container);
        return container;
    }

    function showToast(message, type) {
        if (!message) {
            return;
        }

        var container = ensureContainer();
        var toast = document.createElement('div');
        toast.className = 'alert ' + (type === 'warning' ? 'alert-warning' : 'alert-info') + ' mb-0';
        toast.textContent = message;
        toast.style.boxShadow = '0 6px 16px rgba(0,0,0,0.15)';
        toast.style.opacity = '0';
        toast.style.transition = 'opacity .2s ease';
        container.appendChild(toast);

        requestAnimationFrame(function () {
            toast.style.opacity = '1';
        });

        setTimeout(function () {
            toast.style.opacity = '0';
            setTimeout(function () {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 200);
        }, 1800);
    }

    document.addEventListener('click', function (event) {
        var target = event.target.closest('[data-click-notify]');
        if (!target) {
            return;
        }

        var message = target.getAttribute('data-click-notify');
        showToast(message, 'info');
    });

    window.customerNotify = showToast;

    window.setTimeout(function () {
        document.querySelectorAll('#serverFlashContainer .customer-toast').forEach(function (toast) {
            toast.style.transition = 'opacity .25s ease';
            toast.style.opacity = '0';
            window.setTimeout(function () {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 250);
        });
    }, 2600);
})();