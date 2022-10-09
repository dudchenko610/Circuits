namespace Circuits.Shared.Extensions;

public static class LinqExtensions
{
    public static void EliminateGroupedCopies<T>(this IList<T> list) where T : class
    {
        for (int i = 0; i < list.Count; i++)
        {
            for (int j = i + 1; j < list.Count; j++)
            {
                if (list[j] == list[i])
                {
                    list.RemoveAt(j);
                    j--;
                }
                else
                {
                    break;
                }
            }
        }
    }
}