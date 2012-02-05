using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.ServiceModel.Syndication;
using System.Collections.ObjectModel;
using System.IO;

namespace SimpleBlogger.Mvc
{

    public class SiteNewsModel
    {
        private List<SyndicationItem> FeedItems { get; set; }
        public IOrderedEnumerable<NewsItem> News { get; private set; }
        public IOrderedEnumerable<BlogPost> BlogPosts { get; private set; }

        public SiteNewsModel(String baseHttpUrl, String newsContentFilePath)
        {
            this.FeedItems = new List<SyndicationItem>();
            var news = new Collection<NewsItem>();
            var blogPosts = new Collection<BlogPost>();

            String[] blogDataFiles = Directory.GetFiles(newsContentFilePath, "*.html");
            
            foreach (var blogDataFile in blogDataFiles)
            {
                BlogPost blogPost = new BlogPost(baseHttpUrl, blogDataFile);

                String postAnnounementTitle;
                switch (blogPost.PrimaryZone)
                {
                    case "News":
                        postAnnounementTitle = String.Format("<a href=\"{0}\">{1}</a>", blogPost.InternalHttpPath, blogPost.Title);
                        break;

                    case "Reviews":
                        postAnnounementTitle = String.Format("New review: <a href=\"{0}\">{1}</a>", blogPost.InternalHttpPath, blogPost.Title);
                        break;

                    case "Articles":
                        postAnnounementTitle = String.Format("New article: <a href=\"{0}\">{1}</a>", blogPost.InternalHttpPath, blogPost.Title);
                        break;

                    case "Projects":
                        postAnnounementTitle = String.Format("New project post: <a href=\"{0}\">{1}</a>", blogPost.InternalHttpPath, blogPost.Title);
                        break;

                    default:
                        postAnnounementTitle = String.Format("<a href=\"{0}\">{1}</a>", blogPost.InternalHttpPath, blogPost.Title);
                        break;
                }


                String blogHtml;
                if (blogPost.PreviewAvailable)
                {
                    StringBuilder blogHtmlBuilder = new StringBuilder();
                    blogHtmlBuilder.Append(blogPost.PostPreviewHtml);
                    blogHtmlBuilder.AppendFormat("...<br/><br/><a href=\"{0}\">Read more</a><br/>", blogPost.InternalHttpPath);
                    blogHtml = blogHtmlBuilder.ToString();
                }
                else
                {
                    blogHtml = blogPost.PostHtml;
                }

                blogPosts.Add(blogPost);

                NewsItem newsItem = new NewsItem(NewsType.Blog, blogPost.UrlFriendlyTitle, blogPost.Title, postAnnounementTitle, blogHtml, blogPost.InternalHttpPath, blogPost.CreatedDate, blogPost.PublishDate);
                news.Add(newsItem);

                var newsFeedItem = new SyndicationItem(blogPost.Title, blogPost.PostExternalHtml, new Uri(blogPost.ExternalHttpPath), blogPost.UrlFriendlyTitle, blogPost.CreatedDate);
                newsFeedItem.PublishDate = newsItem.CreatedDate;

                this.FeedItems.Add(newsFeedItem);
            }

            // Order the news by creation date.
            this.News = news.Where(n => !n.PublishDate.HasValue || n.PublishDate.Value < DateTime.Now).OrderByDescending(n => n.CreatedDate);
            this.BlogPosts = blogPosts.Where(b=>!b.PublishDate.HasValue || b.PublishDate.Value < DateTime.Now).OrderByDescending(n => n.CreatedDate);
        }

        public SyndicationFeed BuildFeed()
        {
            return BuildFeed(this.FeedItems);
        }

        private static SyndicationFeed BuildFeed(List<SyndicationItem> feedItems)
        {
            // Order and filter the feed.
            IOrderedEnumerable<SyndicationItem> orderedFeedItems = feedItems.Where(f => f.PublishDate < DateTime.Now).OrderByDescending(f => f.LastUpdatedTime);
            SyndicationItem firstItem = orderedFeedItems.FirstOrDefault();
            DateTimeOffset feedLastUpdated = DateTime.Now;
            if (firstItem != null)
            {
                feedLastUpdated = firstItem.LastUpdatedTime;
            }

            // Create the feed.
            SyndicationFeed feed = new SyndicationFeed("Simon P Stevens - News", "Simon P Stevens - News", new Uri("http://www.simonpstevens.com/"), "Simon P Stevens - News", feedLastUpdated, orderedFeedItems);

            return feed;
        }
    }
}