using System;
using System.Globalization;
using System.Text;

namespace AICBank.Core.Util.Extensions;

public static class StringExtensions
{
    
    public static string RemoverAcentos(this string texto)
    {
        if (string.IsNullOrEmpty(texto))
        {
            return texto;
        }

        // Normaliza a string em forma de decomposição (combinando caracteres)
        string textoNormalizado = texto.Normalize(NormalizationForm.FormD);

        // Cria um StringBuilder para armazenar o resultado
        StringBuilder sb = new StringBuilder();

        // Percorre cada caractere da string
        foreach (char c in textoNormalizado)
        {
            // Obtém a categoria do caractere
            UnicodeCategory categoria = CharUnicodeInfo.GetUnicodeCategory(c);

            // Mantém apenas letras e números, ignorando os diacríticos (acentos)
            if (categoria != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        // Retorna a string sem acentos, normalizando novamente
        return sb.ToString().Normalize(NormalizationForm.FormC)
                .Replace("ç", "c")
                .Replace("Ç", "C");
    }
}
