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

            if(errorDetails.Details != null){
                foreach(var detail in errorDetails.Details){
                    foreach(var error in detail.Value){
                        sb.AppendLine(error);
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
}
