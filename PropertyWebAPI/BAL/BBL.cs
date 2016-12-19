using System.Text.RegularExpressions;

namespace PropertyWebAPI.BAL
{

    public static class BBL
    {
        public static string GetBoroughName(string BBL)
        {
            switch (BBL.Substring(0, 1))
            {
                case "1":
                    return "Manhattan";
                case "2":
                    return "Bronx";
                case "3":
                    return "Brooklyn";
                case "4":
                    return "Queens";
                case "5":
                    return "Staten Island";
            }
            return "";
        }

        public static int GetBlock(string BBL)
        {
            return int.Parse(BBL.Substring(1, 5));
        }

        public static int GetLot(string BBL)
        {
            return int.Parse(BBL.Substring(6, 4));
        }

        public static bool IsValid(string BBL)
        {
            return Regex.IsMatch(BBL, "^[1-5][0-9]{9}[A-Z]??$");
        }

        public static bool IsRegularTaxLot(string BBL)
        {
            if (GetLot(BBL) < 1000)
                return true;
            return false;
        }

        public static bool IsCondoTaxLot(string BBL)
        {
            if (GetLot(BBL) >= 1001 && GetLot(BBL) <= 6999)
                return true;
            return false;
        }

        public static bool IsSpecialCondoBuildingTaxLot(string BBL)
        {
            if (GetLot(BBL) >= 7501 && GetLot(BBL) <= 7599)
                return true;
            return false;
        }
    }
}