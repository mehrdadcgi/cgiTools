using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppBulckMatch
{
    public static class NormalStringForES
    {

        public static string getInversAl(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return "";
            }

            string AferAl = "";
            string BeforeAl = "";
            bool isMatch = NormalStringForES.SplitOnAl(name, out AferAl, out BeforeAl);
            string inv = AferAl + " " + BeforeAl;
            return inv;
        }

        public static bool SplitOnAl(
    string input,
    out string afterAl,
    out string beforeAl)
        {
            afterAl = null;
            beforeAl = null;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            // Normalize spacing but keep original content
            string text = input.Trim();

            // Case-insensitive search for " AL "
            int index = text
                .IndexOf(" AL ", StringComparison.OrdinalIgnoreCase);

            if (index < 0)
                return false;

            beforeAl = text.Substring(0, index).Trim();
            afterAl = text.Substring(index + 4).Trim(); // 4 = length of " AL "

            return true;
        }

        public static string NormalizeOrgName(string orgName)
        {
            if (string.IsNullOrWhiteSpace(orgName))
                return orgName;

            string result = orgName.Trim();

            // 1️⃣ Remove anything after the 5th space
            int spaceCount = 0;
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] == ' ')
                {
                    spaceCount++;
                    if (spaceCount == 5)
                    {
                        result = result.Substring(0, i);
                        break;
                    }
                }
            }

            // 2️⃣ Remove anything after the 2nd comma
            int commaCount = 0;
            for (int i = 0; i < result.Length; i++)
            {
                if (result[i] == ',')
                {
                    commaCount++;
                    if (commaCount == 2)
                    {
                        result = result.Substring(0, i);
                        break;
                    }
                }
            }

            // 3️⃣ If length > 25
            if (result.Length > 50)
            {
                // Cut to 25 chars
                result = result.Substring(0, 50);

                // Remove partial word (cut to last space)
                int lastSpace = result.LastIndexOf(' ');
                if (lastSpace > 0)
                    result = result.Substring(0, lastSpace);
            }

            return result.Trim();
        }

    }
}
