    document.getElementById("btSaveUpdateInfo").addEventListener("click", async () => await editUser());
    document.getElementById("btResetInfo").addEventListener("click", async () => await getUser());

    getUser();

    async function getUser() {
        const params = new URLSearchParams(window.location.search);
        const userId = params.get("id");

        const response = await fetch(`/getUser/${userId}`, {
            method: "GET",
            headers: { "Accept": "application/json" }
        });

        if(!response.ok)
        {
            const errorText = await response.text();
            console.log(`Ошибка: ${errorText}`);
            return;
        }

        const user = await response.json();
        console.log(user);

        displayUser(user);
    }

    function displayUser(data)
    {
        const user = data.user;
        const formattedDate = user.lastDownloadDate
            ? user.lastDownloadDate.split("T")[0]
            : "";

        const formattedBirthdayDate = user.birthdayDate
        ? user.birthdayDate.split("T")[0]
        : "";

        document.getElementById("userId").value = user.id;
        document.getElementById("userName").value = user.name;
        document.getElementById("userSurName").value = user.surName;
        document.getElementById("userAge").value = user.age;
        document.getElementById("userPhone").value = user.numberPhone;
        document.getElementById("downloadToday").value = user.downloadToday;
        document.getElementById("lastDownloadDate").value = formattedDate;
        document.getElementById("BirthdayDate").value = formattedBirthdayDate;
        document.getElementById("emailUser").value = user.emailUser;
        document.getElementById("passwordUser").value = user.passwordUser;
    }

    async function editUser()
    {
        const params = new URLSearchParams(window.location.search);
        const userId = params.get("id");

        const updateUser = {
            name: document.getElementById("userName").value,
            surName: document.getElementById("userName").value,
            age: document.getElementById("userAge").value,
            numberPhone: document.getElementById("userPhone").value,
            downloadToday: document.getElementById("downloadToday").value,
            birthdayDate: document.getElementById("BirthdayDate").value,
            lastDOwnloadDate: document.getElementById("lastDownloadDate").value,
            emailUser: document.getElementById("emailUser").value,
            passwordUser: document.getElementById("passwordUser").value,
        };
        
        const response = await fetch(`/editUser/${userId}`, {
            method: "PUT",
            headers: {"Accept": "application/json", "Content-Type": "application/json"},
            body: JSON.stringify(updateUser)
        });

        if(!response.ok)
        {
            const errorText = await response.text();
            console.log(`Ошибка: ${errorText}`);
            return;
        }

        const user = await response.json();
        console.log(user);
    }

    document.addEventListener("change", function (e) {
    if (e.target && e.target.id === "showPassword") {
        const passwordInput = document.getElementById("passwordUser");
        passwordInput.type = e.target.checked ? "text" : "password";
    }});