getAuthorBook();

async function getAuthorBook()
{
    try
    {
        const params = new URLSearchParams(window.location.search);
        const id = params.get("id");

        if (!id) {
            console.error("ID книги не найден в URL");
            return;
        }

        const response = await fetch(`/api/book/${id}`);

        if (!response.ok) {
            console.error("Ошибка загрузки книги. Статус:", response.status);
            return;
        }

        const book = await response.json();
        console.log("Полученная книга:", book);

        displayBook(book);
    }
    catch(err)
    {
        console.log("Ошибка загрузки книги:", err);
    }
}


function displayBook(book)
{
    const container = document.getElementById("main-book");
    container.innerHTML = "";

    let gentre = "";

    if (book.gentres && book.gentres.length > 0)
    {
        book.gentres.forEach(g => gentre += `${g.nameGentre}, `);
        gentre = gentre.slice(0, -2);
    }
    else
    {
        gentre = "Не указано";
    }

    // тут мы берём id книги безопасно
    const bookId = book.bookId || book.id || book.pk_BookParametrsId || book.pk_BookParametrsID;

    console.log("ID книги для скачивания:", bookId);

    const div_img = document.createElement("div");
    div_img.classList.add("img-book");

    div_img.innerHTML = `
    <div class="book-cover-card">
        <img src="${fixImagePath(book.imagePathBook)}" alt="${book.bookName}">
    </div>

    <div class="gentre">
        <h3>Информация о книге</h3>

        <div class="meta-item">
            <span>Автор</span>
            <p>${book.fullName ?? "Неизвестно"}</p>
        </div>

        <div class="meta-item">
            <span>Издательство</span>
            <p>${book.namePublishing ?? "Не указано"}</p>
        </div>

        <div class="meta-item">
            <span>Жанры</span>
            <p>${gentre}</p>
        </div>
    </div>`;

    const info_book = document.createElement("div");
    info_book.classList.add("info-book");

    info_book.innerHTML = `
    <div class="book-header">
        <span class="book-badge">Коллекционное издание</span>
        <h1>${book.bookName ?? "Без названия"}</h1>
    </div>

    <hr>

    <div class="description-box">
        <h2>Описание</h2>
        <p>
            ${book.description ?? `
            В мрачном тоталитарном государстве жизнь человека находится под постоянным контролем.
            Главный герой пытается сохранить свободу мышления в мире, где даже чувства подчинены системе.
            Роман показывает, как власть может влиять на правду, память и саму личность человека.
            `}
        </p>
    </div>

    <div class="book-actions">
        <button class="btn primary">Читать</button>
        <button id="BTepubBook" class="btn secondary">Скачать Epub</button>
        <button id="BTpdfBook" class="btn secondary">Скачать Pdf</button>
        <button class="btn ghost">В избранное</button>
    </div>`;

    container.appendChild(div_img);
    container.appendChild(info_book);

    const downloadBtnEpub = document.getElementById("BTepubBook");
    downloadBtnEpub.addEventListener("click", () => {
        downloadBook(book.bookId, book.bookName, "epub");
    });
    

    const downloadBtnPdf = document.getElementById("BTpdfBook");

    downloadBtnPdf.addEventListener("click", () => {
        downloadBook(book.bookId, book.bookName, "pdf");
    });
}

async function downloadBook(bookId, bookName, type) {
    try {
        const response = await fetch(`/api/download-book/${bookId}/${type}`);
        if (!response.ok) {

            let errorMessage = "Не удалось скачать файл";
            try {

                const errorData = await response.json();
                if (errorData.message) {
                    errorMessage = errorData.message;
                }

            } catch { }
            throw new Error(errorMessage);
        }

        const blob = await response.blob();
        const url = URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = url;

        a.download = bookName;

        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        URL.revokeObjectURL(url);

    } catch (err) {

        console.error(err);

        alert(err.message);
    }
}

async function loadDownloadsInfo() {
    try {
        const response = await fetch("/api/downloads-left");

        if (!response.ok) {
            console.error("Ошибка загрузки лимита. Статус:", response.status);
            return;
        }

        const data = await response.json();
        const downloadBtn = document.getElementById("dwBook");

        if (!downloadBtn) return;

        let left;

        if (data.limit === -1) {
            left = -1; // безлимит
        } else {
            left = data.limit - data.used;
        }

        if (left === -1) {
            console.log("Безлимитный доступ");

            downloadBtn.disabled = false;
            downloadBtn.textContent = "Скачать (∞)";
        } 
        else if (left <= 0) {
            console.log("Лимит исчерпан");

            downloadBtn.disabled = true;
            downloadBtn.textContent = "Лимит исчерпан";
        } 
        else {
            console.log(`Осталось скачиваний: ${left}`);

            downloadBtn.disabled = false;
            downloadBtn.textContent = `Скачать (${left})`;
        }

    } catch (error) {
        console.error("Ошибка загрузки лимита скачиваний:", error);
    }
}


function fixBookPath(path) {
    if (!path) return "Не верный путь к файлу";

    let cleanPath = path.replace("C:\Users\Limbo\Desktop\Diplom Web-Site\wwwroot", "");
    return encodeURI(cleanPath);
}

function fixImagePath(path) {
    if (!path) return "\images\default.png";

    const wwwrootIndex = path.lastIndexOf("wwwroot\\");

    let relativePath = path;

    if (wwwrootIndex !== -1) {
        relativePath = path.substring(wwwrootIndex + 8);
    } else {
        relativePath = path.replace(/^[A-Za-z]:\\/, "");
    }

    relativePath = relativePath.replace(/\\/g, "/");

    if (!relativePath.startsWith("/")) {
        relativePath = "/" + relativePath;
    }

    const parts = relativePath.split("/");
    const encodedParts = parts.map(part => encodeURIComponent(part));
    const finalPath = encodedParts.join("/");

    return finalPath;
}

function getFileExtension(path) {
    const match = path.match(/\.[^/.]+$/);
    return match ? match[0] : "";
}