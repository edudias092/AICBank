using System.Text.Json.Serialization;

namespace AICBank.Core.DTOs.CelCash
{
    public class CelcashSendMandatoryDocumentsDTO
    {
        [JsonPropertyName("Fields")]
        public FieldsDTO Fields { get; set; }

        [JsonPropertyName("Documents")]
        public DocumentsDTO Documents { get; set; }
        [JsonPropertyName("Associate")]
        public AssociateDTO[] Associate { get; set; }

        public static CelcashSendMandatoryDocumentsDTO FromMandatoryDocumentsDto(
            MandatoryDocumentsDTO mandatoryDocumentsDto)
        {
            var dto = new CelcashSendMandatoryDocumentsDTO
            {
                Fields = new FieldsDTO
                {
                    About = mandatoryDocumentsDto.About,
                    BirthDate = mandatoryDocumentsDto.BirthDate,
                    MonthlyIncome = mandatoryDocumentsDto.MonthlyIncome,
                    MotherName = mandatoryDocumentsDto.MotherName,
                    SocialMediaLink = mandatoryDocumentsDto.SocialMediaLink
                },
                Documents = new DocumentsDTO
                {
                    Personal = new PersonalDocumentsDTO(),
                    Company = new CompanyDocumentsDTO()
                },
                Associate = new AssociateDTO[]
                {
                    new AssociateDTO{
                        MotherName = mandatoryDocumentsDto.MotherName,
                        BirthDate = mandatoryDocumentsDto.BirthDate
                    }
                }
                
            };

            if (mandatoryDocumentsDto.AssociateDocument != null)
            {
                dto.Associate[0].Document = mandatoryDocumentsDto.AssociateDocument;
            }
            if (mandatoryDocumentsDto.AssociateName != null)
            {
                dto.Associate[0].Name = mandatoryDocumentsDto.AssociateName;
            }
            if (mandatoryDocumentsDto.AssociateType != null)
            {
                dto.Associate[0].Type = mandatoryDocumentsDto.AssociateType;
            }

            return dto;
        }
    }

    public class AssociateDTO
    {
        public string Document { get; set; }
        public string Name { get; set; }
        public string MotherName { get; set; }
        public string Type { get; set; }
        [JsonIgnore]
        public DateTime BirthDate { get; set; }
        [JsonPropertyName("birthDate")]
        public string BirthDateString { get => BirthDate.ToString("yyyy-MM-dd");}
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
        
        [JsonPropertyName("Company")]
        public CompanyDocumentsDTO Company { get; set; }
    }

    public class CompanyDocumentsDTO
    {
        public string LastContract { get; set; }
        public string CnpjCard { get; set; }
        public string ElectionRecord { get; set; }
        public string Statute { get; set; }
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