﻿using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Raven.Migrations.Sample.Pages
{
    public class ContactModel : PageModel
    {
        public string Message { get; set; }

        public void OnGet()
        {
            Message = "Your contact page.";
        }
    }
}