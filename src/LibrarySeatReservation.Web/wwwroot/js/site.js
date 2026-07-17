/**
 * 图书馆座位预约系统 — 全局交互 JS
 * 包含：Toast 通知 / 表单 Loading / 导航辅助
 */
(function () {
  'use strict';

  // ==================== Toast 通知系统 ====================
  const Toast = {
    container: null,

    init: function () {
      this.container = document.getElementById('toastContainer');
      if (!this.container) return;
    },

    show: function (message, type) {
      if (!this.container) this.init();
      if (!this.container) return;

      type = type || 'success';
      var bgColor = type === 'success' ? 'var(--color-success)' : 'var(--color-danger)';

      var toast = document.createElement('div');
      toast.className = 'toast-custom toast-' + type;
      toast.setAttribute('role', 'alert');
      toast.innerHTML =
        '<div class="toast-custom-body">' +
          '<span>' + this._escapeHtml(message) + '</span>' +
          '<button type="button" class="toast-custom-close" onclick="this.parentElement.parentElement.remove()">&times;</button>' +
        '</div>';

      this.container.appendChild(toast);

      // 触发动画
      requestAnimationFrame(function () {
        toast.classList.add('toast-visible');
      });

      // 自动消失
      setTimeout(function () {
        toast.classList.remove('toast-visible');
        toast.classList.add('toast-hiding');
        setTimeout(function () { if (toast.parentNode) toast.remove(); }, 300);
      }, 4000);
    },

    success: function (msg) { this.show(msg, 'success'); },
    error: function (msg) { this.show(msg, 'error'); },

    _escapeHtml: function (text) {
      var div = document.createElement('div');
      div.appendChild(document.createTextNode(text));
      return div.innerHTML;
    }
  };

  // ==================== 表单提交 Loading ====================
  function initFormLoading() {
    document.addEventListener('submit', function (e) {
      var form = e.target;
      // 只处理有 .btn-loading 按钮的表单
      var btn = form.querySelector('.btn-loading');
      if (!btn) return;

      // 如果是非 POST 表单，跳过
      if (form.method && form.method.toLowerCase() !== 'post') return;

      // 禁用按钮 + 显示 spinner
      btn.disabled = true;
      var originalHtml = btn.innerHTML;
      btn.innerHTML = '<span class="spinner-border spinner-border-sm me-1" role="status" aria-hidden="true"></span> ' +
        (btn.getAttribute('data-loading-text') || '处理中…');
      btn.setAttribute('data-original-html', originalHtml);

      // 表单提交失败时恢复按钮
      // (成功时页面跳转或刷新，不需要恢复)
    });
  }

  // ==================== 自动关闭 Alert ====================
  function initAutoDismissAlerts() {
    var alerts = document.querySelectorAll('.alert-custom-success, .alert-custom-error');
    alerts.forEach(function (alert) {
      setTimeout(function () {
        alert.classList.remove('show');
        setTimeout(function () { if (alert.parentNode) alert.remove(); }, 300);
      }, 5000);
    });
  }

  // ==================== 密码显示/隐藏切换 ====================
  function initPasswordToggles() {
    document.addEventListener('click', function (e) {
      var btn = e.target.closest('.password-toggle');
      if (!btn) return;

      var wrapper = btn.closest('.password-wrapper');
      var input = wrapper.querySelector('input[type="password"], input[type="text"]');
      if (!input) return;

      var isPassword = input.type === 'password';
      input.type = isPassword ? 'text' : 'password';
      btn.setAttribute('aria-label', isPassword ? '隐藏密码' : '显示密码');

      // 切换图标: eye-open ↔ eye-slash
      var icon = btn.querySelector('.eye-icon');
      if (icon) {
        if (isPassword) {
          icon.setAttribute('data-icon', 'open');
          icon.innerHTML =
            '<path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94"></path>' +
            '<path d="M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19"></path>' +
            '<line x1="1" y1="1" x2="23" y2="23"></line>';
        } else {
          icon.setAttribute('data-icon', 'slash');
          icon.innerHTML =
            '<path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"></path>' +
            '<circle cx="12" cy="12" r="3"></circle>';
        }
      }
    });
  }

  // ==================== 初始化 ====================
  document.addEventListener('DOMContentLoaded', function () {
    Toast.init();
    initFormLoading();
    initAutoDismissAlerts();
    initPasswordToggles();
  });

  // 暴露 Toast 到全局，方便内联 script 调用
  window.Toast = Toast;

})();
