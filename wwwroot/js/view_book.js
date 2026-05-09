let allBooks = [];
let currentPage = 1;
const pageSize = 50;

function goToPage(page) {
  currentPage = page;

  renderBooks();
  renderPagination();

  requestAnimationFrame(() => {
    requestAnimationFrame(() => {
      window.scrollTo({
        top: 0,
        behavior: "smooth"
      });
    });
  });
}

document.getElementById("submit_filter").addEventListener("click", PostCategoryBook);

GetBookFilter();

async function PostCategoryBook() {
    try {
        
        const response = await fetch("/category_book", {
            method: "POST",
            headers: { 
                "Accept": "application/json", 
                "Content-Type": "application/json" 
            },
            body: JSON.stringify({
              Author: document.getElementById("author_filter").value,
              Publishing_House: document.getElementById("publishing_House").value,
              Gentres: document.getElementById("gentre_filter").value
            })
        });
        
        if (!response.ok) {
            const errorData = await response.json();
            console.error("Ошибка сервера:", errorData);
            throw new Error(errorData.error || "Ошибка сервера");
        }

        const filteredBooks = await response.json();
        
        if (filteredBooks.getBook) {
            currentPage = 1;
            displayBook(filteredBooks);
        } else {
            console.error("Неверный формат ответа:", filteredBooks);
        }
        
    } catch (err) {
        console.log('Ошибка: ' + err);
    }
}

async function GetBookFilter()
{
  try{
    const response = await fetch("/getBook_filter");
    const message = await response.json();

    if(!response.ok)
    {
      return err;
    }

    const select_gentre = document.getElementById("gentre_filter");
    message.gentre.forEach(gentre_filter => {
      const option_gentre = document.createElement("option");
      option_gentre.value = gentre_filter.nameGentre;
      option_gentre.textContent = gentre_filter.nameGentre;
      select_gentre.appendChild(option_gentre);
    });

    const select_publishing = document.getElementById("publishing_House");
    message.publishing.forEach(publishing_filter => {
      const option_author = document.createElement("option");
      option_author.value = publishing_filter.namePublishing;
      option_author.textContent = publishing_filter.namePublishing;
      select_publishing.appendChild(option_author);
    });

    const select_author = document.getElementById("author_filter");
    message.authorBooks.forEach(author_filter => {
      const option_author = document.createElement("option");
      option_author.value = author_filter.fullName;
      option_author.textContent = author_filter.fullName;
      select_author.appendChild(option_author);
    });

    GetBook();
  }

  catch(err)
  {
    console.log('Ошибка: ' + err);
  }
}

async function GetBook() {
  try {
    const responseBook = await fetch("/book", {
      method: "GET",
      headers: { "Accept": "application/json" }
    });

    if (!responseBook.ok) {
      throw new Error("Ошибка сервера");
    }

    const message = await responseBook.json();

    displayBook(message);

  } catch (err) {
    console.log("Ошибка:", err);
  }
}

function displayBook(booksDate) {
  const container = document.getElementById("book-view");
  container.innerHTML = "";

  allBooks = booksDate.getBook;

  const start = (currentPage - 1) * pageSize;
  const end = start + pageSize;

  const booksToShow = allBooks.slice(start, end);

  booksToShow.forEach(book => {
    const div = document.createElement("div");
    div.classList.add("exp-box");

    let gentre = "";

    if (book.gentre && Array.isArray(book.gentre)) {
      book.gentre.forEach(g => gentre += `${g.nameGentre}, `);
      gentre = gentre.slice(0, -2);
    }

    div.innerHTML = `
      <div id="${book.bookId}" class="img-book">
        <img src="${fixImagePath(book.img)}" id="book-img">
        <div class="option">
          <h1>${book.title}</h1>
          <p>${book.author}</p>
          <p>${gentre}</p>
        </div>
      </div>
    `;

    div.addEventListener("click", () => {
      window.location.href = `../html/book.html?id=${book.bookId}`;
    });

    container.appendChild(div);
  });

  renderBooks();
  renderPagination();
}

function renderPagination() {
  const pagination = document.getElementById("pagination");
  pagination.innerHTML = "";

  const totalPages = Math.ceil(allBooks.length / pageSize);
  const maxVisible = 5; // сколько кнопок показывать

  let startPage = Math.max(1, currentPage - Math.floor(maxVisible / 2));
  let endPage = startPage + maxVisible - 1;

  if (endPage > totalPages) {
    endPage = totalPages;
    startPage = Math.max(1, endPage - maxVisible + 1);
  }

  // ⬅️ назад
  const prev = document.createElement("button");
  prev.textContent = "←";
  prev.disabled = currentPage === 1;

  prev.onclick = () => {
    currentPage--;
    renderBooks();
    renderPagination();
    scrollToTop();
  };

  pagination.appendChild(prev);

  // если есть скрытые слева
  if (startPage > 1) {
    const first = document.createElement("button");
    first.textContent = 1;
    first.onclick = () => goToPage(1);
    pagination.appendChild(first);

    const dots = document.createElement("span");
    dots.textContent = "...";
    pagination.appendChild(dots);
  }

  // основные страницы
  for (let i = startPage; i <= endPage; i++) {
    const btn = document.createElement("button");
    btn.textContent = i;

    if (i === currentPage) {
      btn.classList.add("active");
    }

    btn.onclick = () => goToPage(i);

    pagination.appendChild(btn);
  }

  // если есть скрытые справа
  if (endPage < totalPages) {
    const dots = document.createElement("span");
    dots.textContent = "...";
    pagination.appendChild(dots);

    const last = document.createElement("button");
    last.textContent = totalPages;
    last.onclick = () => goToPage(totalPages);
    pagination.appendChild(last);
  }

  // ➡️ вперед
  const next = document.createElement("button");
  next.textContent = "→";
  next.disabled = currentPage === totalPages;

  next.onclick = () => {
    currentPage++;
    renderBooks();
    renderPagination();
    scrollToTop();
  };

  pagination.appendChild(next);
}

function renderBooks() {
  const container = document.getElementById("book-view");
  container.innerHTML = "";

  const start = (currentPage - 1) * pageSize;
  const end = start + pageSize;

  const booksToShow = allBooks.slice(start, end);

  booksToShow.forEach(book => {
    const div = document.createElement("div");
    div.classList.add("exp-box");

    let gentre = "";

    if (book.gentre && Array.isArray(book.gentre)) {
      book.gentre.forEach(g => gentre += `${g.nameGentre}, `);
      gentre = gentre.slice(0, -2);
    }

    div.innerHTML = `
      <div class="img-book">
        <img src="${fixImagePath(book.img)}">
        <div class="option">
          <h1>${book.title}</h1>
          <p>${book.author}</p>
          <p>${gentre}</p>
        </div>
      </div>
    `;

    div.addEventListener("click", () => {
      window.location.href = `../html/book.html?id=${book.bookId}`;
    });

    container.appendChild(div);
  });
}

function fixImagePath(path) {
    if (!path) return "/images/default.png";

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