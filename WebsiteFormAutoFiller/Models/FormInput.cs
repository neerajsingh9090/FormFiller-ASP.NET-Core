namespace WebsiteFormAutoFiller.Models
{
    public class FormInput
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Message { get; set; }
        public string Company { get; set; }           
        public string Subject { get; set; }           
        public IFormFile UrlFile { get; set; }
        public List<CustomField> ExtraFields { get; set; } = new(); // <-- Added

    }
}
