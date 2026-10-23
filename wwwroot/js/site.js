document.addEventListener("click", (event) => {
    if (!(event.target instanceof Element)) {
        return;
    }

    const dismissButton = event.target.closest('[data-bs-dismiss="alert"]');
    if (dismissButton) {
        dismissButton.closest(".alert")?.remove();
    }
});

(() => {
    const root = document.documentElement;
    const toggle = document.getElementById("themeToggle");
    const savedTheme = localStorage.getItem("tema");
    const preferredTheme = window.matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";

    const setTheme = (theme) => {
        root.setAttribute("data-bs-theme", theme);
        if (toggle) {
            toggle.innerHTML = theme === "dark"
                ? '<i class="bi bi-sun" aria-hidden="true"></i>'
                : '<i class="bi bi-moon-stars" aria-hidden="true"></i>';
            toggle.setAttribute("aria-label", theme === "dark" ? "Cambiar a modo claro" : "Cambiar a modo oscuro");
        }
    };

    setTheme(savedTheme === "dark" || savedTheme === "light" ? savedTheme : preferredTheme);
    toggle?.addEventListener("click", () => {
        const nextTheme = root.getAttribute("data-bs-theme") === "dark" ? "light" : "dark";
        localStorage.setItem("tema", nextTheme);
        setTheme(nextTheme);
    });
})();

document.querySelectorAll("form[data-date-range]").forEach((form) => {
    const desde = form.querySelector('input[name="desde"]');
    const hasta = form.querySelector('input[name="hasta"]');
    if (!(desde instanceof HTMLInputElement) || !(hasta instanceof HTMLInputElement)) {
        return;
    }

    const validarRango = () => {
        hasta.setCustomValidity(
            desde.value && hasta.value && desde.value > hasta.value
                ? "La fecha Desde no puede ser posterior a Hasta."
                : ""
        );
    };

    validarRango();
    desde.addEventListener("input", validarRango);
    desde.addEventListener("change", validarRango);
    hasta.addEventListener("input", validarRango);
    hasta.addEventListener("change", validarRango);
    form.addEventListener("submit", (event) => {
        validarRango();
        if (!hasta.checkValidity()) {
            event.preventDefault();
            hasta.reportValidity();
        }
    });
});
