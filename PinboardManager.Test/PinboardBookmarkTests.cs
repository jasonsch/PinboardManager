using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pinboard;

namespace Pinboard.Test
{
    public partial class PinboardTests 
    {
        [TestMethod]
        public void TestTagsAreKeptSortedWhenAdding()
        {
            PinboardBookmark bookmark = new PinboardBookmark(TestBookmarkURL, "Title");

            bookmark.AddTag("Tag3");
            bookmark.AddTag("Tag2");
            bookmark.AddTag("Tag1");

            Assert.IsTrue(bookmark.TagString == "Tag1 Tag2 Tag3");
        }

        [TestMethod]
        public void TestTagsAreKeptSortedWhenRemoving()
        {
            PinboardBookmark bookmark = new PinboardBookmark(TestBookmarkURL, "Title");

            bookmark.AddTag("Tag3");
            bookmark.AddTag("Tag2");
            bookmark.AddTag("Tag1");
            bookmark.RemoveTag("Tag2");

            Assert.IsTrue(bookmark.TagString == "Tag1 Tag3");
        }
    }
}
