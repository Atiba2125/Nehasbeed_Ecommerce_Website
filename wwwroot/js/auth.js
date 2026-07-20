/* Auth JS */
document.addEventListener('DOMContentLoaded', function () {
  // Password toggle
  const toggleBtns = document.querySelectorAll('[data-toggle-password]');
  toggleBtns.forEach(function (btn) {
    btn.addEventListener('click', function () {
      const target = document.getElementById(btn.dataset.togglePassword);
      if (!target) return;
      if (target.type === 'password') {
        target.type = 'text';
        btn.innerHTML = '<i class="fa-regular fa-eye-slash"></i>';
      } else {
        target.type = 'password';
        btn.innerHTML = '<i class="fa-regular fa-eye"></i>';
      }
    });
  });
});
