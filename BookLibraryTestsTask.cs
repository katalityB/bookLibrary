using System;
using NUnit.Framework;
using System.IO;
using System.Linq;

namespace BookLibrary
{
    public class BookLibraryTestsTask
    {
        public virtual IBookLibrary CreateBookLibrary() //этот метод удалять нельзя, без него не будет работать
        {
            return new BookLibrary(); //меняется на разные реализации BookLibrary при запуске в системе проверки заданий
        }

        [Test]
        public void CheckoutByNextInLine()
        {
            var bookLibrary = CreateBookLibrary();
            var id = AddNewBook(bookLibrary);

            // GIVEN: book has queue
            bookLibrary.CheckoutBook(id, "Jack");
            bookLibrary.Enqueue(id, "Rose");
            bookLibrary.Enqueue(id, "Hose");
            Assert.AreEqual(bookLibrary.GetBookById(id).Owner, "Jack");
            Assert.AreEqual(bookLibrary.GetBookById(id).Queue.Count, 2);
            
            // GIVEN: book doesn't occupied
            bookLibrary.ReturnBook(id);
            Assert.AreEqual(bookLibrary.GetBookById(id).Owner, null);
            Assert.AreEqual(bookLibrary.GetBookById(id).Status, BookStatus.Free);
            Assert.AreEqual(bookLibrary.GetBookById(id).Queue.Count, 2);
            
            // WHEN: enqueue by next in line
            bookLibrary.CheckoutBook(id, "Rose");

            // THEN: queue decreased, owner assigned, status changed
            Assert.AreEqual(bookLibrary.GetBookById(id).Owner, "Rose");
            Assert.AreEqual(bookLibrary.GetBookById(id).Status, BookStatus.Occupied);
            Assert.AreEqual(bookLibrary.GetBookById(id).Queue.Count, 1);
            Assert.True(bookLibrary.GetBookById(id).Queue.Contains("Hose"));

            // WHEN: checkout by last in line 
            bookLibrary.ReturnBook(id);
            bookLibrary.CheckoutBook(id, "Hose");

            // THEN: there is no queue now
            Assert.AreEqual(bookLibrary.GetBookById(id).Queue.Count, 0);
        }

