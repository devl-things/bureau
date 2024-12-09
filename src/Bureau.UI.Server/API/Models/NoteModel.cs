namespace Bureau.UI.API.Models
{
    public class NoteModel
    {
        public string Id { get; set; }
        public List<TagModel> Tags { get; set; }

        public string Note { get; set; }
    }
}
