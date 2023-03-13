namespace SQR.Utilities;

public static class StringUtilities
{
    public static string ProgressBar(char fill, char rest, double fillPercentage, int maxCharAmount = 10)
    {
        if (fillPercentage is < 0 or > 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(fillPercentage), 
                fillPercentage,
                $"Argument {nameof(fillPercentage)} should be greater or equals 0 and less or equals 1"
                );
        }
        return new string(fill, (int)(fillPercentage * maxCharAmount)) + 
               new string(rest, (int)((1 - fillPercentage) * maxCharAmount));
    }
}