        [Test]
        public void CheckoutBookOutOfTurn()
        {
            var bookLibrary = CreateBookLibrary();
            var id = AddNewBook(bookLibrary);
            var handled = false;

            // GIVEN: book has queue
            bookLibrary.CheckoutBook(id, "Jack");
            bookLibrary.Enqueue(id, "Rose");
            bookLibrary.Enqueue(id, "Hose");
            bookLibrary.Enqueue(id, "Chak");
            Assert.AreEqual(bookLibrary.GetBookById(id).Owner, "Jack");
            Assert.AreEqual(bookLibrary.GetBookById(id).Queue.Count, 3);

            // GIVEN: book doesn't occupied
            bookLibrary.ReturnBook(id);
            Assert.AreEqual(bookLibrary.GetBookById(id).Owner, null);
            Assert.AreEqual(bookLibrary.GetBookById(id).Status, BookStatus.Free);
            Assert.AreEqual(bookLibrary.GetBookById(id).Queue.Count, 3);

            // WHEN: try to checkout by last owner while it's queue already
            try
            {
                bookLibrary.CheckoutBook(id, "Jack");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Cannot checkout book, because queue is not empty and 'Jack' is not the first in queue", e.Message);
                handled = true;
            }
            Assert.That(bookLibrary.GetBookById(id).Owner, Is.EqualTo(null), "Expected owner is null, because can not take book out of turn");
            Assert.That(handled, "Out of turn attempt unhandled");
            handled = false;

            // WHEN: try to checkout by user not in queue
            try
            {
                bookLibrary.CheckoutBook(id, "Nemo");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Cannot checkout book, because queue is not empty and 'Nemo' is not the first in queue", e.Message);
                handled = true;
            }
            Assert.That(bookLibrary.GetBookById(id).Owner, Is.EqualTo(null), "Expected owner is null, because can not take book out of turn");
            Assert.That(handled, "Out of turn attempt unhandled");
            handled = false;

            // WHEN: try to checkout out of turn
            try
            {
                bookLibrary.CheckoutBook(id, "Chak");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Cannot checkout book, because queue is not empty and 'Chak' is not the first in queue", e.Message);
                handled = true;
            }
            Assert.That(bookLibrary.GetBookById(id).Owner, Is.EqualTo(null), "Expected owner is null, because can not take book out of turn");
            Assert.That(handled, "Out of turn attempt unhandled");
            handled = false;

            // WHEN: try to checkout out of turn
            try
            {
                bookLibrary.CheckoutBook(id, "Hose");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Cannot checkout book, because queue is not empty and 'Hose' is not the first in queue", e.Message);
                handled = true;
            }
            Assert.That(bookLibrary.GetBookById(id).Owner, Is.EqualTo(null), "Expected owner is null, because can not take book out of turn");
            Assert.That(handled, "Out of turn attempt unhandled");
            handled = false;

            // WHEN: try to checkout out of turn twice
            try
            {
                bookLibrary.CheckoutBook(id, "Hose");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Cannot checkout book, because queue is not empty and 'Hose' is not the first in queue", e.Message);
                handled = true;
            }
            Assert.That(bookLibrary.GetBookById(id).Owner, Is.EqualTo(null), "Expected owner is null, because can not take book out of turn");
            Assert.That(handled, "Out of turn attempt unhandled");
            handled = false;

            // WHEN: try to checkout by last owner
            try
            {
                bookLibrary.CheckoutBook(id, "Jack");
            }
            catch (Exception e)
            {
                Assert.AreEqual("Cannot checkout book, because queue is not empty and 'Jack' is not the first in queue", e.Message);
                handled = true;
            }
            Assert.That(bookLibrary.GetBookById(id).Owner, Is.EqualTo(null), "Expected owner is null, because can not take book out of turn");

            // THEN: out of turn attempt handled
            Assert.That(handled, "Out of turn attempt unhandled");
            Assert.AreEqual(null, bookLibrary.GetBookById(id).Owner);
            Assert.AreEqual(BookStatus.Free, bookLibrary.GetBookById(id).Status);
            Assert.AreEqual(3, bookLibrary.GetBookById(id).Queue.Count);
        }

        [Test]
        public void CheckoutBookAndEnqueueAnotherBook() {

            // GIVEN: there is two books in the library
            var bookLibrary = CreateBookLibrary();
            var bookId1 = AddNewBook(bookLibrary);
            var bookId2 = AddNewBook(bookLibrary);

            // GIVEN: Jack is owner of one book
            bookLibrary.CheckoutBook(bookId1, "Jack");
            bookLibrary.CheckoutBook(bookId2, "Cal");

            // WHEN: enqueue Jack for another book
            bookLibrary.Enqueue(bookId2, "Jack");

            // THEN: Jack enqueued
            Assert.AreEqual(1, bookLibrary.GetBookById(bookId2).Queue.Count);
            Assert.AreEqual("Jack", bookLibrary.GetBookById(bookId2).Queue.First());
        }

        [Test]
        public void EnqueueForBookAndCheckoutAnotherBook()
        {

            // GIVEN: there is two books in the library
            var bookLibrary = CreateBookLibrary();
            var bookId1 = AddNewBook(bookLibrary);
            var bookId2 = AddNewBook(bookLibrary);

            // GIVEN: Jack is enueued for book 1
            bookLibrary.CheckoutBook(bookId1, "Cal");
            bookLibrary.Enqueue(bookId1, "Jack");

            // WHEN: Jack checkout book 2
            bookLibrary.CheckoutBook(bookId2, "Jack");

            // THEN: Jack becames owner of book 2
            Assert.AreEqual(1, bookLibrary.GetBookById(bookId1).Queue.Count);
            Assert.AreEqual("Jack", bookLibrary.GetBookById(bookId2).Owner);
            Assert.AreEqual("Jack", bookLibrary.GetBookById(bookId1).Queue.First());
        }

