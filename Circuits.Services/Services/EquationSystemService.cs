using Circuits.Services.Services.Interfaces;
using Circuits.ViewModels.Entities.Equations;

namespace Circuits.Services.Services;

public class EquationSystemService : IEquationSystemService
{
    public string PerformKirchhoffElimination(EquationSystem equationSystem)
    {
        var matrixLength = equationSystem.Matrix.Length;
        var mat = equationSystem.Matrix;

        for (var k = 0; k < matrixLength; k++)
        {
            // Initialize maximum value and index for pivot
            var iMax = k;
            var vMax = mat[iMax][k] is ExpressionValue expVal ? Math.Abs(expVal.Value) : 0;

            /* find greater amplitude for pivot if any */
            for (var i = k + 1; i < matrixLength; i++)
            {
                if (mat[i][k] is ExpressionValue exVal && Math.Abs(exVal.Value) > vMax)
                {
                    vMax = Math.Abs(exVal.Value);
                    iMax = i;
                }

                /* If a principal diagonal element  is zero,
                *  it denotes that matrix is singular, and
                *  will lead to a division-by-zero later. */
                if (mat[iMax][k] is ExpressionValue { Value: 0 })
                {
                    return mat[k][matrixLength] is ExpressionValue { Value: 0 }
                        ? "May have infinitely many solutions"
                        : "Inconsistent System";
                }
            }

            /* Swap the greatest value row with
               current row
            */
            if (iMax != k)
            {
                (mat[k], mat[iMax]) = (mat[iMax], mat[k]);
            }

            for (var i = k + 1; i < matrixLength; i++)
            {
                /* factor f to set current row kth element
                *  to 0, and subsequently remaining kth
                *  column to 0 */
                var f = mat[i][k] / mat[k][k];

                /* subtract fth multiple of corresponding
                   kth row element*/
                for (var j = k + 1; j <= matrixLength; j++)
                {
                    mat[i][j] -= mat[k][j] * f;
                }

                /* filling lower triangular matrix with
                *  zeros*/
                mat[i][k] = new ExpressionValue(0);
            }
        }

        return "Success";
    }
}