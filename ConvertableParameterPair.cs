namespace Martin_App
{
    public class ConvertableParameterPair
    {
        public string AmazonParameterName { get; set; }

        public string PohodaParameterName { get; set; }

        public ConvertableParameterPair(string amazonName, string pohodaName)
        {
            this.AmazonParameterName = amazonName;
            this.PohodaParameterName = pohodaName;
        }
    }
}