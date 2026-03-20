const API_URL = '/api';

function switchTab(tabName) {
    document.querySelectorAll('.tab-content').forEach(tab => {
        tab.classList.remove('active');
    });
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.classList.remove('active');
    });
    document.getElementById(tabName + '-tab').classList.add('active');
    event.target.classList.add('active');
    
    if (tabName === 'books') loadBooks();
    else if (tabName === 'readers') loadReaders();
    else if (tabName === 'loans') loadLoans();
}

async function loadBooks() {
    const loading = document.getElementById('books-loading');
    const error = document.getElementById('books-error');
    const list = document.getElementById('books-list');
    
    loading.style.display = 'block';
    error.style.display = 'none';
    list.innerHTML = '';
    
    try {
        const response = await fetch(API_URL + '/books');
        if (!response.ok) throw new Error('HTTP ' + response.status);
        const books = await response.json();
        
        if (books.length === 0) {
            list.innerHTML = '<div class="empty"><div class="empty-icon">📚</div><p>Книг пока нет</p></div>';
        } else {
            let html = '<table><thead><tr><th>ID</th><th>Название</th><th>Автор</th><th>Год</th><th>Жанр</th><th>Доступно</th><th>Действия</th></tr></thead><tbody>';
            books.forEach(book => {
                html += '<tr>';
                html += '<td>' + book.id + '</td>';
                html += '<td><strong>' + book.title + '</strong></td>';
                html += '<td>' + book.author + '</td>';
                html += '<td>' + book.publicationYear + '</td>';
                html += '<td>' + (book.genre || '—') + '</td>';
                html += '<td><span class="badge ' + (book.availableCopies > 0 ? 'badge-success' : 'badge-danger') + '">' + book.availableCopies + '/' + book.totalCopies + '</span></td>';
                html += '<td><div class="action-buttons"><button class="btn btn-sm" onclick="editBook(' + book.id + ')">✏️</button><button class="btn btn-sm btn-danger" onclick="deleteBook(' + book.id + ')">🗑️</button></div></td>';
                html += '</tr>';
            });
            html += '</tbody></table>';
            list.innerHTML = html;
        }
    } catch (err) {
        error.textContent = 'Ошибка загрузки: ' + err.message;
        error.style.display = 'block';
    } finally {
        loading.style.display = 'none';
    }
}

function openAddBookModal() {
    document.getElementById('book-modal-title').textContent = 'Добавить книгу';
    document.getElementById('book-form').reset();
    document.getElementById('book-id').value = '';
    document.getElementById('book-modal').classList.add('active');
}

function closeBookModal() {
    document.getElementById('book-modal').classList.remove('active');
}

async function editBook(id) {
    try {
        const response = await fetch(API_URL + '/books/' + id);
        const book = await response.json();
        
        document.getElementById('book-modal-title').textContent = 'Редактировать книгу';
        document.getElementById('book-id').value = book.id;
        document.getElementById('book-title').value = book.title;
        document.getElementById('book-author').value = book.author;
        document.getElementById('book-isbn').value = book.isbn || '';
        document.getElementById('book-year').value = book.publicationYear;
        document.getElementById('book-genre').value = book.genre || '';
        document.getElementById('book-copies').value = book.totalCopies;
        
        document.getElementById('book-modal').classList.add('active');
    } catch (err) {
        alert('Ошибка: ' + err.message);
    }
}

