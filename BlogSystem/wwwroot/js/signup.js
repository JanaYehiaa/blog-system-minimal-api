document.addEventListener("DOMContentLoaded", function () {
    const form = document.getElementById("signup-form");

    if (!form) return;

    form.addEventListener("submit", async function (e) {
        e.preventDefault();

        const data = {
            username: document.querySelector('input[name="Username"]').value,
            email: document.querySelector('input[name="Email"]').value,
            password: document.querySelector('input[name="Password"]').value,
            role: document.querySelector('select[name="SelectedRole"]').value
        };

        if (!data.role) 
        {
            showError("Please select a role.");
            return;
        }

        try {
            const response = await fetch("/api/users/register", {
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
                    result?.ErrorMessage || 
                    result?.Error ||
                    "Registration failed.";

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
