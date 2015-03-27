using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web;
using System.Web.Script.Serialization;

namespace Pinboard
{
    /// <summary>
    /// Represents the number of posts on a particular date.
    /// </summary>
    public class PinboardPostDate
    {
        /// <summary>
        /// The date in question.
        /// </summary>
        public DateTime Date { get; private set; }

        /// <summary>
        /// The number of posts on that date.
        /// </summary>
        public uint Count { get; private set; }

        internal PinboardPostDate(string date, uint count)
        {
            this.Date = DateTime.Parse(date);
            this.Count = count;
        }
    }

    /// <summary>
    /// Represents a particular tag and how many times it's used.
    /// </summary>
    public class PinboardCountedTag
    {
        /// <summary>
        /// The tag in question.
        /// </summary>
        public string tag { get; private set; }

        /// <summary>
        /// The number of occurrences of that tag.
        /// </summary>
        public uint count { get; private set; }

        internal PinboardCountedTag(string tag, uint count)
        {
            this.tag = tag;
            this.count = count;
        }
    }

    /// <summary>
    /// Represents the tags that Pinboard suggests for a particular URL.
    /// </summary>
    public class PinboardSuggestedTags
    {
        /// <summary>
        /// The URL that these tags were suggested for.
        /// </summary>
        public string URL { get; private set; }

        /// <summary>
        /// The tags that are popular for this particular URL.
        /// </summary>
        public string[] popular;

        /// <summary>
        /// The tags that Pinboard would recommend using for this particular URL.
        /// </summary>
        public string[] recommended;

        internal PinboardSuggestedTags(string URL)
        {
            this.URL = URL;

            this.popular = new string[0];
            this.recommended = new string[0];
        }
    }

    /// <summary>
    /// Represents a bookmark.
    /// </summary>
    public class PinboardBookmark
    {
        #region Internal Data
        private readonly List<string> TagList;
        #endregion

        #region Public Data
        /// <summary>
        /// The bookmark's URL. This is never null / empty.
        /// </summary>
        public string URL { get; private set; }

        /// <summary>
        /// The bookmark's title. This is never null / empty.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The bookmark's descriptiong. This can be null / empty.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Timestamp of when the bookmark was created. This can be
        /// set to an arbitrary value, but defaults to current wall time.
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// Whether the bookmark is publicly readable. This default to true.
        /// </summary>
        public bool Shared { get; set; }

        /// <summary>
        /// Indicates that this bookmark should be added to the user's unread list. This defaults to false.
        /// </summary>
        public bool ToRead { get; set; }
        #endregion

        /// <summary>
        /// Constructs a new PinboardBookmark object.
        /// </summary>
        /// <param name="URL">The bookmark's URL. This cannot be null / empty.</param>
        /// <param name="Title">The bookmark's title. This cannot be null / empty.</param>
        public PinboardBookmark(string URL, string Title)
        {
            this.URL = URL;
            this.Title = Title;
            this.Description = null;

            Shared = true;
            ToRead = false;

            CreationTime = DateTime.UtcNow;
            TagList = new List<string>();
        }

        /// <summary>
        /// Specifies the tags for the bookmarks. Any existing tags are overwritten.
        /// </summary>
        /// <param name="TagListString">A space-delimited list of the tags for this bookmark.</param>
        public void SetTags(string TagListString)
        {
            TagList.Clear();
            TagList.AddRange(TagListString.Split(' ').OrderBy(a => a));
        }

        /// <summary>
        /// Add a single tag to the bookmark's existing tags.
        /// </summary>
        /// <param name="tag"></param>
        public void AddTag(string tag)
        {
            TagList.Add(tag);
            TagList.Sort();
        }

        /// <summary>
        /// Remove a single tag from the bookmark's existing tags.
        /// </summary>
        /// <param name="tag">The tag to remove.</param>
        /// <returns>True if that tag was present, false otherwise.</returns>
        public bool RemoveTag(string tag)
        {
            if (TagList.Contains(tag))
            {
                TagList.Remove(tag);
                TagList.Sort();

                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Retrieves the bookmark's tags as a space-delimited string.
        /// </summary>
        public string TagString
        {
            get
            {
                return PinboardManager.FormatTags(TagList);
            }
        }

        /// <summary>
        /// Retrieves all the tags on this bookmark.
        /// </summary>
        public string[] Tags
        {
            get
            {
                return TagList.ToArray();
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string with the bookmark's URL, Title, and Description.</returns>
        public override string ToString()
        {
            return String.Format("URL: {0}, Title: '{1}', Description:'{2}'", URL, Title, Description);
        }
    }

    /// <summary>
    /// A note already stored on the Pinboard server. There is currently no way to create one locally to be saved remotely.
    /// </summary>
    public class PinboardNote
    {
        /// <summary>
        /// The date that the note was created.
        /// </summary>
        public DateTime CreatedDate { get; private set; }

        /// <summary>
        /// The date that the note was last updated.
        /// </summary>
        public DateTime UpdatedDate { get; private set; }

        /// <summary>
        /// The text / body of the note.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// A unique ID for this note.
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// A 20 character long SHA1 hash of the Text.
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        /// The note's title.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// The note's length in bytes.
        /// </summary>
        public uint Length { get; private set; }

        internal PinboardNote(string Text, string ID, string Hash, string Title, uint Length, string CreatedTimestamp, string UpdatedTimestamp)
        {
            this.Text = Text;
            this.ID = ID;
            this.Hash = Hash;
            this.Title = Title;
            this.Length = Length;
            this.CreatedDate = DateTime.Parse(CreatedTimestamp);
            this.UpdatedDate = DateTime.Parse(UpdatedTimestamp);
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string.</returns>
        public override string ToString()
        {
            return String.Format("ID: {0}, Title: '{1}', Created At: {2}\nBody: {3}", ID, Title, CreatedDate.ToString(), Text);
        }
    }
}