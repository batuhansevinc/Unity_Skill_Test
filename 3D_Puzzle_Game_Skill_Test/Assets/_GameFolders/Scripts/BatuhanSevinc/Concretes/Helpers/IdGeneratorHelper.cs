using System.Linq;
using System.Text;

namespace Assignment01.Helpers
{
    public static class IdGeneratorHelper
    {
        public static string GenerateID()
        {
            StringBuilder builder = new StringBuilder();
            Enumerable
                .Range(65, 26)
                .Select(e => ((char)e).ToString())
                .Concat(Enumerable.Range(97, 26).Select(e => ((char)e).ToString()))
                .Concat(Enumerable.Range(0, 10).Select(e => e.ToString()))
                .OrderBy(e => System.Guid.NewGuid())
                .Take(11)
                .ToList().ForEach(e => builder.Append(e));
            return builder.ToString();
        }
    }
}