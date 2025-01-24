namespace Bureau.UI.API.Features.Notes
{
    public class TextNoteModel
    {
        public string Id { get; set; }
        public List<TagModel> Tags { get; set; }

        public string Note { get; set; }
    }
}
