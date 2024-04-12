namespace LiveOn.Game
{
    public class ScriptItem
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public string ScriptCode { get; set; }

        public List<ScriptItem> Items { get; set;}
    }
}
