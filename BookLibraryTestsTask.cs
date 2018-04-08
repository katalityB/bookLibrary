using System;
using NUnit.Framework;

namespace BookLibrary
{
    public class BookLibraryTestsTask
    {
        public virtual IBookLibrary CreateBookLibrary() //этот метод удалять нельзя, без него не будет работать
        {
            return new BookLibrary(); //меняется на разные реализации BookLibrary при запуске в системе проверки заданий
        }

        //Пример теста. Должен упасть на реализации IncorrectBookLibraryAlwaysFails
        [Test]
        public void SimpleTest()
        {
            var bookLibrary = CreateBookLibrary();
            var id = bookLibrary.AddBook(new Book("Книга1"));
            var book = bookLibrary.GetBookById(id);
            Assert.That(book.Book.Title, Is.EqualTo("Книга1"));
        }
    }
	
}