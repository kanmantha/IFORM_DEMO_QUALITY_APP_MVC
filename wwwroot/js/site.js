// Sidebar toggle for mobile
document.addEventListener('DOMContentLoaded', function () {
    var toggle = document.getElementById('sidebarToggle');
    if (toggle) {
        toggle.addEventListener('click', function () {
            var sidebar = document.getElementById('sidebar');
            if (sidebar) {
                sidebar.classList.toggle('show');
            }
        });
    }

    var dateEl = document.getElementById('currentDate');
    if (dateEl) {
        var now = new Date();
        var options = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' };
        dateEl.textContent = now.toLocaleDateString('en-IN', options);
    }

    // Auto-dismiss alerts after 5 seconds
    setTimeout(function () {
        document.querySelectorAll('.alert-dismissible').forEach(function (el) {
            var bsAlert = bootstrap && bootstrap.Alert ? bootstrap.Alert.getOrCreateInstance(el) : null;
            if (bsAlert) bsAlert.close();
        });
    }, 5000);
});
