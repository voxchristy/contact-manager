namespace ContactManager.API
{
    public class DomainContactModel : Contacts
    {
        public DateTime CreatedDate { get; set; }
        public DateTime LastChangeDate { get; set; }
        public string? HasBirthdaySoon { get; set; }
        
    }
}
