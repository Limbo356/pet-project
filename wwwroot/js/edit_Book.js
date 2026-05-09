    document.getElementById("btSaveBookInfo").addEventListener("click", async () => await editBook());
    document.getElementById("btResetInfo").addEventListener("click", async () => await getBook());

    getBook();

    async function getBook() {
        const params = new URLSearchParams(window.location.search);
        const bookId = params.get("id");

        const response = await fetch(`/getBook/${bookId}`, {
            method: "GET",
            headers: { "Accept": "application/json" }
        });

        if(!response.ok)
        {
            const errorText = await response.text();
            console.log(`Ошибка: ${errorText}`);
            return;
        }

        const book = await response.json();
        console.log(book);

        displayBook(book);
    }

    function displayBook(data)
    {
        let i = 1;
        const book = data.book;
        const gentre_form = document.getElementById("gentre-form");
        gentre_form.innerHTML = "";

        document.getElementById("bookId").value = book.bookId;
        document.getElementById("bookName").value = book.bookName;
        document.getElementById("pdfPath").value = book.pdfPathBook;
        document.getElementById("epubPath").value = book.epubPathBook;
        document.getElementById("imagePath").value = book.imagePathBook;
        document.getElementById("description").value = book.description;
        document.getElementById("authorId").value = book.authorId;
        document.getElementById("authorName").value = book.fullName;
        document.getElementById("publishingId").value = book.publishingId;
        document.getElementById("publishingName").value = book.namePublishing;
        

        book.gentres.forEach(gentre => {

            const div = document.createElement("div");
            div.classList.add("form-group");

            div.innerHTML = 
            `<label for="gentre${i}">Жанр ${i}</label>
            <input type="text" id="gentre${i}" class="gentre-input" placeholder="Например: Фантастика" value="${gentre.nameGentre}">`;

            gentre_form.append(div);
            i++;
        });
    }

    async function editBook()
    {
        const params = new URLSearchParams(window.location.search);
        const bookId = params.get("id");

        const gentres = Array.from(document.querySelectorAll(".gentre-input"))
        .map(input => ({
            nameGentre: input.value.trim()
        }))
        .filter(gentre => gentre.nameGentre !== "");

        const updateBook = {
            bookName: document.getElementById("bookName").value,
            pdfPathBook: document.getElementById("pdfPath").value,
            epubPathBook: document.getElementById("epubPath").value,
            imagePathBook: document.getElementById("imagePath").value,
            description: document.getElementById("description").value,
            authorId: Number(document.getElementById("authorId").value),
            fullName: document.getElementById("authorName").value,
            publishingId: document.getElementById("publishingId").value,
            namePublishing: document.getElementById("publishingName").value,
            gentres: gentres
        };


        const response = await fetch(`/editBook/${bookId}`, {
            method: "PUT",
            headers: {"Accept": "application/json", "Content-Type": "application/json"},
            body: JSON.stringify(updateBook)
        });

        if(!response.ok)
        {
            const errorText = await response.text();
            console.log(`Ошибка: ${errorText}`);
            return;
        }

        const book = await response.json();
        console.log(book);
    }