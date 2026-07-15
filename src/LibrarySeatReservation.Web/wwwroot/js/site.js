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

  // ==================== 初始化 ====================
  document.addEventListener('DOMContentLoaded', function () {
    Toast.init();
    initFormLoading();
    initAutoDismissAlerts();
  });

  // 暴露 Toast 到全局，方便内联 script 调用
  window.Toast = Toast;

})();
