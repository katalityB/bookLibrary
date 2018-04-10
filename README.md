## Задание
Есть класс
```
public class BookLibrary
{
IEnumerable<IBookInfoSnapshot>  GetAllBooks();
IBookInfoSnapshot GetBookById(Guid bookId);
Guid AddBook(Book book);
void CheckoutBook(Guid bookId, string userName);
void ReturnBook(Guid bookId);
}
```
Эти методы работают хорошо, их тестировать не нужно.

Разработчик добавил возможность встать в очередь за книгой. Доработал старые методы и добавил новый
```
void Enqueue(Guid bookId, string userName)
```
### Что делать
В классе BookLibraryTestsTask напишите тесты на работу с очередями.