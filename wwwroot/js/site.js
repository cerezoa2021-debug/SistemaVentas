document.addEventListener("click", (event) => {
    if (!(event.target instanceof Element)) {
        return;
    }

    const dismissButton = event.target.closest('[data-bs-dismiss="alert"]');
    if (dismissButton) {
        dismissButton.closest(".alert")?.remove();
    }
});
