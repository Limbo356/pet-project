async function getCount()
{
    const response = await fetch("/api/count", {
        method: "GET",
        headers: {"Accept": "application/json"}
    });

    try {
        if (!response.ok)
            return err;

        const count = await response.json();

        const CountUser = document.querySelector("#CountUsers");
        const pUser = document.createElement("p");
        pUser.append(count.countUser);
        CountUser.append(pUser);

        const CountBook = document.querySelector("#CountBook");
        const pBook = document.createElement("p");
        pBook.append(count.countBook);
        CountBook.append(pBook);
    }
    catch (err) {
        console.log(err);
    }
}

async function getUser() {
    const response = await fetch("/api/users", {
        method: "GET",
        headers: {"Accept": "application/json"}
    });

    try{
        if(!response.ok)
        return err;

        const user = await response.json();
        const rows = document.querySelector("#tbodyUser");

        user.forEach(users => rows.append(rowUser(users)));
    }

    catch(err){
        console.log(err);
    }
}

async function getBook(){
    const response = await fetch("/api/books", {
        method: "GET",
        headers: {"Accept": "application/json"}
    });

    try{
        if(!response.ok)
        return err
    
        const book = await response.json();
        const rows = document.querySelector("#tbodyBook");
    
        book.forEach(books => rows.append(rowBook(books)));
    }

    catch(err)
    {
        console.log(err)
    }

}

function rowUser(user)
{
    const tr = document.createElement("tr");
    tr.setAttribute("date-rowid", user.id);

    const idTd = document.createElement("td");
    idTd.append(user.id);
    tr.append(idTd);

    const loginTd = document.createElement("td");
    loginTd.append(user.name);
    tr.append(loginTd);

    const emailTd = document.createElement("td");
    emailTd.append(user.email);
    tr.append(emailTd)

    const roleTd = document.createElement("td");
    roleTd.append(user.role);
    tr.append(roleTd);

    const linkedTd = document.createElement("td");

    const editlink = document.createElement("button");
    editlink.append("Изменить");
    editlink.addEventListener("click", () => {window.location.href = `../html/edit-user.html?id=${user.id}`});
    linkedTd.append(editlink);
    
    const deletelink = document.createElement("button");
    deletelink.append("Удалить");
    deletelink.addEventListener("click", async () => await deleteUser(user.id));
    linkedTd.append(deletelink);

    tr.appendChild(linkedTd);
    return tr;
}

async function deleteUser(userId) {
    const response = await fetch(`/deleteUser/${userId}`, {
        method: "DELETE",
        headers: { "Accept": "application/json" }
    });

    if (!response.ok) {
        const errorText = await response.text();
        console.log(`Ошибка: ${errorText}`);
        return;
    }
}

function rowBook(book)
{
    const tr = document.createElement("tr");
    tr.setAttribute("data-rowid",book.pK_BookParametrsId);

    const idTd = document.createElement("td");
    idTd.append(book.pK_BookParametrsId);
    tr.append(idTd);

    const namebookTd = document.createElement("td");
    namebookTd.append(book.nameBook);
    tr.append(namebookTd);
        
    const fullnameauthorTd = document.createElement("td");
    fullnameauthorTd.append(book.authorBooks);
    tr.append(fullnameauthorTd);
        
    const gentreTd = document.createElement("td");
    gentreTd.textContent = book.gentres.join(", ");
    tr.append(gentreTd);

    const linkedTd = document.createElement("td");

    const editlink = document.createElement("button");
    editlink.append("Изменить");
    editlink.addEventListener("click", () => {window.location.href = `../html/edit-book.html?id=${book.pK_BookParametrsId}`});
    linkedTd.append(editlink);
    
    const deletelink = document.createElement("button");
    deletelink.append("Удалить");
    deletelink.addEventListener("click", async() => await deleteBook(book.pK_BookParametrsId))
    linkedTd.append(deletelink);

    tr.appendChild(linkedTd);
    return tr;
}

async function deleteBook(bookId) {
    const response = await fetch(`/deleteBook/${bookId}`, {
        method: "DELETE",
        headers: { "Accept": "application/json" }
    });

    if (!response.ok) {
        const errorText = await response.text();
        console.log(`Ошибка: ${errorText}`);
        return;
    }
}

getUser();
getBook();
getCount();