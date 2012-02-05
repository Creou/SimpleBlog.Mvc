using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SimpleBlogger.Mvc.ViewModels
{
    public class BlogIndexViewModel
    {
        public BlogIndexViewModel(SiteNewsModel newsModel) 
        {
            NewsModel = newsModel;
        }

        public SiteNewsModel NewsModel { get; set; }
    }
}