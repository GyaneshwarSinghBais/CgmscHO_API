using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CgmscHO_API.HODTO
{
    public class ExcessLiftSummaryDTO
    {
        //[Column("facilityname")]
        //[JsonPropertyName("facilityname")]
        //public string? Facilityname { get; set; }

        //[Column("category_label")]
        //[JsonPropertyName("category_label")]
        //public string? Category_label { get; set; }

        //[Column("Count of Code")]
        //[JsonPropertyName("Count of Code")]
        //public int? Count_of_Code { get; set; }

        //[Column("Sum of Excess Lifted Value in Rs")]
        //[JsonPropertyName("Sum of Excess Lifted Value in Rs")]
        //public int? Sum_of_Excess_Lifted_Value_in_Rs { get; set; }

        public long? facilityid { get; set; }
        public string? facilityname { get; set; }
        public string? category_label { get; set; }

        [Column("Count of Code")]
        public int? Count_of_Code { get; set; }

        [Column("Sum of Excess Lifted Value in Rs")]
        public decimal? Sum_of_Excess_Lifted_Value_in_Rs { get; set; }
    }
}