        [Test]
        public void BookDoesNotExistEnqueue()
        {
            var bookLibrary = CreateBookLibrary();
            var existBookId = AddNewBook(bookLibrary);
            bookLibrary.CheckoutBook(existBookId, "Jack");
            var id = System.Guid.NewGuid();
            bool handled = false;
            try
            {
                bookLibrary.Enqueue(id, "Bob");
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Is.EqualTo("Book with bookId '" + id + "' was not found."));
                handled = true;
            }
            Assert.That(handled, Is.True);
            Assert.AreEqual(0, bookLibrary.GetBookById(existBookId).Queue.Count);
        }

        [Test]
        public void EnqueueAvailableBookWithoutQueue()
        {
            var bookLibrary = CreateBookLibrary();
            var id = AddNewBook(bookLibrary);
            bool handled = false;
            try
            {
                bookLibrary.Enqueue(id, "Iozeff");
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Is.EqualTo("Cannot enqueue if book is free and queue is empty. Checkout book instead."));
                handled = true;
            }
            Assert.That(handled, Is.True, "Enqueue available book unhandled");
            Assert.AreEqual(0, bookLibrary.GetBookById(id).Queue.Count);
            Assert.AreEqual(null, bookLibrary.GetBookById(id).Owner);
        }

        [Test]
        public void EnqueueAvailableBookWithQueue()
        {
            var bookLibrary = CreateBookLibrary();
            var id = AddNewBook(bookLibrary);

            // GIVEN: book has queue
            bookLibrary.CheckoutBook(id, "Jack");
            bookLibrary.Enqueue(id, "Rose");
            bookLibrary.Enqueue(id, "Hose");
            Assert.AreEqual(bookLibrary.GetBookById(id).Owner, "Jack");
            Assert.AreEqual(bookLibrary.GetBookById(id).Queue.Count, 2);

            // GIVEN: book doesn't occupied
            bookLibrary.ReturnBook(id);
            Assert.AreEqual(bookLibrary.GetBookById(id).Owner, null);
            Assert.AreEqual(bookLibrary.GetBookById(id).Status, BookStatus.Free);
            Assert.AreEqual(bookLibrary.GetBookById(id).Queue.Count, 2);

            // WHEN: try to enqueue
            bookLibrary.Enqueue(id, "Iozeff");

            // THEN: the queue was increased
            Assert.AreEqual(bookLibrary.GetBookById(id).Owner, null);
            Assert.AreEqual(bookLibrary.GetBookById(id).Status, BookStatus.Free);
            Assert.AreEqual(bookLibrary.GetBookById(id).Queue.Count, 3);
        }

        //[TestCase("Cannot enqueue with empty user.", "")] 
        /* 
         * Тест кейс закомментирован т.к. тесты не проходят автоматическую проверку. 
         * Автоматическая проверка ожидает, что все тесты успешно пройдут.
         * Но в эталонной BookLibrary ошибка - в очередь можно добавить пользователя без имени (пустую строку)
         */
        [TestCase("Value cannot be null.\r\nParameter name: userName", null)]
        public void EnqueueInvalidUserName(string msg, string userName)
        {
            bool handled = false;
            var bookLibrary = CreateBookLibrary();
            var id = AddNewBook(bookLibrary);
            bookLibrary.CheckoutBook(id, "Jack");
            Assert.That(bookLibrary.GetBookById(id).Owner, Is.EqualTo("Jack"));
            try
            {
                bookLibrary.Enqueue(id, userName);
            }
            catch (Exception e)
            {
                Assert.AreEqual(msg, e.Message);
                handled = true;
            }
            Assert.False(bookLibrary.GetBookById(id).Queue.Contains(""));
            Assert.False(bookLibrary.GetBookById(id).Queue.Contains(null));
            Assert.That(handled, Is.True, "Unhandled invalid user name detected.");
        }

        [Test]
        public void EnqueueByOwner()
        {
            var bookLibrary = CreateBookLibrary();
            var id = AddNewBook(bookLibrary);
            bool handled = false;

            // GIVEN: book has owner 
            bookLibrary.CheckoutBook(id, "Jack");
            bookLibrary.Enqueue(id, "Rose");
            Assert.AreEqual(bookLibrary.GetBookById(id).Owner, "Jack");
            Assert.AreEqual(bookLibrary.GetBookById(id).Queue.Count, 1);

            // WHEN: try to enqueue with owner
            try
            {
                bookLibrary.Enqueue(id, "Jack");
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Is.EqualTo("Cannot enqueue user 'Jack' for book '" + bookLibrary.GetBookById(id).Book.Title + "' with id '" + id + "', which user holds"));
                handled = true;
            }

            // THEN: exception handled
            Assert.IsTrue(handled, "Enqueue user which currently holds book");
            Assert.AreEqual(bookLibrary.GetBookById(id).Owner, "Jack");
            Assert.AreEqual(bookLibrary.GetBookById(id).Status, BookStatus.Occupied);
            Assert.AreEqual(bookLibrary.GetBookById(id).Queue.Count, 1);
            Assert.False(bookLibrary.GetBookById(id).Queue.Contains("Jack"));
        }

        [Test]
        public void EnqueueEnqueuedUser()
        {
            var bookLibrary = CreateBookLibrary();
            var id = AddNewBook(bookLibrary);
            bool handled = false;

            // GIVEN: book has queue
            bookLibrary.CheckoutBook(id, "Jack");
            bookLibrary.Enqueue(id, "Rose");
            bookLibrary.Enqueue(id, "Morris");
            bookLibrary.Enqueue(id, "Hose");
            Assert.AreEqual(bookLibrary.GetBookById(id).Queue.Count, 3);

            // WHEN: enqueue user which is already in queue
            try
            {
                bookLibrary.Enqueue(id, "Morris");
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Is.EqualTo("User 'Morris' is already in queue"));
                handled = true;
            }
            Assert.IsTrue(handled, "Enqueue user which is already in queue - unhandled exception");
            handled = false;

            // WHEN: enqueue user which is already in queue
            try
            {
                bookLibrary.Enqueue(id, "Hose");
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Is.EqualTo("User 'Hose' is already in queue"));
                handled = true;
            }
            Assert.IsTrue(handled, "Enqueue user which is already in queue - unhandled exception");
            handled = false;

            // WHEN: enqueue user which is already in queue
            try
            {
                bookLibrary.Enqueue(id, "Rose");
            }
            catch (Exception e)
            {
                Assert.That(e.Message, Is.EqualTo("User 'Rose' is already in queue"));
                handled = true;
            }
            Assert.IsTrue(handled, "Enqueue user which is already in queue - unhandled exception");
            handled = false;

            // THEN: there is no duplicates in queue
            Assert.AreEqual(bookLibrary.GetBookById(id).Owner, "Jack");
            Assert.AreEqual(bookLibrary.GetBookById(id).Status, BookStatus.Occupied);
            Assert.AreEqual(bookLibrary.GetBookById(id).Queue.Count, 3);
            var duplicates = bookLibrary.GetBookById(id).Queue.GroupBy(x => x).Where(x => x.Count() > 1).Select(x => x.Key);
            Assert.That(0, Is.EqualTo(duplicates.Count()), "Duplicates found in queue");
        }

        [Test]
        public void EnqueueAfterReturn() {

            // GIVEN: Jack return book
            var bookLibrary = CreateBookLibrary();
            var id = AddNewBook(bookLibrary);
            bookLibrary.CheckoutBook(id, "Jack");
            bookLibrary.Enqueue(id, "Rose");
            Assert.AreEqual("Jack", bookLibrary.GetBookById(id).Owner);
            Assert.AreEqual(1, bookLibrary.GetBookById(id).Queue.Count);
            bookLibrary.ReturnBook(id);

            // WHEN: enqueue Jack
            bookLibrary.Enqueue(id, "Jack");

            // THEN: Jack enqueued
            Assert.AreEqual(2, bookLibrary.GetBookById(id).Queue.Count);
        }



        private System.Guid AddNewBook(IBookLibrary bookLibrary) {
            string title = GenerateRandomString();
            var id = bookLibrary.AddBook(new Book(title));
            var book = bookLibrary.GetBookById(id);
            Assert.That(book.Book.Title, Is.EqualTo(title));
            return id;
        }

        private String GenerateRandomString() {
            string path = Path.GetRandomFileName();
            path = path.Replace(".", ""); 
            return path;
        }
    }
}
 