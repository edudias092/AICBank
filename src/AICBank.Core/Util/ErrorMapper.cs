using System;
using System.Collections.ObjectModel;
using System.Text;
using AICBank.Core.DTOs;
using AICBank.Core.DTOs.CelCash;

namespace AICBank.Core.Util;

public class ErrorMapper
{
    public static string MapErrors(ErrorDetails errorDetails) 
    {
        if(errorDetails != null){
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(errorDetails.Message);

            if(errorDetails.Details != null)
            {
                foreach(var detail in errorDetails.Details){
                    var keyName = detail.Key?.Split('.').LastOrDefault();
                    keyName = !string.IsNullOrEmpty(keyName) && _fieldNames.ContainsKey(keyName)? _fieldNames[keyName]+": " : "";
                    foreach(var error in detail.Value){
                        sb.AppendFormat("\n{0}{1}", keyName, error);
                    }
                }
            }

            return sb.ToString();
        }

        return null;
    }

    public static ResponseDTO<object> CreateErrorResponse(params string[] messages)
    {
        var errors = new Collection<string>{};

        foreach (var msg in messages)
        {
            errors.Add(msg);
        }

        return new ResponseDTO<object>
        {
            Success = false,
            Errors = errors.ToArray()
        };
    }
    
    
    private static Dictionary<string, string> _fieldNames = new Dictionary<string, string>
    {
        {"softDescriptor", "Nome para exibição na Fatura"},
        {"internalName", "Profissão"},
        {"inscription", "Inscrição do profissional"},
        {"name", "Nome"},
        {"document", "Documento"},
        {"phone", "Telefone"},
        {"emailContact", "E-mail"},
        {"zipcode", "CEP"},
        {"street", "Rua"},
        {"number", "Número"},
        {"complement", "Complemento"},
        {"neighborhood", "Bairro"},
        {"city", "Cidade"},
        {"state", "Estado"},
        {"nameDisplay", "Nome para Exibição"},
        {"responsibleDocument", "Documento do Responsável"},
        {"typeCompany", "Tipo de Empresa"},
        {"cnae", "CNAE"},
        {"motherName", "Nome da mãe"},
        {"birthDate", "Data de nascimento"},
        {"monthlyIncome", "Renda mensal"},
        {"about", "Sobre o negócio"},
        {"socialMediaLink", "Rede social"},
        {"type", "Tipo de Associado"},
        {"lastContract", "Contrato social"},
        {"cnpjCard", "Documento CNPJ"},
        {"electionRecord", "Ata de eleição da diretoria"},
        {"statute", "Estatuto"},
        {"selfie", "Selfie"},
        {"picture", "Foto da CNH"},
        {"front", "Foto da frente do RG"},
        {"back", "Foto do verso do RG"},
        {"address", "Comprovante de Endereço"},
        {"instructions", "Instruções"}
    };
}
