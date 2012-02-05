using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using SimpleBlogger.Mvc.ViewModels;

namespace SimpleBlogger.Mvc.Controllers
{
    public partial class BlogController : Controller
    {
        public ActionResult Index()
        {
            SiteNewsModel newsModel = new SiteNewsModel("http://creou.com", HttpContext.Server.MapPath("~/Content/blog/"));
            return View(new BlogIndexViewModel(newsModel));            
        }

        public ActionResult BlogPost(String blogId)
        {
            SiteNewsModel newsModel = new SiteNewsModel("http://creou.com", HttpContext.Server.MapPath("~/Content/blog/"));
            var blogPost = newsModel.BlogPosts.Where(b => b.Id == blogId).FirstOrDefault();
           
            return View(blogPost);
            //var newsItemQuery = from n in MvcApplication.SiteNewsModel.NewsPosts
            //                    where String.Equals(n.Id, newsId, StringComparison.OrdinalIgnoreCase)
            //                    select n;

            //var newsItem = newsItemQuery.FirstOrDefault();

            //if (newsItem != null)
            //{
            //    if (values.Count > 0)
            //    {
            //        newsItem.AddComment(values["Name"], values["Email"], values["Comment"]);
            //    }

            //    return View(newsItem);
            //}
            //else
            //{
            //    return View("Index", MvcApplication.SiteNewsModel);
            //}
        }
    }
}
