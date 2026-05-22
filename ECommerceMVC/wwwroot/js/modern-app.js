/* ============================================================
   Modern App — ECommerceMVC
   Vanilla JS replacing jQuery: dark mode, mobile nav, search,
   toasts, range slider, qty steppers, scroll effects
   ============================================================ */

(function () {
  'use strict';

  /* ======================== Theme ======================== */

  function applyTheme() {
    document.documentElement.setAttribute('data-theme', 'light');
    document.documentElement.setAttribute('data-bs-theme', 'light');
  }

  applyTheme();

  /* ======================== DOM Ready ======================== */

  document.addEventListener('DOMContentLoaded', function () {


    /* ======================== Navbar Scroll ======================== */

    const navbar = document.querySelector('.modern-navbar');
    if (navbar) {
      const handleScroll = () => {
        navbar.classList.toggle('scrolled', window.scrollY > 10);
      };
      window.addEventListener('scroll', handleScroll, { passive: true });
      handleScroll();
    }

    /* ======================== Mobile Drawer ======================== */

    const hamburger = document.querySelector('.navbar-hamburger');
    const drawerOverlay = document.querySelector('.mobile-drawer-overlay');
    const drawer = document.querySelector('.mobile-drawer');
    const drawerClose = document.querySelector('.drawer-close');

    function openDrawer() {
      if (drawerOverlay) drawerOverlay.classList.add('open');
      if (drawer) drawer.classList.add('open');
      document.body.style.overflow = 'hidden';
    }

    function closeDrawer() {
      if (drawerOverlay) drawerOverlay.classList.remove('open');
      if (drawer) drawer.classList.remove('open');
      document.body.style.overflow = '';
    }

    if (hamburger) hamburger.addEventListener('click', openDrawer);
    if (drawerOverlay) drawerOverlay.addEventListener('click', closeDrawer);
    if (drawerClose) drawerClose.addEventListener('click', closeDrawer);

    // Close on ESC
    document.addEventListener('keydown', e => {
      if (e.key === 'Escape') {
        closeDrawer();
        closeSearch();
      }
    });

    /* ======================== Search Overlay ======================== */

    const searchOverlay = document.querySelector('.search-overlay');
    const searchInput = document.querySelector('.search-overlay .search-input');

    function openSearch() {
      if (searchOverlay) {
        searchOverlay.classList.add('open');
        document.body.style.overflow = 'hidden';
        setTimeout(() => {
          if (searchInput) searchInput.focus();
        }, 300);
      }
    }

    function closeSearch() {
      if (searchOverlay) {
        searchOverlay.classList.remove('open');
        document.body.style.overflow = '';
        renderSearchSuggestions([]);
      }
    }

    const suggestionsBox = document.querySelector('[data-search-suggestions]');
    let searchDebounceTimer = null;
    let activeSearchController = null;

    function renderSearchSuggestions(items, loading) {
      if (!suggestionsBox) return;
      if (loading) {
        suggestionsBox.innerHTML = '<div class="search-suggestion-state">Đang tìm kiếm...</div>';
        suggestionsBox.classList.add('open');
        return;
      }
      if (!items || !items.length) {
        suggestionsBox.innerHTML = '';
        suggestionsBox.classList.remove('open');
        return;
      }
      suggestionsBox.innerHTML = items.map(item => {
        const price = new Intl.NumberFormat('vi-VN').format(item.donGia || 0) + ' VND';
        return '<a class="search-suggestion-item" href="' + item.detailUrl + '">' +
          '<img src="' + item.hinhUrl + '" alt="' + item.tenHH + '" loading="lazy" />' +
          '<span><strong>' + item.tenHH + '</strong><small>' + (item.tenLoai || '') + ' • ' + price + '</small></span>' +
          '</a>';
      }).join('');
      suggestionsBox.classList.add('open');
      if (window.ModernIcons) window.ModernIcons.renderIcons(suggestionsBox);
    }

    function loadSearchSuggestions() {
      if (!searchInput) return;
      const query = searchInput.value.trim();
      if (query.length < 2) {
        renderSearchSuggestions([]);
        return;
      }
      if (activeSearchController) activeSearchController.abort();
      activeSearchController = new AbortController();
      renderSearchSuggestions([], true);
      fetch('/HangHoa/SearchSuggestions?query=' + encodeURIComponent(query), { signal: activeSearchController.signal })
        .then(response => response.ok ? response.json() : [])
        .then(items => renderSearchSuggestions(items))
        .catch(error => {
          if (error.name !== 'AbortError') renderSearchSuggestions([]);
        });
    }

    if (searchInput) {
      searchInput.addEventListener('input', () => {
        clearTimeout(searchDebounceTimer);
        searchDebounceTimer = setTimeout(loadSearchSuggestions, 250);
      });
    }

    document.querySelectorAll('[data-open-search]').forEach(btn => {
      btn.addEventListener('click', e => {
        e.preventDefault();
        openSearch();
      });
    });

    document.querySelectorAll('[data-close-search]').forEach(btn => {
      btn.addEventListener('click', closeSearch);
    });

    if (searchOverlay) {
      searchOverlay.addEventListener('click', e => {
        if (e.target === searchOverlay) closeSearch();
      });
    }

    /* ======================== Toast / Notification ======================== */

    window.customerNotify = function (message, type) {
      type = type || 'info';
      let container = document.querySelector('.toast-container');
      if (!container) {
        container = document.createElement('div');
        container.className = 'toast-container';
        document.body.appendChild(container);
      }

      const iconMap = {
        success: 'check-circle',
        error: 'alert-triangle',
        warning: 'alert-triangle',
        info: 'info'
      };

      const toast = document.createElement('div');
      toast.className = 'toast-modern toast-' + type;
      toast.innerHTML =
        '<i data-icon="' + (iconMap[type] || 'info') + '" class="toast-icon"></i>' +
        '<span class="toast-body">' + message + '</span>' +
        '<button class="toast-close" aria-label="Close"><i data-icon="x" data-size="14px"></i></button>' +
        '<div class="toast-progress"></div>';

      container.appendChild(toast);

      // Render icons in toast
      if (window.ModernIcons) {
        window.ModernIcons.renderIcons(toast);
      }

      // Close button
      toast.querySelector('.toast-close').addEventListener('click', () => dismissToast(toast));

      // Auto-dismiss
      setTimeout(() => dismissToast(toast), 3000);
    };

    function dismissToast(toast) {
      if (!toast || toast.classList.contains('toast-exit')) return;
      toast.classList.add('toast-exit');
      setTimeout(() => toast.remove(), 300);
    }

    // Handle click-notify buttons
    document.addEventListener('click', function (e) {
      const btn = e.target.closest('[data-click-notify]');
      if (btn) {
        const msg = btn.getAttribute('data-click-notify');
        if (msg && window.customerNotify) {
          window.customerNotify(msg, 'info');
        }
      }
    });

    // Server flash messages
    document.querySelectorAll('.server-flash').forEach(el => {
      const msg = el.textContent.trim();
      const type = el.dataset.type || 'success';
      if (msg) window.customerNotify(msg, type);
      el.remove();
    });

    /* ======================== Scroll Reveal ======================== */

    const revealElements = document.querySelectorAll('.reveal, .reveal-left, .reveal-right, .reveal-scale');
    if (revealElements.length) {
      const revealObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            entry.target.classList.add('revealed');
            revealObserver.unobserve(entry.target);
          }
        });
      }, { threshold: 0.1, rootMargin: '0px 0px -50px 0px' });

      revealElements.forEach(el => revealObserver.observe(el));
    }

    // Stagger items
    const staggerItems = document.querySelectorAll('.stagger-item');
    if (staggerItems.length) {
      const staggerObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            entry.target.classList.add('visible');
            staggerObserver.unobserve(entry.target);
          }
        });
      }, { threshold: 0.05 });

      staggerItems.forEach(el => staggerObserver.observe(el));
    }

    /* ======================== Back to Top ======================== */

    const backToTop = document.querySelector('.back-to-top');
    if (backToTop) {
      window.addEventListener('scroll', () => {
        backToTop.classList.toggle('visible', window.scrollY > 300);
      }, { passive: true });

      backToTop.addEventListener('click', () => {
        window.scrollTo({ top: 0, behavior: 'smooth' });
      });
    }

    /* ======================== Quantity Stepper ======================== */

    document.querySelectorAll('.qty-stepper').forEach(stepper => {
      const input = stepper.querySelector('input');
      const minusBtn = stepper.querySelector('[data-qty-minus]');
      const plusBtn = stepper.querySelector('[data-qty-plus]');
      if (!input) return;

      const min = parseInt(input.min) || 1;
      const max = parseInt(input.max) || 999;

      if (minusBtn) {
        minusBtn.addEventListener('click', () => {
          const val = parseInt(input.value) || min;
          if (val > min) input.value = val - 1;
        });
      }

      if (plusBtn) {
        plusBtn.addEventListener('click', () => {
          const val = parseInt(input.value) || min;
          if (val < max) input.value = val + 1;
        });
      }
    });

    /* ======================== Dropdown (BS5 compatible) ======================== */

    // Bootstrap 5 handles dropdowns via data-bs-toggle="dropdown"
    // No custom code needed

    /* ======================== Price Range Slider ======================== */

    const rangeMin = document.getElementById('priceRangeMin');
    const rangeMax = document.getElementById('priceRangeMax');
    const rangeMinLabel = document.getElementById('priceMinLabel');
    const rangeMaxLabel = document.getElementById('priceMaxLabel');
    const minPriceHidden = document.getElementById('minPriceHidden');
    const maxPriceHidden = document.getElementById('maxPriceHidden');

    function formatVND(value) {
      return new Intl.NumberFormat('vi-VN').format(value) + ' VND';
    }

    function updateRangeSlider() {
      if (!rangeMin || !rangeMax) return;
      let minVal = parseInt(rangeMin.value);
      let maxVal = parseInt(rangeMax.value);

      if (minVal > maxVal) {
        [rangeMin.value, rangeMax.value] = [maxVal, minVal];
        [minVal, maxVal] = [maxVal, minVal];
      }

      if (rangeMinLabel) rangeMinLabel.textContent = formatVND(minVal);
      if (rangeMaxLabel) rangeMaxLabel.textContent = formatVND(maxVal);
      if (minPriceHidden) minPriceHidden.value = minVal;
      if (maxPriceHidden) maxPriceHidden.value = maxVal;
    }

    if (rangeMin) rangeMin.addEventListener('input', updateRangeSlider);
    if (rangeMax) rangeMax.addEventListener('input', updateRangeSlider);

    /* ======================== Form Validation Visual ======================== */

    document.querySelectorAll('.form-modern').forEach(input => {
      input.addEventListener('invalid', () => {
        input.classList.add('is-invalid');
      });
      input.addEventListener('input', () => {
        if (input.classList.contains('is-invalid') && input.validity.valid) {
          input.classList.remove('is-invalid');
        }
      });
    });

    /* ======================== Smooth page enter ======================== */
    const main = document.querySelector('.main-content');
    if (main) {
      main.classList.add('page-enter');
    }

  }); // end DOMContentLoaded

  /* ======================== Expose ======================== */
  window.ModernApp = {
    applyTheme,
  };

})();

