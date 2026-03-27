function dismissTempAlerts() {
  const alerts = document.querySelectorAll(".temp-alert");
  if (!alerts || alerts.length === 0) return;

  // Låt användaren hinna se feedbacken.
  window.setTimeout(() => {
    alerts.forEach((el) => el.remove());
  }, 3500);
}

document.addEventListener("DOMContentLoaded", dismissTempAlerts);

// Om sidan visas från browserns BFCache (t.ex. back/forward),
// vill vi inte att gamla alerts ska “hänga kvar”.
window.addEventListener("pageshow", (event) => {
  if (event.persisted) {
    document.querySelectorAll(".temp-alert").forEach((el) => el.remove());
  }
});
