namespace Bindicator.ViewModels
{
    public class EditLocationViewModel
    {
        public string Postcode { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public int BinNumber { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
