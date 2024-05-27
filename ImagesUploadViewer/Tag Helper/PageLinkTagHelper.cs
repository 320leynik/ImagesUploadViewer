using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using ImagesUploadViewer.ViewModels;

namespace ImagesUploadViewer.Tag_Helper
{
    public class PageLinkTagHelper: TagHelper
    {
        private IUrlHelperFactory urlHelperFactory;

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }
        public PageViewModel PageModel { get; set; }
        public string PageAction { get; set; }

        public PageLinkTagHelper(IUrlHelperFactory helperFactory)
        {
            urlHelperFactory = helperFactory;
        }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContext);
            output.TagName = "div";

            // -> Набір посилань, який буде представляти список ul:
            TagBuilder tag = new TagBuilder("ul");
            tag.AddCssClass("pagination");

            // -> Створення посилання на попередню сторінку, якщо вона існує + завжди 1 сторінка:
            if (PageModel.HasPreviousPage)
            {
                if (PageModel.PageNumber - 1 != 1)
                {
                    TagBuilder firstItem = CreateTag(1, urlHelper);
                    tag.InnerHtml.AppendHtml(firstItem);
                }

                TagBuilder prevItem = CreateTag(PageModel.PageNumber - 1, urlHelper);
                tag.InnerHtml.AppendHtml(prevItem);
            }

            // -> Створення посилання на поточну сторінку:
            TagBuilder currentItem = CreateTag(PageModel.PageNumber, urlHelper);
            tag.InnerHtml.AppendHtml(currentItem);

            // -> Створення посилання на наступну сторінку, якщо вона існує + завжди остання сторінка:
            if (PageModel.HasNextPage)
            {
                TagBuilder nextItem = CreateTag(PageModel.PageNumber + 1, urlHelper);
                tag.InnerHtml.AppendHtml(nextItem);

                if(PageModel.PageNumber+1 != PageModel.TotalPages)
                {
                    TagBuilder lastItem = CreateTag(PageModel.TotalPages, urlHelper);
                    tag.InnerHtml.AppendHtml(lastItem);
                }
            }

            // -> Вставляємо список посилань у панель навігації:
            output.Content.AppendHtml(tag);

        }
        private TagBuilder CreateTag(int pageNumber, IUrlHelper urlHelper)
        {
            TagBuilder item = new("li");
            TagBuilder link = new("a");

            // ->
            if (pageNumber == this.PageModel.PageNumber)
                item.AddCssClass("active");
            else
            {
                link.Attributes["href"] = urlHelper.Action(PageAction, new { pageNumber = pageNumber });
                link.Attributes["data-ajax"] = "true";
            }
            // ->

            item.AddCssClass("page-item");
            link.AddCssClass("page-link");
            if(pageNumber == this.PageModel.TotalPages && pageNumber != this.PageModel.PageNumber)
            {
                link.InnerHtml.Append(".." + pageNumber.ToString());
            }
            else if(pageNumber == 1 && pageNumber != this.PageModel.PageNumber)
            {
                link.InnerHtml.Append(pageNumber.ToString() + "..");
            }
            else
            {
                link.InnerHtml.Append(pageNumber.ToString());
            }
            item.InnerHtml.AppendHtml(link);

            // ->
            return item;
        }
    }
}
