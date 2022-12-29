using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace Muffin.Common.Geolocation
{
    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Coordinates
    /// </summary>
    public interface ICoordinates
    {
        /// <summary>
        /// Returns the position's latitude in decimal degrees
        /// </summary>
        [Display(Name = "Längengrad")]
        [JsonProperty(PropertyName = "latitude")]
        double Latitude { get; set; }

        /// <summary>
        /// Returns the position's longitude in decimal degrees
        /// </summary>
        [Display(Name = "Breitengrad")]
        [JsonProperty(PropertyName = "longitude")]
        double Longitude { get; set; }

        /// <summary>
        /// Returns the accuracy of the latitude and longitude properties in meters
        /// </summary>
        [Display(Name = "Genauigkeit")]
        [JsonProperty(PropertyName = "accuracy")]
        double Accuracy { get; set; }

        /// <summary>
        /// Returns the position's altitude in meters, relative to sea level
        /// </summary>
        [Display(Name = "Altitude")]
        [JsonProperty(PropertyName = "altitude")]
        double? Altitude { get; set; }

        /// <summary>
        /// Returns the accuracy of the altitude property in meters
        /// </summary>
        [Display(Name = "Genauigkeit Altitude")]
        [JsonProperty(PropertyName = "altitudeAccuracy")]
        double? AltitudeAccuracy { get; set; }

        /// <summary>
        /// Returns the direction in which the device is traveling. This value, specified in degrees, indicates how far off from heading true north the device is. 0 degrees represents true north, and the direction is determined clockwise (east is 90 degrees and west is 270 degrees). If speed is 0, heading is NaN. If the device is unable to provide heading information, this value is null
        /// </summary>
        [Display(Name = "Blickrichtung")]
        [JsonProperty(PropertyName = "heading")]
        double? Heading { get; set; }

        /// <summary>
        /// Returns the velocity of the device in meters per second. This value can be null
        /// </summary>
        [Display(Name = "Geschwindigkeit")]
        [JsonProperty(PropertyName = "speed")]
        double? Speed { get; set; }
    }

    /// <summary>
    /// https://developer.mozilla.org/en-US/docs/Web/API/Coordinates
    /// </summary>
    public class Coordinates : ICoordinates
    {
        /// <summary>
        /// Returns the position's latitude in decimal degrees
        /// </summary>
        [Display(Name = "Längengrad")]
        [JsonProperty(PropertyName = "latitude")]
        public double Latitude { get; set; }

        /// <summary>
        /// Returns the position's longitude in decimal degrees
        /// </summary>
        [Display(Name = "Breitengrad")]
        [JsonProperty(PropertyName = "longitude")]
        public double Longitude { get; set; }

        /// <summary>
        /// Returns the accuracy of the latitude and longitude properties in meters
        /// </summary>
        [Display(Name = "Genauigkeit")]
        [JsonProperty(PropertyName = "accuracy")]
        public double Accuracy { get; set; }

        /// <summary>
        /// Returns the position's altitude in meters, relative to sea level
        /// </summary>
        [Display(Name = "Altitude")]
        [JsonProperty(PropertyName = "altitude")]
        public double? Altitude { get; set; }

        /// <summary>
        /// Returns the accuracy of the altitude property in meters
        /// </summary>
        [Display(Name = "Genauigkeit Altitude")]
        [JsonProperty(PropertyName = "altitudeAccuracy")]
        public double? AltitudeAccuracy { get; set; }

        /// <summary>
        /// Returns the direction in which the device is traveling. This value, specified in degrees, indicates how far off from heading true north the device is. 0 degrees represents true north, and the direction is determined clockwise (east is 90 degrees and west is 270 degrees). If speed is 0, heading is NaN. If the device is unable to provide heading information, this value is null
        /// </summary>
        [Display(Name = "Blickrichtung")]
        [JsonProperty(PropertyName = "heading")]
        public double? Heading { get; set; }

        /// <summary>
        /// Returns the velocity of the device in meters per second. This value can be null
        /// </summary>
        [Display(Name = "Geschwindigkeit")]
        [JsonProperty(PropertyName = "speed")]
        public double? Speed { get; set; }
    }
}