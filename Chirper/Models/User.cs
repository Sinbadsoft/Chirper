namespace JavaGeneration.Chirper.Models
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    public class User
    {
        // TODO: this is not the best place to put this, at some point we should have a view model with User and this data
        public IEnumerable<SelectListItem> Genders { get; set; }

        public string Gender { get; set; }

        public string Name { get; set; }

        public string PasswordHash { get; set; }

        public string DisplayName { get; set; }

        public string Location { get; set; }

        public string Web { get; set; }

        public string Bio { get; set; }

        public string Email { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}