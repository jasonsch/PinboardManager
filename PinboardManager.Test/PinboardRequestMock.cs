using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Pinboard;

namespace Pinboard.Test
{
    class PinboardRequestMock : IPinboardRequest
    {
        private string API;
        private Dictionary<string, string> Parameters;
        private readonly JavaScriptSerializer JSSerializer = new JavaScriptSerializer();

        /// <summary>
        /// This is reference data the we expose as properties to allow tests to compare their results with what they should be.
        /// </summary>
        public PinboardBookmark ReferenceBookmark { get; private set; }
        public List<PinboardPostDate> ReferencePostDates { get; private set; }
        public PinboardSuggestedTags ReferenceSuggestedTags { get; private set; }
        public List<PinboardCountedTag> ReferenceTags { get; private set; }
        public List<PinboardNote> ReferenceNotes { get; private set; }
        public readonly DateTime ReferenceDate = new DateTime(1977, 8, 10);
        public readonly string ReferenceSuggestedTagsURL = @"http://www.sun.com/";

        public string AccessToken { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        public PinboardRequestMock(string AccessToken) : this()
        {
            this.AccessToken = AccessToken;
        }

        public PinboardRequestMock(string Username, string Password) : this()
        {
            this.Username = Username;
            this.Password = Password;
        }

        public PinboardRequestMock()
        {
            ReferenceBookmark = new PinboardBookmark("http://www.test.com/", "This is the title", "This is the description");
            ReferenceBookmark.AddTag("tag1");
            ReferenceBookmark.AddTag("tag2");
            ReferenceBookmark.CreationTime = ReferenceDate;
            ReferenceBookmark.Shared = true;
            ReferenceBookmark.ToRead = false;

            ReferencePostDates = new List<PinboardPostDate>();
            ReferencePostDates.Add(new PinboardPostDate("1977-08-10", 7));
            ReferencePostDates.Add(new PinboardPostDate("1982-02-10", 1));

            ReferenceSuggestedTags = new PinboardSuggestedTags();
            ReferenceSuggestedTags.popular = new string[] { "hardware" };
            ReferenceSuggestedTags.recommended = new string[] { "sun", "solaris", "Hardware", "java", "Bookmarks_bar", "computer", "Computer_Hardware", "Bookmarks_Menu", "ComputerHardware", "Software" };

            ReferenceTags = new List<PinboardCountedTag>();
            ReferenceTags.Add(new PinboardCountedTag("tag1", 7));
            ReferenceTags.Add(new PinboardCountedTag("tag2", 11));
            ReferenceTags.Add(new PinboardCountedTag("tag7", 41));

            ReferenceNotes = new List<PinboardNote>();
            ReferenceNotes.Add(new PinboardNote("Body 1", "ID#1", "skdfjsldf", "Title 1", 6, "1977-08-10", "1983-02-10"));
            ReferenceNotes.Add(new PinboardNote("Body 2", "ID#2", "skdfjsldf", "Title 2", 6, "1977-08-10", "1983-02-10"));
            ReferenceNotes.Add(new PinboardNote("Body 3", "ID#3", "skdfjsldf", "Title 3", 6, "1977-08-10", "1983-02-10"));
        }

        public void SetRequest(string API)
        {
            this.API = API;
            Parameters = new Dictionary<string, string>();
        }

        public void AddParameter(string name, string value)
        {
            Parameters[name] = value;
        }

        public void AddParameter(string name, uint value)
        {
            AddParameter(name, value.ToString());
        }

        public void AddParameter(string name, bool value)
        {
            AddParameter(name, PinboardManager.BoolToString(value));
        }

        public Task<string> SendRequestAsync()
        {
            if (API == "posts/update")
            {
                return Task.FromResult<string>(GetLastUpdatedTime());
            }
            else if (API == "posts/add")
            {
                return Task.FromResult<string>(AddBookmark());
            }
            else if (API == "posts/delete")
            {
                return Task.FromResult<string>(DeleteBookmark());
            }
            else if (API == "posts/get")
            {
                return Task.FromResult<string>(GetBookmarks());
            }
            else if (API == "posts/dates")
            {
                return Task.FromResult<string>(GetPostDates());
            }
            else if (API == "posts/recent")
            {
                return Task.FromResult<string>(GetRecentPosts());
            }
            else if (API == "posts/all")
            {
                return Task.FromResult<string>(GetAllPosts());
            }
            else if (API == "posts/suggest")
            {
                return Task.FromResult<string>(GetPostSuggestions());
            }
            else if (API == "tags/get")
            {
                return Task.FromResult<string>(GetTags());
            }
            else if (API == "tags/delete")
            {
                return Task.FromResult<string>(DeleteTag());
            }
            else if (API == "tags/rename")
            {
                return Task.FromResult<string>(RenameTag());
            }
            else if (API == "user/secret")
            {
                return Task.FromResult<string>(GetUserSecret());
            }
            else if (API == "user/api_token")
            {
                return Task.FromResult<string>(GetUsersToken());
            }
            else if (API == "notes/list")
            {
                return Task.FromResult<string>(GetNoteList());
            }
            else if (API.StartsWith("notes/"))
            {
                return Task.FromResult<string>(GetNote((API.Split('/'))[1]));
            }
            else
            {
                System.Diagnostics.Debug.Assert(false, "Invalid API");
            }

            // TODO
            return new Task<string>(null);
        }

        #region API stubs
        private string ConvertTimestamp(DateTime dt)
        {
            return dt.ToString("s") + "Z";
        }

        private string GetLastUpdatedTime()
        {
            Parameters.Add("update_time", ConvertTimestamp(new DateTime(1977, 08, 10)));
            return JSSerializer.Serialize(Parameters);
        }

        private string AddBookmark()
        {
            Dictionary<string, string> hash = new Dictionary<string, string>();

            if (String.IsNullOrEmpty(Parameters["url"]))
            {
                hash["result_code"] = "missing url";
            }
            else if (String.IsNullOrEmpty(Parameters["description"]))
            {
                hash["result_code"] = "missing title";
            }
            else if (Parameters["replace"] != "yes")
            {
                hash["result_code"] = PinboardManager.AddBookmarkErrorString;
            }
            else
            {
                hash["result_code"] = "done";
            }

            return JSSerializer.Serialize(hash);
        }

        private string DeleteBookmark()
        {
            Dictionary<string, string> hash = new Dictionary<string, string>();

            if (String.IsNullOrEmpty(Parameters["url"]))
            {
                hash["result_code"] = "invalid parameter";
            }
            else
            {
                hash["result_code"] = "done";
            }

            return JSSerializer.Serialize(hash);
        }

        private bool PinboardBoolFromString(string str)
        {
            return str == "yes";
        }

        private string PinboardStringFromBool(bool value)
        {
            return value ? "yes" : "no";
        }

        private string GetBookmarks()
        {
            Dictionary<string, object> hash = new Dictionary<string, object>();

            hash["date"] = DateTime.Now; // We don't currently care about this value so it doesn't matter what we put here.
            hash["user"] = "username"; // We don't use this either.

            //
            // The user can choose any combination of the three possible parameters (URL, tags, and date). Let's make
            // sure there's at least one.
            //
            if (String.IsNullOrEmpty(Parameters["url"]) && String.IsNullOrEmpty(Parameters["tags"]) && String.IsNullOrEmpty(Parameters["dt"]))
            {
                hash["posts"] = new object[0];
            }
            else if (!ValidateTags())
            {
                hash["posts"] = new object[0];
            }
            else
            {
                Dictionary<string, string> Bookmark = new Dictionary<string, string>();
                object[] array = new object[1];
                array[0] = Bookmark;
                Bookmark["href"] = ReferenceBookmark.URL;
                Bookmark["description"] = ReferenceBookmark.Title;
                Bookmark["extended"] = ReferenceBookmark.Description;
                Bookmark["meta"] = "somemetastring";
                Bookmark["hash"] = "somehashstring";
                Bookmark["time"] = ConvertTimestamp(ReferenceBookmark.CreationTime);
                Bookmark["shared"] = PinboardStringFromBool(ReferenceBookmark.Shared);
                Bookmark["toread"] = PinboardStringFromBool(ReferenceBookmark.ToRead);
                Bookmark["tags"] = ReferenceBookmark.TagString;
                hash["posts"] = array;
            }

            return JSSerializer.Serialize(hash);
        }

        private bool ValidateTags()
        {
            string TagString;

            if (Parameters.TryGetValue("tags", out TagString))
            {
                return false;
            }

            if (!String.IsNullOrEmpty(TagString))
            {
                //
                // Validate the tags.
                //
                string[] tags = TagString.Split(',');
                if (tags.Length > 3)
                {
                    return false;
                }
            }

            return true;
        }

        private string GetPostDates()
        {
            Dictionary<string, object> hash = new Dictionary<string, object>();

            hash["user"] = "username"; // We don't use this field so the value doesn't matter.
            hash["tag"] = "tag1"; // We don't use this field so the value doesn't matter.

            Dictionary<string, string> DateHash = new Dictionary<string, string>();
            foreach (PinboardPostDate PostDate in ReferencePostDates)
            {
                DateHash[PostDate.Date.ToString("yyyy-MM-dd")] = PostDate.Count.ToString();
            }

            hash["dates"] = DateHash;

            return JSSerializer.Serialize(hash);
        }

        private string GetRecentPosts()
        {
            // TODO -- Naming on this variable and "array".
            Dictionary<string, object> ObjectHash = new Dictionary<string, object>();

            if (String.IsNullOrEmpty(Parameters["count"]) || !ValidateTags())
            {
                return ""; // TODO -- What does the error look like? Just an empty posts array?
            }

            ObjectHash["date"] = DateTime.Now.ToString(); // We don't use this field so the value doesn't matter.
            ObjectHash["user"] = "username";              // We don't use this field so the value doesn't matter.
            object[] PostList = new object[1];
            // TODO -- This bookmark setup is used twice. Move into a function.
            Dictionary<string, string> Bookmark = new Dictionary<string, string>();
            Bookmark["href"] = ReferenceBookmark.URL;
            Bookmark["description"] = ReferenceBookmark.Title;
            Bookmark["extended"] = ReferenceBookmark.Description;
            Bookmark["meta"] = "somemetastring";
            Bookmark["hash"] = "somehashstring";
            Bookmark["time"] = ConvertTimestamp(ReferenceBookmark.CreationTime);
            Bookmark["shared"] = PinboardStringFromBool(ReferenceBookmark.Shared);
            Bookmark["toread"] = PinboardStringFromBool(ReferenceBookmark.ToRead);
            Bookmark["tags"] = ReferenceBookmark.TagString;
            PostList [0] = Bookmark;
            ObjectHash["posts"] = PostList;

            return JSSerializer.Serialize(ObjectHash);
        }

        private string GetAllPosts()
        {
            object[] array = new object[1];
            Dictionary<string, string> Bookmark = new Dictionary<string, string>();
            Bookmark["href"] = ReferenceBookmark.URL;
            Bookmark["description"] = ReferenceBookmark.Title;
            Bookmark["extended"] = ReferenceBookmark.Description;
            Bookmark["meta"] = "fslfjsaklfjsdflj"; // TODO
            Bookmark["hash"] = "ksjdflksjfslkdf"; // TODO
            Bookmark["time"] = ConvertTimestamp(ReferenceBookmark.CreationTime);
            Bookmark["shared"] = PinboardStringFromBool(ReferenceBookmark.Shared);
            Bookmark["toread"] = PinboardStringFromBool(ReferenceBookmark.ToRead);
            Bookmark["tags"] = ReferenceBookmark.TagString;
            array[0] = Bookmark;

            return JSSerializer.Serialize(array);
        }

        private string GetPostSuggestions()
        {
            object[] TopLevelArray = new object[2];

            Dictionary<string, object> popular = new Dictionary<string, object>();
            Dictionary<string, object> recommended = new Dictionary<string, object>();

            TopLevelArray[0] = popular;
            TopLevelArray[1] = recommended;

            popular["popular"] = ReferenceSuggestedTags.popular;
            recommended["recommended"] = ReferenceSuggestedTags.recommended;

            return JSSerializer.Serialize(TopLevelArray);
        }

        private string GetTags()
        {
            Dictionary<string, string> hash = new Dictionary<string, string>();

            foreach (PinboardCountedTag Tag in ReferenceTags)
            {
                hash[Tag.tag] = Tag.count.ToString();
            }

            return JSSerializer.Serialize(hash);
        }

        private string DeleteTag()
        {
            Dictionary<string, string> hash = new Dictionary<string, string>();

            if (String.IsNullOrEmpty(Parameters["tag"]))
            {
                return ""; // TODO
            }

            hash["result"] = "done";

            return JSSerializer.Serialize(hash);
        }

        private string RenameTag()
        {
            Dictionary<string, string> hash = new Dictionary<string, string>();

            if (String.IsNullOrEmpty(Parameters["old"]) || String.IsNullOrEmpty(Parameters["new"]))
            {
                return ""; // TODO
            }

            hash["result"] = "done";

            return JSSerializer.Serialize(hash);
        }

        private string GetUserSecret()
        {
            Dictionary<string, string> hash = new Dictionary<string, string>();

            hash["result"] = "ksdfjlsjfslkfjsldjfslfjs"; // TODO -- Not sure what this value is supposed to be.

            return JSSerializer.Serialize(hash);
        }

        private string GetUsersToken()
        {
            Dictionary<string, string> hash = new Dictionary<string, string>();

            hash["result"] = "ksdfjlsjfslkfjsldjfslfjs"; // TODO

            return JSSerializer.Serialize(hash);
        }

        private string GetNoteList()
        {
            Dictionary<string, object> hash = new Dictionary<string, object>();

            hash["count"] = ReferenceNotes.Count;

            object[] NoteArray = new object[ReferenceNotes.Count];
            int index = 0;
            foreach (PinboardNote Note in ReferenceNotes)
            {
                Dictionary<string, string> NoteHash = new Dictionary<string, string>();

                // TODO -- Duplicated code
                NoteHash["id"] = Note.ID;
                NoteHash["hash"] = Note.Hash;
                NoteHash["title"] = Note.Title;
                NoteHash["length"] = Note.Length.ToString();
                NoteHash["created_at"] = Note.CreatedDate.ToString(); // TODO
                NoteHash["updated_at"] = Note.UpdatedDate.ToString(); // TODO

                NoteArray[index] = NoteHash;
                index += 1;
            }

            hash["notes"] = NoteArray;
            return JSSerializer.Serialize(hash);
        }

        private string GetNote(object context)
        {
            Dictionary<string, object> NoteHash = new Dictionary<string, object>();
            string ID = (string)context;
            PinboardNote Note;

            Note = ReferenceNotes.Find((n) => n.ID == ID);

            // TODO -- Duplicated code
            NoteHash["id"] = Note.ID;
            NoteHash["hash"] = Note.Hash;
            NoteHash["title"] = Note.Title;
            NoteHash["length"] = Note.Length;
            NoteHash["created_at"] = Note.CreatedDate.ToString(); // TODO
            NoteHash["updated_at"] = Note.UpdatedDate.ToString(); // TODO
            NoteHash["text"] = Note.Text;

            return JSSerializer.Serialize(NoteHash);
        }
        #endregion
    }
}
