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
    /// Used to communicate with http://pinboard.in/.
    /// </summary>
    public class PinboardManager
    {
        //
        // The server will return this as the result code if we try to delete a bookmark
        // via a URL that doesn't correspond to an actual bookmark.
        //
        internal const string DeleteBookmarkErrorString = "item not found";

        //
        // The server will return this as the result code if we try to add a bookmark
        // via a URL that already has a corresponding bookmark.
        //
        internal const string AddBookmarkErrorString = "item already exists";

        //
        // The interface used to talk to the server. This will be created only once. This
        // is internal so test code can get a pointer to the mock that we create for them.
        //
        internal readonly IPinboardRequest RequestObject;

        //
        // Used to parse the JSON returned by the server.
        //
        private readonly JavaScriptSerializer JsonSerializer = new JavaScriptSerializer();

        /// <summary>
        /// Constructs a new PinboardManager object using the specified user's account and password.
        /// </summary>
        /// <param name="UserName">The user's user name.</param>
        /// <param name="Password">The user's password.</param>
        public PinboardManager(string UserName, string Password) : this(typeof(PinboardRequest), UserName, Password)
        {
        }

        /// <summary>
        /// Constructs a new PinboardManager object using the user's API token. This token can be found on the
        /// user's settings page (under the "password" tab).
        /// </summary>
        /// <param name="ApiToken">A string of the form "username:hexvalues".</param>
        public PinboardManager(string ApiToken) : this(typeof(PinboardRequest), ApiToken)
        {
        }

        internal PinboardManager(Type type, string UserName, string Password)
        {
            var Constructor = type.GetConstructor(new Type[] { typeof(string), typeof(string) });
            RequestObject = (IPinboardRequest)Constructor.Invoke(new object[] { UserName, Password });
        }

        internal PinboardManager(Type type, string ApiToken)
        {
            var Constructor = type.GetConstructor(new Type[] { typeof(string) });
            RequestObject = (IPinboardRequest)Constructor.Invoke(new object[] { ApiToken });
        }

        /// <summary>
        /// Returns the most recent time a bookmark was added, updated, or deleted.
        /// </summary>
        /// <returns>If successful, the time that the user's bookmarks were last updated. Otherwise, default(DateTime) is returned.</returns>
        public Task<DateTime> GetLastUpdateDate()
        {
            RequestObject.SetRequest("posts/update");

            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                string content = task.Result;
                Dictionary<string, object> json = (Dictionary<string, object>)JsonSerializer.DeserializeObject(content);
                return DateTime.Parse((string)json["update_time"]);
            });
        }

        #region Bookmark-related APIs
        /// <summary>
        /// Returns a list of all of the user's bookmarks and notes.
        /// </summary>
        /// <returns>A list of PinboardBookmark objects representing the user's posts.</returns>
        public Task<List<PinboardBookmark>> GetAllBookmarks()
        {
            RequestObject.SetRequest("posts/all");

            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                List<PinboardBookmark> List = new List<PinboardBookmark>();

                string content = task.Result;
                dynamic PostObjects = JsonSerializer.DeserializeObject(content);
                foreach (Dictionary<string, object> PostObject in PostObjects)
                {
                    PinboardBookmark Bookmark = new PinboardBookmark((string)PostObject["href"], (string)PostObject["description"]);
                    Bookmark.Description = (string)PostObject["extended"];
                    Bookmark.SetTags((string)PostObject["tags"]);
                    Bookmark.CreationTime = DateTime.Parse((string)PostObject["time"]);
                    List.Add(Bookmark);
                }

                return List;
            });
        }

        /// <summary>
        /// Deletes a specified bookmark.
        /// </summary>
        /// <param name="url">The PinboardBookmark's URL to delete.</param>
        /// <returns>true if successful, false if the user didn't have a bookmark with the specified URL.</returns>
        public Task<bool> DeleteBookmark(string URL)
        {
            //
            // The URL is a required parameter so let's do some light local validation.
            //
            if (String.IsNullOrEmpty(URL))
            {
                return Task.FromResult<bool>(false);
            }

            RequestObject.SetRequest("posts/delete");

            RequestObject.AddParameter("url", URL);

            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                string content = task.Result;

                Dictionary<string, object> json = (Dictionary<string, object>)JsonSerializer.DeserializeObject(content);
                if ((string)json["result_code"] == DeleteBookmarkErrorString)
                {
                    //
                    // The user doesn't have a bookmark with that URL.
                    //
                    return false;
                }

                return true;
            });
        }

        /// <summary>
        /// Gets the PinboardBookmark object for the specified URL.
        /// </summary>
        /// <param name="URL">The URL for which you want the corresponding PinboardBookmark.</param>
        /// <returns>A PinboardBookmark if the user has a bookmark for the specified URL, null otherwise.</returns>
        public Task<PinboardBookmark> GetBookmarkByURL(string URL)
        {
            RequestObject.SetRequest("posts/get");

            RequestObject.AddParameter("url", URL);

            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                string content = task.Result;
                Dictionary<string, object> hash = (Dictionary<string, object>)JsonSerializer.DeserializeObject(content);

                //
                // This API returns a list of posts, even though we know in this case there can be at most one result.
                //
                Object[] PostsArray = (Object[])hash["posts"];

                //
                // Check if there were actually any results.
                //
                if (PostsArray.Length == 0)
                {
                    return null;
                }

                System.Diagnostics.Debug.Assert(PostsArray.Length == 1);
                Dictionary<string, object> PostObject = PostsArray[0] as Dictionary<string, object>;
                PinboardBookmark Bookmark = new PinboardBookmark((string)PostObject["href"], (string)PostObject["description"]);
                Bookmark.Description = (string)PostObject["extended"];
                Bookmark.Shared = StringToBool((string)PostObject["shared"]);
                Bookmark.ToRead = StringToBool((string)PostObject["toread"]);
                Bookmark.SetTags((string)PostObject["tags"]);
                Bookmark.CreationTime = DateTime.Parse((string)PostObject["time"]);

                return Bookmark;
            });
        }

        /// <summary>
        /// Gets a list of bookmarks that match the given criteria.
        /// </summary>
        /// <param name="Tags">A list of up to three tags used to filter the results.</param>
        /// <param name="Date">The date of the bookmarks to retrieve. If no date is specified the date of the most recent post is used.</param>
        /// <returns>A (possibly empty) list of PinboardBookmarks that match the specified criteria.</returns>
        public Task<List<PinboardBookmark>> GetBookmarks(List<string> Tags = null, DateTime Date = default(DateTime))
        {
            RequestObject.SetRequest("posts/get");

            if (Tags != null)
            {
                RequestObject.AddParameter("tag", FormatTags(Tags));
            }

            if (Date != default(DateTime))
            {
                RequestObject.AddParameter("dt", ConvertTimestamp(Date));
            }

            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                List<PinboardBookmark> BookmarkList = new List<PinboardBookmark>();

                string content = task.Result;
                dynamic PostObjects = JsonSerializer.DeserializeObject(content);
                foreach (Dictionary<string, object> PostObject in PostObjects["posts"])
                {
                    PinboardBookmark Bookmark = new PinboardBookmark((string)PostObject["href"], (string)PostObject["description"]);
                    Bookmark.Description = (string)PostObject["extended"];
                    Bookmark.Shared = StringToBool((string)PostObject["shared"]);
                    Bookmark.ToRead = StringToBool((string)PostObject["toread"]);
                    Bookmark.SetTags((string)PostObject["tags"]);
                    Bookmark.CreationTime = DateTime.Parse((string)PostObject["time"]);
                    BookmarkList.Add(Bookmark);
                }

                return BookmarkList;
            });
        }

        /// <summary>
        /// Retrieves <i>count</i> number of the user's most recent posts.
        /// </summary>
        /// <param name="count">The number of posts to retrieve.</param>
        /// <returns>A (possibly empty) list of PinboardBookmark objects.</returns>
        public Task<List<PinboardBookmark>> GetRecentBookmarks(uint count)
        {
            return GetRecentBookmarks(count, null);
        }

        /// <summary>
        /// Retrieves <i>count</i> number of the user's most recent posts filtered by specific tags.
        /// </summary>
        /// <param name="count">The number of posts to retrieve.</param>
        /// <param name="Tags">A list of up to three tags to filter on.</param>
        /// <returns>A (possibly empty) list of PinboardBookmarks.</returns>
        public Task<List<PinboardBookmark>> GetRecentBookmarks(uint count, List<string> Tags)
        {
            RequestObject.SetRequest("posts/recent");
            RequestObject.AddParameter("count", count);

            if (Tags != null)
            {
                RequestObject.AddParameter("tag", FormatTags(Tags));
            }

            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                List<PinboardBookmark> PostList = new List<PinboardBookmark>();

                string content = task.Result;
                dynamic PostObjects = JsonSerializer.DeserializeObject(content);
                foreach (Dictionary<string, object> PostObject in PostObjects["posts"])
                {
                    PinboardBookmark Bookmark = new PinboardBookmark((string)PostObject["href"], (string)PostObject["description"]);
                    Bookmark.Description = (string)PostObject["extended"];
                    Bookmark.Shared = StringToBool((string)PostObject["shared"]);
                    Bookmark.ToRead = StringToBool((string)PostObject["toread"]);
                    Bookmark.SetTags((string)PostObject["tags"]);
                    Bookmark.CreationTime = DateTime.Parse((string)PostObject["time"]);
                    PostList.Add(Bookmark);
                }

                return PostList;
            });
        }

        /// <summary>
        /// Adds a bookmark.
        /// </summary>
        /// <param name="Bookmark">A PinboardBookmark to add.</param>
        /// <param name="ReplaceExisting">If true then any existing bookmark for the specified URL will be replaced. If a bookmark already exists for the given URL and this parameter is false then the function will fail.</param>
        /// <returns>True if successful, false otherwise.</returns>
        public Task<bool> AddBookmark(PinboardBookmark Bookmark, bool ReplaceExisting)
        {
            //
            // The URL and Title are required parameters so let's just go ahead and validate them locally.
            //
            if (String.IsNullOrEmpty(Bookmark.URL) || String.IsNullOrEmpty(Bookmark.Title))
            {
                return Task.FromResult<bool>(false);
            }

            RequestObject.SetRequest("posts/add");

            RequestObject.AddParameter("url", Bookmark.URL);
            RequestObject.AddParameter("description", Bookmark.Title);
            RequestObject.AddParameter("extended", Bookmark.Description);
            RequestObject.AddParameter("tags", Bookmark.TagString);
            RequestObject.AddParameter("dt", ConvertTimestamp(Bookmark.CreationTime));
            RequestObject.AddParameter("replace", BoolToString(ReplaceExisting));
            RequestObject.AddParameter("shared", BoolToString(Bookmark.Shared));
            RequestObject.AddParameter("toread", BoolToString(Bookmark.ToRead));

            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                string content = task.Result;
                Dictionary<string, object> hash = (Dictionary<string, Object>)JsonSerializer.DeserializeObject(content);

                if ((string)hash["result_code"] != AddBookmarkErrorString)
                {
                    return true;
                }

                return false;
            });
        }

        /// <summary>
        /// Gets post count by date. Up to three tags can be supplied as arguments to restrict the search.
        /// </summary>
        /// <param name="tags">A list of up to three tags that restricts what posts are considered.</param>
        /// <returns></returns>
        public Task<List<PinboardPostDate>> GetPostsByDate(List<string> tags = null)
        {
            RequestObject.SetRequest("posts/dates");

            if (tags != null)
            {
                RequestObject.AddParameter("tag", FormatTags(tags));
            }

            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                List<PinboardPostDate> PostDateList = new List<PinboardPostDate>();

                string content = task.Result;
                Dictionary<string, object> hash = (Dictionary<string, Object>)JsonSerializer.DeserializeObject(content);
                Dictionary<string, object> StringHash = hash["dates"] as Dictionary<string, object>;

                //
                // This will be null if there are no entries.
                //
                if (StringHash != null)
                {
                    foreach (KeyValuePair<string, object> kvp in StringHash)
                    {
                        PostDateList.Add(new PinboardPostDate(kvp.Key, (uint)Convert.ToInt32(kvp.Value.ToString())));
                    }
                }

                return PostDateList;
            });
        }
        #endregion

        #region Tag-Related APIs
        public Task<List<PinboardCountedTag>> GetTags()
        {
            RequestObject.SetRequest("tags/get");

            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                string content = task.Result;
                Dictionary<string, object> ObjectHash = (Dictionary<string, object>)JsonSerializer.DeserializeObject(content);

                List<PinboardCountedTag> TagList = new List<PinboardCountedTag>();

                foreach (KeyValuePair<string, object> kvp in ObjectHash)
                {
                    TagList.Add(new PinboardCountedTag(kvp.Key, (uint)Convert.ToInt32(kvp.Value.ToString())));
                }

                return TagList;
            });
        }

        /// <summary>
        /// Deletes the specified tag (all occurrences).
        /// </summary>
        /// <param name="Tag">The name of the tag to delete.</param>
        /// <returns>Returns true if the request is communicated to the server, regardless of whether the tag actually exists.</returns>
        public Task<bool> DeleteTag(string Tag)
        {
            //
            // The tag is a required parameter so let's just go ahead and validate it locally.
            //
            if (String.IsNullOrEmpty(Tag))
            {
                return Task.FromResult<bool>(false);
            }

            RequestObject.SetRequest("tags/delete");

            RequestObject.AddParameter("tag", Tag);
            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                return !String.IsNullOrEmpty(task.Result);
            });
        }

        /// <summary>
        /// Renames a given tag. Tag matching is case-insensitive.
        /// </summary>
        /// <param name="OldTagName">The old name of the tag.</param>
        /// <param name="NewTagName">The new name of the tag. If this is empty nothing will happen and we'll return success.</param>
        /// <returns>Returns true if the request is communicated to the server, regardless of whether the tag actually exists.</returns>
        public Task<bool> RenameTag(string OldTagName, string NewTagName)
        {
            //
            // The OldTagName and NewTagName are required parameters so let's just go ahead and validate them locally.
            //
            if (String.IsNullOrEmpty(OldTagName) || String.IsNullOrEmpty(NewTagName))
            {
                return Task.FromResult<bool>(false);
            }

            RequestObject.SetRequest("tags/rename");
            RequestObject.AddParameter("old", OldTagName);
            RequestObject.AddParameter("new", NewTagName);

            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                return !String.IsNullOrEmpty(task.Result);
            });
        }

        /// <summary>
        /// Gets the list of suggested tags based on a given URL. The list comprises two hashes. One is called "popular"
        /// and consists of tags that are based on pinboard-wide data. The other is called "recommended" and is culled
        /// from the user's tags.
        /// </summary>
        /// <param name="url">The URL to get suggested tags for.</param>
        /// <returns>A PinboardSuggestedTags objects with the two hashes.</returns>
        public Task<PinboardSuggestedTags> GetSuggestedTags(string url)
        {
            PinboardSuggestedTags Suggestions = new PinboardSuggestedTags();

            //
            // The URL is a required parameter so let's just go ahead and validate it locally.
            //
            if (String.IsNullOrEmpty(url))
            {
                return Task.FromResult<PinboardSuggestedTags>(Suggestions);
            }

            RequestObject.SetRequest("posts/suggest");
            RequestObject.AddParameter("url", url);
            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                string content = task.Result;
                object[] ObjectArray = (object[])JsonSerializer.DeserializeObject(content);

                foreach (Dictionary<string, Object> hash in ObjectArray)
                {
                    foreach (KeyValuePair<string, Object> kvp in hash)
                    {
                        List<string> TagList = new List<string>();

                        object[] Tags = (object[])kvp.Value;
                        foreach (string tag in Tags)
                        {
                            TagList.Add(tag);
                        }

                        if (kvp.Key == "popular")
                        {
                            Suggestions.popular = TagList.ToArray();
                        }
                        else
                        {
                            System.Diagnostics.Debug.Assert(kvp.Key == "recommended");
                            Suggestions.recommended = TagList.ToArray();
                        }
                    }
                }

                return Suggestions;
            });
        }
        #endregion

        #region User-related APIs

        /// <summary>
        /// Gets the user's secret.
        /// </summary>
        /// <returns>A string of the user's secret.</returns>
        public Task<string> GetUsersSecret()
        {
            RequestObject.SetRequest("user/secret");

            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                string content = task.Result;
                Dictionary<string, object> ObjectHash = (Dictionary<string, object>)JsonSerializer.DeserializeObject(content);

                return ObjectHash["result"].ToString();
            });
        }

        /// <summary>
        /// Gets the user's API token.
        /// </summary>
        /// <returns>A string of the user's API token.</returns>
        public Task<string> GetUsersApiToken()
        {
            RequestObject.SetRequest("user/api_token");

            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                string content = task.Result;
                Dictionary<string, object> ObjectHash = (Dictionary<string, object>)JsonSerializer.DeserializeObject(content);
                return ObjectHash["result"].ToString();
            });
        }

        #endregion

        #region Note-related APIs
        /// <summary>
        /// Gets the note with the given ID.
        /// </summary>
        /// <param name="ID">The note's ID.</param>
        /// <returns>A PinboardNote object representing the note.</returns>
        public Task<PinboardNote> GetNote(string ID)
        {
            RequestObject.SetRequest("notes/" + ID);

            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                string content = task.Result;
                // {"error":"the thing you looked for wasn't found", "error_code":"404"}
                Dictionary<string, object> ObjectHash = (Dictionary<string, object>)JsonSerializer.DeserializeObject(content);
                if (ObjectHash.ContainsKey("error"))
                {
                    return null;
                }

                return new PinboardNote((string)ObjectHash["text"], (string)ObjectHash["id"], (string)ObjectHash["hash"], (string)ObjectHash["title"], (uint)(int)ObjectHash["length"], "2014-04-07 04:54:20", "2014-04-07 04:54:20");
            });
        }

        /// <summary>
        /// Gets the full list of the user's notes.
        /// </summary>
        /// <param name="FetchText">If this is true we also fetch the note's full body/text.</param>
        /// <returns>A (possibly empty) list of the user's notes.</returns>
        public Task<List<PinboardNote>> GetNotes(bool FetchText)
        {
            RequestObject.SetRequest("notes/list");

            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                List<PinboardNote> NoteList = new List<PinboardNote>();

                string content = task.Result;
                dynamic NoteObjects = JsonSerializer.DeserializeObject(content);

                foreach (Dictionary<string, object> NoteObject in NoteObjects["notes"])
                {
                    PinboardNote Note = new PinboardNote(FetchText ? GetNoteBody((string)NoteObject["id"]).Result : "", (string)NoteObject["id"], (string)NoteObject["hash"], (string)NoteObject["title"], (uint)Convert.ToInt32((string)NoteObject["length"]), (string)NoteObject["created_at"], (string)NoteObject["updated_at"]);
                    NoteList.Add(Note);
                }

                return NoteList;
            });
        }

        /// <summary>
        /// Gets the body/text of the specified note.
        /// </summary>
        /// <param name="ID">The note's ID.</param>
        /// <returns>The body/text of the note. If an invalid ID is specified then null will be returned.</returns>
        public Task<string> GetNoteBody(string ID)
        {
            RequestObject.SetRequest(String.Format("notes/{0}", ID));

            return RequestObject.SendRequestAsync().ContinueWith((task) =>
            {
                string content = task.Result;
                if (String.IsNullOrEmpty(content))
                {
                    return null;
                }
                else
                {
                    Dictionary<string, object> ObjectHash = (Dictionary<string, object>)JsonSerializer.DeserializeObject(content);

                    return (string)ObjectHash["text"];
                }
            });
        }
        #endregion

        #region Data Conversion Routines
        internal static bool StringToBool(string str)
        {
            return str == "yes";
        }

        internal static string BoolToString(bool value)
        {
            return value ? "yes" : "no";
        }

        internal static string ConvertTimestamp(DateTime dt)
        {
            return dt.ToString("s") + "Z";
        }

        internal static string FormatTags(List<string> TagList)
        {
            return string.Join(",", TagList);
        }
        #endregion
    }
}
