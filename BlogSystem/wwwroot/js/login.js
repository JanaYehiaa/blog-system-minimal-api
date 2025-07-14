document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("login-form");

    if (!form) return;

    form.addEventListener("submit", async function (e) {
        e.preventDefault();

        const data = {
            username: document.querySelector('input[name="Username"]').value,
            password: document.querySelector('input[name="Password"]').value
        };

        try {
            const response = await fetch("/api/users/login", {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify(data)
            });

            const result = await response.json();
            const errorContainer = document.getElementById("error-message");

            if (response.ok) {
                localStorage.setItem("token", result.token);
                window.location.href = "/";
            } else {
                const errorMsg =
                    result.errorMessage ||
                    result.error ||
                    "Login failed. Please check your credentials.";

                if (errorContainer) {
                    errorContainer.textContent = errorMsg;
                } else {
                    alert(errorMsg);
                }
            }
        } catch (err) {
            console.error(err);
            alert("Something went wrong. Please try again.");
        }
    });
});