async function saveBook(event) {
    event.preventDefault();
    
    const id = document.getElementById('book-id').value;
    const data = {
        title: document.getElementById('book-title').value,
        author: document.getElementById('book-author').value,
        isbn: document.getElementById('book-isbn').value || null,
        publicationYear: parseInt(document.getElementById('book-year').value) || 2024,
        genre: document.getElementById('book-genre').value || null,
        totalCopies: parseInt(document.getElementById('book-copies').value) || 1
    };
    
    try {
        const url = id ? API_URL + '/books/' + id : API_URL + '/books';
        const method = id ? 'PUT' : 'POST';
        
        const response = await fetch(url, {
            method: method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        
        if (!response.ok) throw new Error('HTTP ' + response.status);
        
        closeBookModal();
        loadBooks();
    } catch (err) {
        alert('Ошибка: ' + err.message);
    }
}

async function deleteBook(id) {
    if (!confirm('Удалить книгу?')) return;
    
    try {
        const response = await fetch(API_URL + '/books/' + id, { method: 'DELETE' });
        if (!response.ok) throw new Error('HTTP ' + response.status);
        loadBooks();
    } catch (err) {
        alert('Ошибка: ' + err.message);
    }
}

async function loadReaders() {
    const loading = document.getElementById('readers-loading');
    const error = document.getElementById('readers-error');
    const list = document.getElementById('readers-list');
    
    loading.style.display = 'block';
    error.style.display = 'none';
    list.innerHTML = '';
    
    try {
        const response = await fetch(API_URL + '/readers');
        if (!response.ok) throw new Error('HTTP ' + response.status);
        const readers = await response.json();
        
        if (readers.length === 0) {
            list.innerHTML = '<div class="empty"><div class="empty-icon">👥</div><p>Читателей пока нет</p></div>';
        } else {
            let html = '<table><thead><tr><th>ID</th><th>ФИО</th><th>Email</th><th>Телефон</th><th>Регистрация</th><th>Действия</th></tr></thead><tbody>';
            readers.forEach(reader => {
                html += '<tr>';
                html += '<td>' + reader.id + '</td>';
                html += '<td><strong>' + reader.fullName + '</strong></td>';
                html += '<td>' + reader.email + '</td>';
                html += '<td>' + (reader.phone || '—') + '</td>';
                html += '<td>' + new Date(reader.registrationDate).toLocaleDateString('ru-RU') + '</td>';
                html += '<td><button class="btn btn-sm btn-danger" onclick="deleteReader(' + reader.id + ')">🗑️</button></td>';
                html += '</tr>';
            });
            html += '</tbody></table>';
            list.innerHTML = html;
        }
    } catch (err) {
        error.textContent = 'Ошибка: ' + err.message;
        error.style.display = 'block';
    } finally {
        loading.style.display = 'none';
    }
}

function openAddReaderModal() {
    document.getElementById('reader-form').reset();
    document.getElementById('reader-modal').classList.add('active');
}

function closeReaderModal() {
    document.getElementById('reader-modal').classList.remove('active');
}

async function saveReader(event) {
    event.preventDefault();
    
    const data = {
        fullName: document.getElementById('reader-name').value,
        email: document.getElementById('reader-email').value,
        phone: document.getElementById('reader-phone').value || null
    };
    
    try {
        const response = await fetch(API_URL + '/readers', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        
        if (!response.ok) throw new Error('HTTP ' + response.status);
        
        closeReaderModal();
        loadReaders();
    } catch (err) {
        alert('Ошибка: ' + err.message);
    }
}

async function deleteReader(id) {
    if (!confirm('Удалить читателя?')) return;
    
    try {
        const response = await fetch(API_URL + '/readers/' + id, { method: 'DELETE' });
        if (!response.ok) throw new Error('HTTP ' + response.status);
        loadReaders();
    } catch (err) {
        alert('Ошибка: ' + err.message);
    }
}

async function loadLoans() {
    const loading = document.getElementById('loans-loading');
    const error = document.getElementById('loans-error');
    const list = document.getElementById('loans-list');
    
    loading.style.display = 'block';
    error.style.display = 'none';
    list.innerHTML = '';
    
    try {
        const response = await fetch(API_URL + '/bookloans');
        if (!response.ok) throw new Error('HTTP ' + response.status);
        const loans = await response.json();
        
        if (loans.length === 0) {
            list.innerHTML = '<div class="empty"><div class="empty-icon">📋</div><p>Выдач пока нет</p></div>';
        } else {
            let html = '<table><thead><tr><th>ID</th><th>Книга</th><th>Читатель</th><th>Выдана</th><th>Срок</th><th>Статус</th><th>Действия</th></tr></thead><tbody>';
            loans.forEach(loan => {
                const bookTitle = loan.book ? loan.book.title : 'N/A';
                const readerName = loan.reader ? loan.reader.fullName : 'N/A';
                const isReturned = loan.returnDate !== null;
                
                html += '<tr>';
                html += '<td>' + loan.id + '</td>';
                html += '<td>' + bookTitle + '</td>';
                html += '<td>' + readerName + '</td>';
                html += '<td>' + new Date(loan.loanDate).toLocaleDateString('ru-RU') + '</td>';
                html += '<td>' + new Date(loan.dueDate).toLocaleDateString('ru-RU') + '</td>';
                html += '<td><span class="badge ' + (isReturned ? 'badge-success' : 'badge-danger') + '">' + (isReturned ? '✓ Возвращена' : '✗ На руках') + '</span></td>';
                html += '<td>' + (isReturned ? '—' : '<button class="btn btn-sm btn-success" onclick="returnBook(' + loan.id + ')">Вернуть</button>') + '</td>';
                html += '</tr>';
            });
            html += '</tbody></table>';
            list.innerHTML = html;
        }
    } catch (err) {
        error.textContent = 'Ошибка: ' + err.message;
        error.style.display = 'block';
    } finally {
        loading.style.display = 'none';
    }
}

async function openAddLoanModal() {
    try {
        const [booksRes, readersRes] = await Promise.all([
            fetch(API_URL + '/books'),
            fetch(API_URL + '/readers')
        ]);
        
        const books = await booksRes.json();
        const readers = await readersRes.json();
        
        const bookSelect = document.getElementById('loan-book');
        const readerSelect = document.getElementById('loan-reader');
        
        bookSelect.innerHTML = '';
        books.filter(b => b.availableCopies > 0).forEach(b => {
            bookSelect.innerHTML += '<option value="' + b.id + '">' + b.title + ' (' + b.availableCopies + ' дост.)</option>';
        });
        
        readerSelect.innerHTML = '';
        readers.forEach(r => {
            readerSelect.innerHTML += '<option value="' + r.id + '">' + r.fullName + '</option>';
        });
        
        document.getElementById('loan-modal').classList.add('active');
    } catch (err) {
        alert('Ошибка: ' + err.message);
    }
}

function closeLoanModal() {
    document.getElementById('loan-modal').classList.remove('active');
}

async function saveLoan(event) {
    event.preventDefault();
    
    const data = {
        bookId: parseInt(document.getElementById('loan-book').value),
        readerId: parseInt(document.getElementById('loan-reader').value),
        loanDays: parseInt(document.getElementById('loan-days').value) || 14
    };
    
    try {
        const response = await fetch(API_URL + '/bookloans', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(data)
        });
        
        if (!response.ok) throw new Error('HTTP ' + response.status);
        
        closeLoanModal();
        loadLoans();
        loadBooks();
    } catch (err) {
        alert('Ошибка: ' + err.message);
    }
}

async function returnBook(loanId) {
    if (!confirm('Вернуть книгу?')) return;
    
    try {
        const response = await fetch(API_URL + '/bookloans/' + loanId + '/return', { method: 'PUT' });
        if (!response.ok) throw new Error('HTTP ' + response.status);
        loadLoans();
        loadBooks();
    } catch (err) {
        alert('Ошибка: ' + err.message);
    }
}

document.addEventListener('DOMContentLoaded', function() {
    document.getElementById('book-form').addEventListener('submit', saveBook);
    document.getElementById('reader-form').addEventListener('submit', saveReader);
    document.getElementById('loan-form').addEventListener('submit', saveLoan);
    loadBooks();
});