// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', () => {
    const button = document.getElementById('mobile-menu-button');
    const menu = document.getElementById('mobile-menu');
    const openIcon = document.getElementById('menu-open-icon');
    const closeIcon = document.getElementById('menu-close-icon');

    if (button && menu) {
        button.addEventListener('click', () => {
            // Toggle visibility of the mobile menu
            menu.classList.toggle('hidden');

            // Toggle icon visibility (hamburger <-> X)
            openIcon.classList.toggle('hidden');
            closeIcon.classList.toggle('hidden');

            // Update aria-expanded attribute
            const isExpanded = button.getAttribute('aria-expanded') === 'true' || false;
            button.setAttribute('aria-expanded', !isExpanded);
        });
    }
});