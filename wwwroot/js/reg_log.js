document.getElementById("sendBtnLoger").addEventListener("click", log);

async function log() {
    try {
        const response = await fetch("/api/logger", {
            method: "POST",
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: "same-origin",
            body: JSON.stringify({
                email: document.getElementById("email_Loger").value,
                password: document.getElementById("password_Loger").value
            })
        });

        const result = await response.json();

        if (!response.ok) {
            alert(result.text);
            return;
        }

        alert(result.text);

        // Переход вручную
        window.location.href = "/index.html";
    }
    catch (err) {
        console.log("Ошибка логина: " + err);
    }
}

document.getElementById("sendBtnAuthorize").addEventListener("click", AuthorizeUser);

async function AuthorizeUser() {
    try {
        const response = await fetch("/api/authorize", {
            method: "POST",
            headers: {
                "Accept": "application/json",
                "Content-Type": "application/json"
            },
            credentials: "same-origin",
            body: JSON.stringify({
                email: document.getElementById("emailAuthorize").value,
                password: document.getElementById("password_Authorize").value,
                Repeat_Password: document.getElementById("repeat_password_Authorize").value
            })
        });

        const message = await response.text();

        if (!response.ok) {
            alert("Ошибка регистрации: " + message);
            return;
        }

        alert(message);
    }
    catch (err) {
        console.log("Ошибка регистрации: " + err);
    }
}

document.querySelectorAll('.password-toggle').forEach(btn => {
  btn.addEventListener('click', () => {
    const input = document.getElementById(btn.dataset.target);

    input.type = input.type === "password" ? "text" : "password";
  });
});