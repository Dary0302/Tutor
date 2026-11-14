using NUnit.Framework;
using Tutor.Application.Mentors;
using Tutor.Validators;

namespace Tutor.Tests.Api;

public sealed class MentorSearchRequestValidatorTests
{
    [Test]
    public void RejectsMalformedFilters()
    {
        var result = new MentorSearchRequestValidator().Validate(new MentorSearchRequest
        {
            SkillIds = [Guid.Empty], DurationMonths = [0], DaysOfWeek = [(DayOfWeek)7],
            MinYearsExperience = -1, MaxYearsExperience = 101, MinHoursPerWeek = -1,
            MaxHoursPerWeek = 169, MinRating = 5.1m, Page = int.MaxValue, Sort = (MentorSort)100,
            PlaceOfWork = new string('a', 256)
        });
        Assert.That(result.Errors.Select(e => e.PropertyName), Is.EquivalentTo(new[]
        {
            "SkillIds[0]", "DurationMonths[0]", "DaysOfWeek[0]", "MinYearsExperience", "MaxYearsExperience",
            "MinHoursPerWeek", "MaxHoursPerWeek", "MinRating", "Page", "Sort", "PlaceOfWork"
        }));
        Assert.That(new MentorSearchRequestValidator().Validate(new MentorSearchRequest()).IsValid, Is.True);
    }

    [Test]
    public void RejectsReversedRanges()
    {
        var result = new MentorSearchRequestValidator().Validate(new MentorSearchRequest
        {
            MinYearsExperience = 10, MaxYearsExperience = 1, MinHoursPerWeek = 5, MaxHoursPerWeek = 1
        });
        Assert.That(result.Errors.Select(e => e.PropertyName),
            Is.EquivalentTo(new[] { "MaxYearsExperience", "MaxHoursPerWeek" }));
    }

    [TestCase(0, 10, false)]
    [TestCase(1, 0, false)]
    [TestCase(1, 101, false)]
    [TestCase(int.MaxValue, 100, false)]
    [TestCase(1, 1, true)]
    [TestCase(1, 100, true)]
    [TestCase(int.MaxValue, 1, true)]
    [TestCase(21474837, 100, true)]
    public void ValidatesPaginationBoundaries(int page, int pageSize, bool isValid)
    {
        var result = new MentorSearchRequestValidator().Validate(new MentorSearchRequest
        {
            Page = page, PageSize = pageSize
        });
        Assert.That(result.IsValid, Is.EqualTo(isValid));
    }

    [Test]
    public void RejectsNullCollectionsAndInvalidSort()
    {
        var result = new MentorSearchRequestValidator().Validate(new MentorSearchRequest
        {
            SkillIds = null!, DaysOfWeek = null!, DurationMonths = null!, Sort = (MentorSort)999
        });
        Assert.That(result.Errors.Select(e => e.PropertyName),
            Is.EquivalentTo(new[] { "SkillIds", "DaysOfWeek", "DurationMonths", "Sort" }));
    }
}
