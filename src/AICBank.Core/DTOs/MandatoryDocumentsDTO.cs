using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AICBank.Core.DTOs
{
    public class MandatoryDocumentsDTO
    {
        public string MotherName { get; set; }
        public DateTime BirthDate { get; set; }
        public int MonthlyIncome { get; set; }
        public string About { get; set; }
        public string SocialMediaLink { get; set; }
        public DocumentType Type { get; set; }
        public IFormFile Selfie{ get; set; }
        public IFormFile Front { get; set; }
        public IFormFile Back { get; set; }
        public IFormFile Address { get; set; }

        public enum DocumentType
        {
            RG = 1, CNH
        }
    }
}