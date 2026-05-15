document.querySelectorAll('.password-toggle').forEach(btn => {
    btn.addEventListener('click', () => {
      const targetId = btn.dataset.target;
      const input = document.getElementById(targetId);
      const type = input.type === 'password' ? 'text' : 'password';
      input.type = type;
    });
  });

  const avatarInput = document.getElementById('avatar-input');
  const avatarPreview = document.getElementById('avatar-preview');
  
  if (avatarInput) {
    avatarInput.addEventListener('change', (e) => {
      const file = e.target.files[0];
      if (file) {
        const reader = new FileReader();
        reader.onload = (event) => {
          avatarPreview.src = event.target.result;
        };
        reader.readAsDataURL(file);
      }
    });
  }

  const modal = document.getElementById('login-history-modal');
  const historyBtn = document.getElementById('login-history-btn');
  const closeBtn = document.querySelector('.modal-close');

  if (historyBtn) {
    historyBtn.addEventListener('click', () => {
      modal.classList.add('active');
    });
  }

  if (closeBtn) {
    closeBtn.addEventListener('click', () => {
      modal.classList.remove('active');
    });
  }

  modal?.addEventListener('click', (e) => {
    if (e.target === modal) {
      modal.classList.remove('active');
    }
  });

  document.getElementById('reset-profile')?.addEventListener('click', () => {
    document.getElementById('profile-form').reset();
    avatarPreview.src = '../img/user-default.png';
  });

  
document.getElementById("updateSecurity").addEventListener("click", async () => EditSecurityUser());
document.getElementById("updateProfile").addEventListener("click", async () => EditProfileUser());

async function EditSecurityUser()
{
  const new_password = document.getElementById("new-password").value;
  const confirm_password = document.getElementById("confirm-password").value;

  const updateSecurity = {
    emailUser: document.getElementById("email").value,
  };

  if (new_password || confirm_password)
  {
    if (new_password !== confirm_password)
    {
      console.log("Пароли не совпадают");
      return;
    }

    updateSecurity.new_password = new_password;
  }

  console.log(updateSecurity);

  const response = await fetch("/editProfileSecurity", {
    method: "PUT",
    headers: {
      "Accept": "application/json",
      "Content-Type": "application/json"
    },
    body: JSON.stringify(updateSecurity)
  });

  if (!response.ok)
  {
    const errorText = await response.text();
    console.log(`Ошибка: ${errorText}`);
  }

  alert("Данные были изменены");
}

async function EditProfileUser()
{
  const updateProfile = {
    name: document.getElementById("first-name").value,
    surName: document.getElementById("last-name").value,
    nickName: document.getElementById("nickname").value,
    dateBirthday: document.getElementById("birthdate").value,
    numberPhone: document.getElementById("phone").value,
  };

  console.log(updateProfile);

  const response = await fetch("/editProfileUser", {
    method: "PUT",
    headers: {"Accept": "application/json", "Content-Type": "application/json"},
    body: JSON.stringify(updateProfile)
  });

  if(!response.ok)
  {
    const errorText = await response.text();
    console.log(`Ошибка: ${errorText}`);
  }

  alert("Данные были изменены");
}

async function GetProfileUserInfo() {
    const response = await fetch("/getProfileUserInfo");

    if (!response.ok) {
        const err = await response.text();
        console.log("Ошибка: " + err);
    }

    const message = await response.json();
    console.log(message)

    document.getElementById("age").value = message.age;
    document.getElementById("first-name").value = message.name;
    document.getElementById("last-name").value = message.surName;
    document.getElementById("nickname").value = message.nickName;
    document.getElementById("birthdate").value = message.dateBirthday;
    document.getElementById("phone").value = message.phoneNumber;
    document.getElementById("email").value = message.emailUser;
    document.getElementById("new-password").value = message.passwordUser;
}

GetProfileUserInfo();