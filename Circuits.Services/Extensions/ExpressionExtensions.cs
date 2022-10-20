using Circuits.Shared.Extensions;
using Circuits.ViewModels.Attributes;
using Circuits.ViewModels.Entities.Equations;

namespace Circuits.Services.Extensions;

public static class ExpressionExtensions
{
    public static string GetLabel(this Expression expression)
    {
        if (expression is ExpressionValue)
        {
            return expression.Value < 0 ? $"({expression.Value})" : $"{expression.Value}";
        }
        
        if (expression is ExpressionVariable expVar)
        {
            return expVar.Label;
        }
        
        if (expression is ExpressionAdditions expAdd)
        {
            var res = "";

            for (var i = 0; i < expAdd.Nodes.Count; i ++)
            {
                if (expAdd.Nodes[i] is ExpressionAdditions or ExpressionMultipliers)
                {
                    if (i == 0)
                    {
                        res += $"({expAdd.Nodes[i].GetLabel()})";
                        continue;
                    }
                
                    res += $"{expAdd.Signs[i - 1].GetValue<string, DisplayNameAttribute>(x => x.Name)}({expAdd.Nodes[i].GetLabel()})";
                }
                else
                {
                    if (i == 0)
                    {
                        res += $"{expAdd.Nodes[i].GetLabel()}";
                        continue;
                    }
                
                    res += $"{expAdd.Signs[i - 1].GetValue<string, DisplayNameAttribute>(x => x.Name)}{expAdd.Nodes[i].GetLabel()}";
                }
            }

            return res;
        }
        
        if (expression is ExpressionMultipliers expMul)
        {
            var res = "";

            for (var i = 0; i < expMul.Nodes.Count; i ++)
            {
                if (expMul.Nodes[i] is ExpressionAdditions or ExpressionMultipliers)
                {
                    if (i == 0)
                    {
                        res += $"({expMul.Nodes[i].GetLabel()})";
                        continue;
                    }
                
                    res += $"({expMul.Multipliers[i - 1].GetValue<string, DisplayNameAttribute>(x => x.Name)}{expMul.Nodes[i].GetLabel()})";
                }
                else
                {
                    if (i == 0)
                    {
                        res += $"{expMul.Nodes[i].GetLabel()}";
                        continue;
                    }
                
                    res += $"{expMul.Multipliers[i - 1].GetValue<string, DisplayNameAttribute>(x => x.Name)}{expMul.Nodes[i].GetLabel()}";
                }
            }
            
            return res;
        }

        return string.Empty;
    }
}