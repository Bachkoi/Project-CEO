using System.Collections.Generic;

public class RandomNameGenerator
{
    private static readonly System.Random random = new System.Random();

    private static readonly List<string> lastNames = new List<string>
    {
        "Smith", "Johnson", "Williams", "Brown", "Jones",
        "Miller", "Davis", "Garcia", "Rodriguez", "Wilson"
    };

    private static readonly List<string> maleFirstNames = new List<string>
    {
        "James", "John", "Robert", "Michael", "William",
        "David", "Richard", "Joseph", "Thomas", "Daniel"
    };

    private static readonly List<string> femaleFirstNames = new List<string>
    {
        "Mary", "Patricia", "Jennifer", "Linda", "Elizabeth",
        "Barbara", "Susan", "Jessica", "Sarah", "Karen"
    };

    public static string GenerateRandomName(int gender = 0)
    {
        string lastName = lastNames[random.Next(lastNames.Count)];

        string firstName;
        int actualGender = gender;

        if (actualGender == 0)
        {
            actualGender = random.Next(1, 3); // 1 or 2
        }

        if (actualGender == 1)
        {
            firstName = maleFirstNames[random.Next(maleFirstNames.Count)];
        }
        else
        {
            firstName = femaleFirstNames[random.Next(femaleFirstNames.Count)];
        }

        return $"{firstName} {lastName}";
    }
}