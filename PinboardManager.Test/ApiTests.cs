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
    public partial class PinboardTests
    {
        private PinboardManager Pinboard;
        private PinboardRequestMock RequestObject;
        private const string TestBookmarkURL = "http://www.test.com/";

        [TestInitialize]
        public void TestSetup()
        {
            Pinboard = new PinboardManager(typeof(PinboardRequestMock), "accesstoken1");
            RequestObject = (PinboardRequestMock)Pinboard.RequestObject;
        }

        [TestMethod]
        public void TestAccessTokenPlumbing()
        {
            string TestAccessToken = "user:token";

            PinboardManager mgr = new PinboardManager(typeof(PinboardRequestMock), TestAccessToken);
            PinboardRequestMock request = (PinboardRequestMock)mgr.RequestObject;

            Assert.IsTrue(request.AccessToken == TestAccessToken);
        }

        [TestMethod]
        public void TestUsernamePasswordPlumbing()
        {
            string TestUsername = "username";
            string TestPassword = "password";

            PinboardManager mgr = new PinboardManager(typeof(PinboardRequestMock), TestUsername, TestPassword);
            PinboardRequestMock request = (PinboardRequestMock)mgr.RequestObject;

            Assert.IsTrue(request.Username == TestUsername);
            Assert.IsTrue(request.Password == TestPassword);
        }

        [TestMethod]
        public void TestUpdateTimeStamp()
        {
            DateTime UpdateTime = Pinboard.GetLastUpdateDate().Result;
            Assert.IsTrue(UpdateTime == RequestObject.ReferenceDate.ToLocalTime());
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
            Assert.IsTrue(bookmark.TagString == RequestObject.ReferenceBookmark.TagString);
        }

        [TestMethod]
        public void TestGetBookmarksByDate()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestGetBookmarksByTags()
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestRecentPostsWithCountWithTag()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestPostsRecentWithNoCountWithTag()
        {
            throw new NotImplementedException();
        }

        [TestMethod]
        public void TestPostsRecentWithNoCountWithNoTag()
        {
            throw new NotImplementedException();
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
            PinboardSuggestedTags tags = Pinboard.GetSuggestedTags(RequestObject.ReferenceSuggestedTagsURL).Result;

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
        public void TestDeleteValidTag()
        {
            bool b = Pinboard.DeleteTag("tag1").Result;
            Assert.IsTrue(b);
        }

        [TestMethod]
        public void TestDeleteInvalidTag()
        {
            bool b = Pinboard.DeleteTag("").Result;
            Assert.IsFalse(b);
        }

        [TestMethod]
        public void TestRenameTagValid()
        {
            bool b = Pinboard.RenameTag("tag1", "tag2").Result;
            Assert.IsTrue(b);
        }

        [TestMethod]
        public void TestRenameInvalidTag()
        {
            bool b = Pinboard.RenameTag("sjflsdj", "tag2").Result;
            Assert.IsFalse(b);
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
