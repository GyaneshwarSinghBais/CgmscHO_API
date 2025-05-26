using System.ComponentModel.DataAnnotations;
namespace CgmscHO_API.DistrictDTOs
{
    public class DistFacwiseCGMSCSupplyIndentDTO
    {
      
        [Key]
        public Int64? facilityid { get; set; }
        public string? facilityname { get; set; }
        
        public Int64? nosindent { get; set; }
        public Int64? avgdaystaken { get; set; }
        
        public Int64? drugscount { get; set; }
        public Int64? consumables { get; set; }

        public Int64? reagent { get; set; }
        public Int64? ayushdrugs { get; set; }
       
    }
}
