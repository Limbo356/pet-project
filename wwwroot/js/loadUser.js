async function loadCurrentUser() {
    try {
        const response = await fetch("/api/current-user", {
            method: "GET",
            credentials: "same-origin"
        });

        const text = await response.text();
        if (!text) return;

        const user = JSON.parse(text);

        const registerBtn = document.getElementById("registerBtn");
        const adminBtn = document.getElementById("adminLink");
        const userMenu = document.getElementById("userMenu");

        if (user.isAuth) {
            registerBtn.classList.add("hidden");
            userMenu.classList.remove("hidden");

            if(user.role === "Admin")
            {
                adminBtn.classList.remove("hidden");
            } else {
                adminBtn.classList.add("hidden");
            }
        } else {
            registerBtn.classList.remove("hidden");
            userMenu.classList.add("hidden");
        }
    }
    catch (err) {
        console.log("Ошибка загрузки пользователя: " + err);
    }
}

document.addEventListener("click", async (e) => {
    if (e.target.id === "logoutBtn") {
        await fetch("/api/logout", {
            method: "POST",
            credentials: "same-origin"
        });

        location.reload();
    }
});

document.addEventListener("DOMContentLoaded", loadCurrentUser);