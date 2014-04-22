using System;
using System.Collections.Generic;
using Pinboard;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Pinboard.Test
{
    /// <summary>
    /// This tests the 
    /// </summary>
    [TestClass]
    public class ApiTests
    {
        private PinboardManager Pinboard;
        private PinboardRequestMoq RequestObject;
        private const string TestBookmarkURL = "http://www.test.com/";

        [TestInitialize]
        public void TestSetup()
        {
            RequestObject = new PinboardRequestMoq();
            Pinboard = new PinboardManager("APITOKEN", RequestObject);
        }

        [TestMethod]
        public void TestTagsAreKeptSorted()
        {
            PinboardBookmark bookmark = new PinboardBookmark(TestBookmarkURL, "Title");

            bookmark.AddTag("Tag3");
            bookmark.AddTag("Tag2");
            bookmark.AddTag("Tag1");

            Assert.IsTrue(bookmark.Tags == "Tag1,Tag2,Tag3");
        }

        [TestMethod]
        public void TestUpdateTimeStamp()
        {
            DateTime UpdateTime = Pinboard.GetLastUpdateDate().Result;
            Assert.IsTrue(UpdateTime == new DateTime(1977, 8, 10).ToLocalTime());
        }

        [TestMethod]
        public void TestAddValidBookmark()
        {
            PinboardBookmark bookmark = new PinboardBookmark(TestBookmarkURL, "Test Title", "Test Description");
            bookmark.AddTag("test1");
            bookmark.CreationTime = DateTime.Now;
            bookmark.Shared = true;
            bookmark.ToRead = false;

            bool b = Pinboard.AddBookmark(bookmark, true).Result;
            Assert.IsTrue(b);
        }

        [TestMethod]
        public void TestAddInvalidBookmarkNoURL()
        {
            PinboardBookmark bookmark = new PinboardBookmark("", "Title");

            bool b = Pinboard.AddBookmark(bookmark, true).Result;
            Assert.IsFalse(b);
        }

        [TestMethod]
        public void TestAddInvalidBookmarkNoTitle()
        {
            PinboardBookmark bookmark = new PinboardBookmark(TestBookmarkURL, "");

            bool b = Pinboard.AddBookmark(bookmark, true).Result;
            Assert.IsFalse(b);
        }

        [TestMethod]
        public void TestAddDuplicateBookmark()
        {
            PinboardBookmark bookmark = new PinboardBookmark(TestBookmarkURL, "Test Title", "Test Description");
            bookmark.AddTag("test1");
            bookmark.CreationTime = DateTime.Now;
            bookmark.Shared = true;
            bookmark.ToRead = false;

            bool b = Pinboard.AddBookmark(bookmark, false).Result;
            Assert.IsFalse(b);
        }

        [TestMethod]
        public void TestGetBookmarkByURL()
        {
            PinboardBookmark bookmark;

            bookmark = Pinboard.GetBookmarkByURL(TestBookmarkURL).Result;
            Assert.IsTrue(bookmark.URL == RequestObject.ReferenceBookmark.URL);
            Assert.IsTrue(bookmark.Title == RequestObject.ReferenceBookmark.Title);
            Assert.IsTrue(bookmark.Description == RequestObject.ReferenceBookmark.Description);
            Assert.IsTrue(bookmark.CreationTime == RequestObject.ReferenceBookmark.CreationTime.ToLocalTime());
            Assert.IsTrue(bookmark.ToRead == RequestObject.ReferenceBookmark.ToRead);
            Assert.IsTrue(bookmark.Shared == RequestObject.ReferenceBookmark.Shared);
            Assert.IsTrue(bookmark.Tags == RequestObject.ReferenceBookmark.Tags);
        }

        [TestMethod]
        public void TestGetBookmarksByDate()
        {
            // TODO
        }

        [TestMethod]
        public void TestGetBookmarksByTags()
        {
            // TODO
        }

        [TestMethod]
        public void TestDeleteBookmark()
        {
            bool b = Pinboard.DeleteBookmark(TestBookmarkURL).Result;
            Assert.IsTrue(b);
        }

        [TestMethod]
        public void TestPostDatesWithTag()
        {
            List<string> TagList = new List<string>() { "tag1" };
            List<PinboardPostDate> PostDates = Pinboard.GetPostsByDate(TagList).Result;
            foreach (var PostDate in PostDates)
            {
                Assert.IsNotNull(RequestObject.ReferencePostDates.Find(pd => pd.Count == PostDate.Count && pd.Date == PostDate.Date));
            }
        }

        [TestMethod]
        public void TestPostDatesWithoutTag()
        {
            List<PinboardPostDate> PostDates = Pinboard.GetPostsByDate().Result;

            foreach (var PostDate in PostDates)
            {
                Assert.IsNotNull(RequestObject.ReferencePostDates.Find(pd => pd.Count == PostDate.Count && pd.Date == PostDate.Date));
            }
        }

        [TestMethod]
        public void TestRecentPostsWithCountNoTag()
        {
            List<PinboardBookmark> Bookmarks = Pinboard.GetRecentBookmarks(15).Result;
            // TODO
        }

        [TestMethod]
        public void TestRecentPostsWithCountWithTag()
        {
            List<PinboardBookmark> Bookmarks = Pinboard.GetRecentBookmarks(15).Result;
            // TODO
        }

        [TestMethod]
        public void TestPostsRecentWithNoCountWithTag()
        {
            List<PinboardBookmark> Bookmarks = Pinboard.GetRecentBookmarks(15).Result;
            // TODO
        }

        [TestMethod]
        public void TestPostsRecentWithNoCountWithNoTag()
        {
            List<PinboardBookmark> Bookmarks = Pinboard.GetRecentBookmarks(15).Result;
            // TODO
        }

        [TestMethod]
        public void TestGetAllBookmarks()
        {
            List<PinboardBookmark> Bookmarks = Pinboard.GetAllBookmarks().Result;

            foreach (var Bookmark in Bookmarks)
            {
                Assert.IsTrue(Bookmark.URL == RequestObject.ReferenceBookmark.URL);
            }
        }

        [TestMethod]
        public void TestTagSuggestionsWithURL()
        {
            PinboardSuggestedTags tags = Pinboard.GetSuggestedTags("http://www.test.com/").Result; // TODO -- This URL shouldn't be hard-coded

            Assert.IsTrue(tags.popular.Length == RequestObject.ReferenceSuggestedTags.popular.Length);
            Assert.IsTrue(tags.recommended.Length == RequestObject.ReferenceSuggestedTags.recommended.Length);
        }

        [TestMethod]
        public void TestGetTags()
        {
            List<PinboardCountedTag> Tags = Pinboard.GetTags().Result;

            foreach (var Tag in Tags)
            {
                Assert.IsNotNull(RequestObject.ReferenceTags.Find(t => t.tag == Tag.tag && t.count == Tag.count));
            }
        }

        [TestMethod]
        public void TestDeleteTag() // TODO -- Another function that calls with ""?
        {
            bool b = Pinboard.DeleteTag("tag1").Result;
            Assert.IsTrue(b);
        }

        [TestMethod]
        public void TestRenameTag() // TODO -- Another function that calls with ""?
        {
            bool b = Pinboard.RenameTag("tag1", "tag2").Result;
            Assert.IsTrue(b);
        }

        [TestMethod]
        public void TestUsersSecret()
        {
            string secret = Pinboard.GetUsersSecret().Result;
            Assert.IsFalse(String.IsNullOrEmpty(secret));
        }

        [TestMethod]
        public void TestUsersApiToken()
        {
            string token = Pinboard.GetUsersApiToken().Result;
            Assert.IsFalse(String.IsNullOrEmpty(token));
        }

        [TestMethod]
        public void TestGetNotes()
        {
            List<PinboardNote> NoteList = Pinboard.GetNotes(true).Result;

            foreach (PinboardNote Note in NoteList)
            {
                Assert.IsNotNull(RequestObject.ReferenceNotes.Find(rn => rn.Title == Note.Title && rn.Hash == Note.Hash && rn.ID == rn.ID));
            }
        }

        [TestMethod]
        public void TestGetNote()
        {
            PinboardNote Note = Pinboard.GetNote(RequestObject.ReferenceNotes[0].ID).Result;

            Assert.IsTrue(Note.Title == RequestObject.ReferenceNotes[0].Title);
            Assert.IsTrue(Note.Hash == RequestObject.ReferenceNotes[0].Hash);
            Assert.IsTrue(Note.ID == RequestObject.ReferenceNotes[0].ID);
            Assert.IsTrue(Note.Text == RequestObject.ReferenceNotes[0].Text);
        }
    }
}
