using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace SimpleBlogger.Mvc
{
    public enum NewsType
    {
        Blog = 1
    }

    public class NewsItem
    {
        public String Id { get; set; }
        public String TitleLink { get; private set; }
        public String Name { get; private set; }
        public String NewsData { get; private set; }
        public String NewsData_jsEnhanced { get; private set; }
        public String PermaLink { get; private set; }
        public DateTime CreatedDate { get; private set; }
        public DateTime? PublishDate { get; set; }
        public NewsType NewsType { get; private set; }

        public bool Display
        {
            get
            {
#if DEBUG
                return true;
#else 
                return CreatedDate < DateTime.Now;
#endif
            }
            private set { return; }
        }

        public NewsItem(NewsType type, String id, String name, String titleLink, String newsData, String permaLink, DateTime createdDate, DateTime? publishDate)
            : this(type, id, name, titleLink, newsData, newsData, permaLink, createdDate, publishDate)
        {
        }

        public NewsItem(NewsType type, String id, String name, String titleLink, String newsData_jsEnhanced, String newsData, String permaLink, DateTime createdDate, DateTime? publishDate)
        {
            this.NewsType = type;
            this.Id = id;
            this.Name = name;
            this.TitleLink = titleLink;
            this.NewsData = newsData;
            this.NewsData_jsEnhanced = newsData_jsEnhanced;
            this.PermaLink = permaLink;
            this.CreatedDate = createdDate;
            this.PublishDate = publishDate;
        }
    }

    public interface IdentifiableObject
    {
        string Id { get; set; }
    }

    //public abstract class CommentablePost : IdentifiableObject
    //{
    //    public string Id { get; set; }

    //    public ICollection<Comment> Comments { get; private set; }

    //    public void AddComment(string name, string email, string comment)
    //    {
    //        Comment newComment = Comment.CreateAndSaveNewComment(this.Id, name, email, comment);

    //        this.Comments.Add(newComment);
    //    }

    //    protected void LoadComments()
    //    {
    //        using (spsdataEntities commentData = new spsdataEntities())
    //        {
    //            var queryComments = from comment in commentData.Comments
    //                                where comment.ParentPost == this.Id
    //                                orderby comment.PostedDate
    //                                select comment;

    //            this.Comments = queryComments.ToList();
    //        }
    //    }

    //}

    public class BlogPost : IdentifiableObject
    {
        public string Id { get; set; }

        private string _baseHttpUrl;
        private string _newsDataFile;

        public BlogPost(string baseHttpUrl, string newsDataFile)
        {
            _baseHttpUrl = baseHttpUrl;
            _newsDataFile = newsDataFile;

            ProcessNewsFile(baseHttpUrl, newsDataFile);
        }

        public bool Display
        {
            get
            {
#if DEBUG
                return true;
#else 
                return CreatedDate < DateTime.Now;
#endif
            }
            private set { return; }
        }

        public string PostHtml { get; set; }

        public string PostPreviewHtml { get; set; }

        public string PostExternalHtml { get; set; }

        public string PostPreviewExternalHtml { get; set; }

        public string UrlFriendlyTitle { get; set; }

        public ICollection<String> Tags { get; private set; }
        public ICollection<String> Zones { get; private set; }
        public string PrimaryZone { get; private set; }
        public ICollection<String> Projects { get; private set; }
        public ICollection<String> NamedFeeds { get; private set; }
        public String Description { get; private set; }

        public bool IsReview
        {
            get
            {
                return Zones.Contains("Reviews");
            }
        }

        public bool IsArticle
        {
            get
            {
                return Zones.Contains("Articles");
            }
        }

        public bool IsProjectPost
        {
            get
            {
                return Zones.Contains("Projects");
            }
        }

        public bool IsInProject(String projectName)
        {
            return IsProjectPost && Projects.Contains(projectName);
        }

        public string Title { get; set; }

        public string InternalHttpPath { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? PublishDate { get; set; }

        public string ExternalHttpPath { get; set; }

        public bool PreviewAvailable { get; set; }

        private void ProcessNewsFile(String baseHttpUrl, string newsDataFile)
        {
            String postRawHtml = File.ReadAllText(newsDataFile);

            ExtractMetaData(postRawHtml, baseHttpUrl);

            this.InternalHttpPath = String.Format("/{0}/{1}", this.PrimaryZone, this.UrlFriendlyTitle);
            this.ExternalHttpPath = String.Format("{0}/{1}/{2}", baseHttpUrl, this.PrimaryZone, this.UrlFriendlyTitle);

            int contentStartIndex = postRawHtml.IndexOf("<body>") + 6;
            int contentEndIndex = postRawHtml.IndexOf("</body>");

            String postHtmlUntranslated = postRawHtml.Substring(contentStartIndex, contentEndIndex - contentStartIndex);

            this.PostHtml = postHtmlUntranslated.Replace(String.Format("{0}_files", this.UrlFriendlyTitle), String.Format("/content/blog/{0}_files", this.UrlFriendlyTitle));
            this.PostExternalHtml = postHtmlUntranslated.Replace(String.Format("{0}_files", this.UrlFriendlyTitle), String.Format("{0}/content/blog/{1}_files", baseHttpUrl, this.UrlFriendlyTitle));

            ProcessHtmlForPreviews();
        }

        private void ProcessHtmlForPreviews()
        {
            int previewIndexEnd = this.PostHtml.IndexOf("<p class=\"PreviewEnd\" />");
            if (previewIndexEnd > 0)
            {
                this.PostPreviewHtml = this.PostHtml.Substring(0, previewIndexEnd);
                this.PostHtml = this.PostHtml.Replace("<p class=\"PreviewEnd\" />", String.Empty);
                this.PreviewAvailable = true;
            }
            else
            {
                this.PostPreviewHtml = this.PostHtml;
            }

            int previewExternalIndexEnd = this.PostExternalHtml.IndexOf("<p class=\"PreviewEnd\" />");
            if (previewExternalIndexEnd > 0)
            {
                this.PostPreviewExternalHtml = this.PostHtml.Substring(0, previewExternalIndexEnd);
                this.PostExternalHtml = this.PostExternalHtml.Replace("<p class=\"PreviewEnd\" />", String.Empty);
            }
            else
            {
                this.PostPreviewExternalHtml = this.PostHtml;
            }

        }

        private void ExtractMetaData(String postRawHtml, String baseHttpUrl)
        {
            int headStartIndex = postRawHtml.IndexOf("<head>") + 6;
            int headEndIndex = postRawHtml.IndexOf("</head>");
            String header = postRawHtml.Substring(headStartIndex, headEndIndex - headStartIndex);

            Regex titleRegEx = new Regex(@"\<title\>(?'title'.*)\<\/title\>");
            Match titleMatch = titleRegEx.Match(header);
            this.Title = titleMatch.Groups["title"].Value;

            Regex dateRegEx = new Regex(@"\<meta name=""Date"" content=""(?'date'.*)"" \/\>");
            Match dateMatch = dateRegEx.Match(header);
            this.CreatedDate = DateTime.Parse(dateMatch.Groups["date"].Value);

            Regex publishDateRegEx = new Regex(@"\<meta name=""PublishDate"" content=""(?'date'.*)"" \/\>");
            Match publicDateMatch = publishDateRegEx.Match(header);
            if (publicDateMatch.Success)
            {
                this.PublishDate = DateTime.Parse(publicDateMatch.Groups["date"].Value);
            }

            Regex descriptionRegEx = new Regex(@"\<meta name=""Description"" content=""(?'description'.*)"" \/\>");
            Match descriptionMatch = descriptionRegEx.Match(header);
            this.Description = descriptionMatch.Groups["description"].Value;

            Regex idRegEx = new Regex(@"\<meta name=""Id"" content=""(?'id'.*)"" \/\>");
            Match idMatch = idRegEx.Match(header);
            this.UrlFriendlyTitle = idMatch.Groups["id"].Value;
            this.Id = this.UrlFriendlyTitle;

            Regex previewImageRegEx = new Regex(@"\<meta name=""PreviewImage"" content=""(?'previewImage'.*)"" \/\>");
            Match previewImageMatch = previewImageRegEx.Match(header);
            String previewImage = previewImageMatch.Groups["previewImage"].Value;

            this.PreviewImageInternalPath = previewImage.Replace(String.Format("{0}_files", this.UrlFriendlyTitle), String.Format("/content/blog/{0}_files", this.UrlFriendlyTitle));
            this.PreviewImageExternalPath = previewImage.Replace(String.Format("{0}_files", this.UrlFriendlyTitle), String.Format("{0}/content/blog/{1}_files", baseHttpUrl, this.UrlFriendlyTitle));

            Regex tagsRegEx = new Regex(@"\<meta name=""Tags"" content=""(?'tags'.*)"" \/\>");
            Match tagsMatch = tagsRegEx.Match(header);
            this.Tags = tagsMatch.Groups["tags"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            Regex namedFeedsRegEx = new Regex(@"\<meta name=""NamedFeeds"" content=""(?'namedFeeds'.*)"" \/\>");
            Match namedFeedsMatch = namedFeedsRegEx.Match(header);
            this.NamedFeeds = namedFeedsMatch.Groups["namedFeeds"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            Regex zoneRegEx = new Regex(@"\<meta name=""Zones"" content=""(?'zones'.*)"" \/\>");
            Match zoneMatch = zoneRegEx.Match(header);
            String[] zones = zoneMatch.Groups["zones"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            this.Zones = zones.ToList();
            this.PrimaryZone = this.Zones.First();

            Regex projectsRegEx = new Regex(@"\<meta name=""Projects"" content=""(?'projects'.*)"" \/\>");
            Match projectsMatch = projectsRegEx.Match(header);
            this.Projects = projectsMatch.Groups["projects"].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }

        public bool PreviewImageAvailable
        {
            get { return !String.IsNullOrEmpty(PreviewImageInternalPath); }
        }
        public string PreviewImageInternalPath { get; set; }

        public string PreviewImageExternalPath { get; set; }
    }
}
