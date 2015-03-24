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
    public class PinboardPostDate
    {
        public DateTime Date { get; private set; }
        public uint Count { get; private set; }

        public PinboardPostDate(string date, uint count)
        {
            this.Date = DateTime.Parse(date);
            this.Count = count;
        }
    }

    public class PinboardCountedTag
    {
        public string tag { get; private set; }
        public uint count { get; private set; }

        public PinboardCountedTag(string tag, uint count)
        {
            this.tag = tag;
            this.count = count;
        }
    }

    public class PinboardSuggestedTags
    {
        public string[] popular;
        public string[] recommended;

        public PinboardSuggestedTags()
        {
            popular = new string[0];
            recommended = new string[0];
        }
    }

    public class PinboardBookmark
    {
        private readonly List<string> TagList;
        public string URL { get; private set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime CreationTime { get; set; }
        public bool Shared { get; set; }
        public bool ToRead { get; set; }

        public PinboardBookmark(string url, string title, string description = "")
        {
            this.URL = url;
            this.Title = title;
            this.Description = description;

            Shared = true;
            ToRead = false;

            CreationTime = DateTime.UtcNow;
            TagList = new List<string>();
        }

        public void SetTags(string TagListString)
        {
            TagList.Clear();
            TagList.AddRange(TagListString.Split(' ').OrderBy(a => a));
        }

        public void AddTag(string tag)
        {
            TagList.Add(tag);
            TagList.Sort();
        }

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

        public string TagString
        {
            get
            {
                return PinboardManager.FormatTags(TagList);
            }
        }
        public string[] Tags
        {
            get
            {
                return TagList.ToArray();
            }
        }

        public override string ToString()
        {
            return String.Format("URL: {0}, Title: '{1}', Description:'{2}'", URL, Title, Description);
        }
    }

    public class PinboardNote
    {
        public DateTime CreatedDate { get; private set; }
        public DateTime UpdatedDate { get; private set; }

        public string Text { get; private set; }
        public string ID { get; private set; }
        public string Hash { get; private set; }
        public string Title { get; private set; }
        public uint Length { get; private set; }

        public PinboardNote()
        {
        }

        public PinboardNote(string Text, string ID, string Hash, string Title, uint Length, string CreatedTimestamp, string UpdatedTimestamp)
        {
            this.Text = Text;
            this.ID = ID;
            this.Hash = Hash;
            this.Title = Title;
            this.Length = Length;
            this.CreatedDate = DateTime.Parse(CreatedTimestamp);
            this.UpdatedDate = DateTime.Parse(UpdatedTimestamp);
        }

        public override string ToString()
        {
            return String.Format("ID: {0}, Title: '{1}', Created At: {2}\nBody: {3}", ID, Title, CreatedDate.ToString(), Text);
        }
    }
}