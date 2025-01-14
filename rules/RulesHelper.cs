using System;
using System.Collections.Generic;
using System.Linq;

// Abstract base class with input type T and return type R
public abstract class Rule<T, R>
{
    public string Name { get; set; }
    public Func<T, R> EvaluationFunction { get; set; }

    protected Rule(string name, Func<T, R> evaluationFunction)
    {
        Name = name;
        EvaluationFunction = evaluationFunction;
    }

    public R Evaluate(T value) => EvaluationFunction(value);
}

// RulesManager to manage rules
public class RulesManager
{
    private readonly Dictionary<string, object> _rules = new();

    public void AddRule<T, R>(Rule<T, R> rule)
    {
        if (_rules.ContainsKey(rule.Name))
            throw new InvalidOperationException($"Rule '{rule.Name}' already exists");

        _rules[rule.Name] = rule;
    }

    public R Evaluate<T, R>(string ruleName, T value)
    {
        if (!_rules.TryGetValue(ruleName, out var ruleObj))
            throw new InvalidOperationException($"Rule '{ruleName}' not found");

        if (ruleObj is not Rule<T, R> rule)
            throw new InvalidOperationException($"Rule '{ruleName}' expects a different type");

        return rule.Evaluate(value);
    }
}

// StringRule with return type bool
public class StringRule : Rule<string, bool>
{
    public StringRule(string name, IEnumerable<string> criteria, ComparisonType comparison, MatchType match)
        : base(name, value =>
        {
            var matches = criteria.Select(criterion =>
                comparison switch
                {
                    ComparisonType.FullString => value.Equals(criterion, StringComparison.OrdinalIgnoreCase),
                    ComparisonType.Substring => value.Contains(criterion, StringComparison.OrdinalIgnoreCase),
                    ComparisonType.StartsWith => value.StartsWith(criterion, StringComparison.OrdinalIgnoreCase),
                    ComparisonType.EndsWith => value.EndsWith(criterion, StringComparison.OrdinalIgnoreCase),
                    _ => false
                }).ToList();

            return match switch
            {
                MatchType.MatchAll => matches.All(m => m),
                MatchType.MatchAny => matches.Any(m => m),
                MatchType.MatchNone => matches.All(m => !m),
                _ => false
            };
        })
    { }
}

// NumberRule with a customizable return type
public class NumberRule<R> : Rule<int, R>
{
    public NumberRule(string name, Func<int, R> evaluationFunction)
        : base(name, evaluationFunction)
    { }
}

public class DateTimeRule : Rule<DateTime, string>
{
    public DateTimeRule(string name, Func<DateTime, string> evaluationFunction)
        : base(name, evaluationFunction)
    { }
}

public class TaxonomyRule : Rule<string, string>
{
    private readonly List<SubRule> _subRules = new();

    public TaxonomyRule(string name) : base(name, null)
    {
        EvaluationFunction = Evaluate;
    }

    public void AddSubRule(IEnumerable<string> criteria, MatchType matchType, string result, ComparisonType comparisonType)
    {
        _subRules.Add(new SubRule
        {
            Criteria = criteria.ToList(),
            MatchType = matchType,
            Result = result,
            ComparisonType = comparisonType
        });
    }

    private string Evaluate(string value)
    {
        foreach (var subRule in _subRules)
        {
            var matches = subRule.Criteria.Select(criterion =>
                subRule.ComparisonType switch
                {
                    ComparisonType.FullString => value.Equals(criterion, StringComparison.OrdinalIgnoreCase),
                    ComparisonType.Substring => value.Contains(criterion, StringComparison.OrdinalIgnoreCase),
                    ComparisonType.StartsWith => value.StartsWith(criterion, StringComparison.OrdinalIgnoreCase),
                    ComparisonType.EndsWith => value.EndsWith(criterion, StringComparison.OrdinalIgnoreCase),
                    _ => false
                }).ToList();

            var isMatch = subRule.MatchType switch
            {
                MatchType.MatchAll => matches.All(m => m),
                MatchType.MatchAny => matches.Any(m => m),
                MatchType.MatchNone => matches.All(m => !m),
                _ => false
            };

            if (isMatch)
            {
                return subRule.Result; // Stop evaluating further rules
            }
        }

        return null; // No match found
    }


}


public class TaxonomyRule2 : Rule<string, List<string>>
{
    private readonly List<SubRule> _subRules = new();

    public TaxonomyRule2(string name) : base(name, null)
    {
        EvaluationFunction = Evaluate;
    }

    public void AddSubRule(IEnumerable<string> criteria, MatchType matchType, string result, ComparisonType comparisonType)
    {
        _subRules.Add(new SubRule
        {
            Criteria = criteria.ToList(),
            MatchType = matchType,
            Result = result,
            ComparisonType = comparisonType
        });
    }

    private List<string> Evaluate(string value)
    {
        var matches = new List<string>();

        foreach (var subRule in _subRules)
        {
            var criteriaMatches = subRule.Criteria.Select(criterion =>
                subRule.ComparisonType switch
                {
                    ComparisonType.FullString => value.Equals(criterion, StringComparison.OrdinalIgnoreCase),
                    ComparisonType.Substring => value.Contains(criterion, StringComparison.OrdinalIgnoreCase),
                    ComparisonType.StartsWith => value.StartsWith(criterion, StringComparison.OrdinalIgnoreCase),
                    ComparisonType.EndsWith => value.EndsWith(criterion, StringComparison.OrdinalIgnoreCase),
                    _ => false
                }).ToList();

            var isMatch = subRule.MatchType switch
            {
                MatchType.MatchAll => criteriaMatches.All(m => m),
                MatchType.MatchAny => criteriaMatches.Any(m => m),
                MatchType.MatchNone => criteriaMatches.All(m => !m),
                _ => false
            };

            if (isMatch)
            {
                matches.Add(subRule.Result);
            }
        }

        return matches;
    }


}

