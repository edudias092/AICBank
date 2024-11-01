using System;
using System.Text;
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
}
