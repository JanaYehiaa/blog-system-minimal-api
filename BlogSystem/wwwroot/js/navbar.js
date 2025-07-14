import { getUserInfo, isLoggedIn, logout } from '/js/auth.js';
console.log("Navbar JS running...");

document.addEventListener("DOMContentLoaded", () => {
    const nav = document.getElementById("nav-links");
    nav.innerHTML = ""; // Clear existing links if any

    const user = getUserInfo();

    if (isLoggedIn() && user) {
        const { role, username } = user;

        // Role-specific links
        if (role === "Admin") {
            nav.innerHTML += `
                <li class="nav-item me-3"><a href="/create">Create</a></li>
                <li class="nav-item me-3"><a href="/publish">Publish</a></li>
                <li class="nav-item me-3"><a href="/myposts">My Posts</a></li>
            `;
        } else if (role === "Author") {
            nav.innerHTML += `
                <li class="nav-item me-3"><a href="/create">Create</a></li>
                <li class="nav-item me-3"><a href="/myposts">My Posts</a></li>
            `;
        } else if (role === "Editor") {
            nav.innerHTML += `
                <li class="nav-item me-3"><a href="/editqueue">Edit</a></li>
            `;
        }

        // Logout button
        nav.innerHTML += `<li class="nav-item me-3"><a href="#" id="logout-btn">Logout</a></li>`;

        document.getElementById("logout-btn").addEventListener("click", (e) => {
            e.preventDefault();
            logout();
        });
    } else {
        // If not logged in, show login/signup
        nav.innerHTML += `
            <li class="nav-item me-3"><a href="/signup">Sign Up</a></li>
            <li class="nav-item me-3"><a href="/login">Log In</a></li>
`;
    }
    
    const ctaSection = document.getElementById("cta-section");
    if (ctaSection) {
        if (isLoggedIn()) {
            ctaSection.style.display = "none";
        } else {
            ctaSection.style.display = "block";
        }
    }
});
