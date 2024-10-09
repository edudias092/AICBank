using System.Text.Json.Serialization;

namespace AICBank.Core.DTOs.CelCash
{
    public class CelcashSendMandatoryDocumentsDTO
    {
        [JsonPropertyName("Fields")]
        public FieldsDTO Fields { get; set; }

        [JsonPropertyName("Documents")]
        public DocumentsDTO Documents { get; set; }
    }

    public class FieldsDTO 
    {
        public string MotherName { get; set; }
        
        [JsonIgnore]
        public DateTime BirthDate { get; set; }

        [JsonPropertyName("birthDate")]
        public string BirthDateString { get => BirthDate.ToString("yyyy-MM-dd");}
        public int MonthlyIncome { get; set; }
        public string About { get; set; }
        public string SocialMediaLink { get; set; }
    }

    public class DocumentsDTO
    {
        [JsonPropertyName("Personal")]
        public PersonalDocumentsDTO Personal { get; set; }    
    }

    public class PersonalDocumentsDTO 
    {
        [JsonPropertyName("CNH")]
        public CNHDTO CNH { get; set; }

        [JsonPropertyName("RG")]
        public RGDTO RG { get; set; }
    }

    public class CNHDTO 
    {
        public string Selfie{ get; set; }
        public string[] Picture { get; set; }
        public string Address { get; set; }
    }

    public class RGDTO 
    {
        public string Selfie{ get; set; }
        public string Front { get; set; }
        public string Back { get; set; }
        public string Address { get; set; }
    }
}