private class SubRule
{
    public List<string> Criteria { get; set; }
    public MatchType MatchType { get; set; }
    public string Result { get; set; }
    public ComparisonType ComparisonType { get; set; }
}

// Enumerations
public enum ComparisonType
{
    FullString,
    Substring,
    StartsWith,
    EndsWith
}

public enum MatchType
{
    MatchAll,
    MatchAny,
    MatchNone
}

// Example Usage
public class Program
{
    public static void Main()
    {
        var rulesManager = new RulesManager();

        // Add a string rule with a boolean return type
        rulesManager.AddRule(new StringRule(
            "DomainCheck",
            new[] { "google.com", "microsoft.com" },
            ComparisonType.FullString,
            MatchType.MatchAny
        ));

        rulesManager.AddRule(new StringRule(
            "EmailDomain",
            new[] { ""@outlook.com" },
            ComparisonType.EndsWith,
            MatchType.MatchAll
        ));


        // Add a numeric rule with a string return type
        rulesManager.AddRule(new NumberRule<string>(
            "ColorGrade",
            num => num switch
            {
                <= 50 => "red",
                <= 85 => "yellow",
                _ => "green"
            }
        ));

        // Add a numeric rule with a boolean return type
        rulesManager.AddRule(new NumberRule<bool>(
            "GreaterThan50",
            num => num > 50
        ));


        // Adding a DateTime rule
        rulesManager.AddRule(new DateTimeRule(
            "WeekendCheck",
            date => date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday ? "Weekend" : "Weekday"
        ));

        // Evaluate the DateTime rule
        Console.WriteLine(rulesManager.Evaluate<DateTime, string>("WeekendCheck", DateTime.Now)); // Weekday or Weekend


        // Evaluate the rules
        Console.WriteLine(rulesManager.Evaluate<string, bool>("DomainCheck", "google.com")); // True
        Console.WriteLine(rulesManager.Evaluate<string, bool>("EmailDomain", "msn.com")); // False
        Console.WriteLine(rulesManager.Evaluate<string, bool>("EmailDomain", "outlook.com")); // True
        Console.WriteLine(rulesManager.Evaluate<int, string>("ColorGrade", 80));           // yellow
        Console.WriteLine(rulesManager.Evaluate<int, bool>("GreaterThan50", 45));         // False



        SubRulesExample();
        SubRules2Example();
    }

    public static void SubRulesExample()
    {
        var rulesManager = new RulesManager();

        // Add a TaxonomyRule
        var taxonomyRule = new TaxonomyRule("Taxonomy");

        // Sub-rule 1: MatchAny with Substring
        taxonomyRule.AddSubRule(new[] { "red", "blue", "green" }, MatchType.MatchAny, "color", ComparisonType.Substring);

        // Sub-rule 2: MatchAll with FullString
        taxonomyRule.AddSubRule(new[] { "square", "triangle" }, MatchType.MatchAll, "shape", ComparisonType.FullString);

        // Sub-rule 3: MatchAny with StartsWith
        taxonomyRule.AddSubRule(new[] { "bmw", "audi", "merc" }, MatchType.MatchAny, "car brands", ComparisonType.StartsWith);

        // Add the taxonomy rule to the rules manager
        rulesManager.AddRule(taxonomyRule);

        // Evaluate the taxonomy rule
        Console.WriteLine(rulesManager.Evaluate<string, string>("Taxonomy", "red"));       // color
        Console.WriteLine(rulesManager.Evaluate<string, string>("Taxonomy", "square"));   // null (needs both "square" and "triangle")
        Console.WriteLine(rulesManager.Evaluate<string, string>("Taxonomy", "triangle")); // null (needs both "square" and "triangle")
        Console.WriteLine(rulesManager.Evaluate<string, string>("Taxonomy", "bmw320i"));  // car brands
        Console.WriteLine(rulesManager.Evaluate<string, string>("Taxonomy", "audiA6"));   // car brands
        Console.WriteLine(rulesManager.Evaluate<string, string>("Taxonomy", "mercBenz")); // car brands

    }

    public static void SubRules2Example()
    {
        var rulesManager = new RulesManager();

    // Add a TaxonomyRule2
    var taxonomyRule = new TaxonomyRule2("Taxonomy2");

    // Sub-rule 1: MatchAny with Substring
    taxonomyRule.AddSubRule(new[] { "red", "blue", "green" }, MatchType.MatchAny, "color", ComparisonType.Substring);

    // Sub-rule 2: MatchAll with FullString
    taxonomyRule.AddSubRule(new[] { "square", "triangle" }, MatchType.MatchAll, "shape", ComparisonType.FullString);

    // Sub-rule 3: MatchAny with StartsWith
    taxonomyRule.AddSubRule(new[] { "bmw", "audi", "merc" }, MatchType.MatchAny, "car brands", ComparisonType.StartsWith);

    // Add the taxonomy rule to the rules manager
    rulesManager.AddRule(taxonomyRule);

    // Evaluate the taxonomy rule
    Console.WriteLine(string.Join(", ", rulesManager.Evaluate<string, List<string>>("Taxonomy2", "red")));        // color
    Console.WriteLine(string.Join(", ", rulesManager.Evaluate<string, List<string>>("Taxonomy2", "square")));    // shape
    Console.WriteLine(string.Join(", ", rulesManager.Evaluate<string, List<string>>("Taxonomy2", "bmw320i")));   // car brands
    Console.WriteLine(string.Join(", ", rulesManager.Evaluate<string, List<string>>("Taxonomy2", "blue bmw")));  // color, car brands

    }

}
